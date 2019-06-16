using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Canal.Server.Interface;
using Canal.Server.Models;
using CanalSharp.Client;
using CanalSharp.Client.Impl;
using Com.Alibaba.Otter.Canal.Protocol;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Canal.Server
{
    internal class CanalService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly CanalOption _canalOption;
        private System.Threading.Timer _canalTimer;
        private ICanalConnector _canalConnector;
        private bool _isDispose = false;
        private readonly IServiceScope _scope;
        private readonly IConfiguration _configuration;
        private readonly List<System.Type> _registerTypeList;
        public CanalService(ILogger<CanalService> logger, IOptions<CanalOption> canalOption, IServiceScopeFactory scopeFactory, IConfiguration configuration, CanalConsumeRegister register)
        {
            _logger = logger;
            _registerTypeList = new List<System.Type>();
            if (register.SingletonConsumeList != null && register.SingletonConsumeList.Any()) _registerTypeList.AddRange(register.SingletonConsumeList);
            if (register.ConsumeList != null && register.ConsumeList.Any()) _registerTypeList.AddRange(register.ConsumeList);
            _configuration = configuration;
            _canalOption = canalOption?.Value;
            if (_canalOption == null)
            {
                _canalOption = new CanalOption();
            }

            UpdateFromEnv(_canalOption);

            if (string.IsNullOrEmpty(_canalOption.Host) || string.IsNullOrEmpty(_canalOption.Destination) ||
                string.IsNullOrEmpty(_canalOption.MysqlName)
                || string.IsNullOrEmpty(_canalOption.MysqlPwd) || _canalOption.Port < 1 || _canalOption.Timer < 1 || _canalOption.GetCountsPerTimes < 1)
            {
                throw new ArgumentNullException("Canal param in appsettings.json is not correct!");
            }

            if (string.IsNullOrEmpty(_canalOption.Subscribe)) _canalOption.Subscribe = ".*\\..*";
            _scope = scopeFactory.CreateScope();

        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _canalConnector = CanalConnectors.NewSingleConnector(_canalOption.Host, _canalOption.Port, _canalOption.Destination, _canalOption.MysqlName, _canalOption.MysqlPwd);
                _canalConnector.Connect();
                _canalConnector.Subscribe(_canalOption.Subscribe);
                _canalConnector.Rollback();
                _canalTimer = new System.Threading.Timer(CanalGetData, null, _canalOption.Timer * 1000, _canalOption.Timer * 1000);
                _logger.LogInformation("canal client start success...");
                AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "canal client start error...");
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Dispose();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (_isDispose) return;
            _isDispose = true;
            try
            {
                _canalTimer.Change(-1, -1);
                _canalConnector.Disconnect();
                _logger.LogInformation("canal client stop success...");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "canal client stop error...");
            }
            _canalConnector = null;
            _canalTimer.Dispose();
            _scope.Dispose();
        }

        private void CurrentDomainOnProcessExit(object sender, EventArgs e)
        {
            Dispose();
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        private void CanalGetData(object state)
        {
            _canalTimer.Change(-1, -1);
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                if (_canalConnector == null) return;
                var messageList = _canalConnector.GetWithoutAck(_canalOption.GetCountsPerTimes);
                var batchId = messageList.Id;
                if (batchId == -1 || messageList.Entries.Count <= 0)
                {
                    return;
                }

                var result = Send(messageList.Entries, batchId);


                stopwatch.Stop();

                if (result.Item1 < 1) return;

                var doutime = (int)stopwatch.Elapsed.TotalSeconds;
                var time = doutime > 1 ? ParseTimeSeconds(doutime) : stopwatch.Elapsed.TotalMilliseconds + "ms";

                if (result.Item2 == null)
                {
                    return;
                }

                foreach (var item in result.Item2)
                {
                    if (item.Value.Total > 0)
                    {
                        _logger.LogInformation($"【batchId:{batchId}】batchCount:{result.Item1},batchTime:{time},process:{item.Key},processTotal:{item.Value.Total},processSucc:{item.Value.SuccessCount},processTime:{item.Value.ProcessTime}");
                        if (result.Item3 != null && result.Item3.Any())
                        {
                            foreach (var groupResult in result.Item3)
                            {
                                _logger.LogInformation($"batchId:{batchId},process:{item.Key},target:{groupResult.Item1},count:{groupResult.Item2}");
                            }
                        }
                    }
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "canal get data error...");
            }
            finally
            {
                _canalTimer.Change(_canalOption.Timer * 1000, _canalOption.Timer * 1000);
            }
        }


        /// <summary>
        /// 发送数据
        /// 如果handler处理失败就停止 保证消息
        /// </summary>
        /// <param name="entrys">一个entry表示一个数据库变更</param>
        /// <param name="batchId"></param>
        private (long, Dictionary<string, ProcessResult>,List<(string,long)>) Send(List<Entry> entrys, long batchId)
        {
            var canalBodyList = GetCanalBodyList(entrys);
            if (canalBodyList.Count < 1)
            {
                return (0, null,null);
            }


            if (_registerTypeList == null || !_registerTypeList.Any())
            {
                return (0, null,null);
            }

            var groupCount = new List<(string,long)>();
            //分组日志
            var group = canalBodyList.Select(r => r.Message).GroupBy(r => new {r.DbName, r.TableName, r.EventType});
            foreach (var g in group)
            {
                groupCount.Add(($"{g.Key.DbName}.{g.Key.TableName}.{g.Key.EventType}",g.Count()));
            }

            Dictionary<string, ProcessResult> result = new Dictionary<string, ProcessResult>();
            foreach (var re in _registerTypeList)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                result.Add(re.FullName, new ProcessResult());
            }

            try
            {
                foreach (var type in _registerTypeList)
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    // ReSharper disable once AssignNullToNotNullAttribute
                    var re = result[type.FullName];
                    foreach (var message in canalBodyList)
                    {
                        var service = _scope.ServiceProvider.GetRequiredService(type) as INotificationHandler<CanalBody>;
                        service?.Handle(message).ConfigureAwait(false).GetAwaiter().GetResult();
                        re.Total++;
                        if (message.Succ)
                        {
                            re.SuccessCount++;
                        }
                    }
                    stopwatch.Stop();
                    var doutime = (int)stopwatch.Elapsed.TotalSeconds;
                    var time = doutime > 1 ? ParseTimeSeconds(doutime) : stopwatch.Elapsed.TotalMilliseconds + "ms";
                    re.ProcessTime = time;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "canal produce error,end process!");
                Dispose();
                return (canalBodyList.Count, result, groupCount);
            }

            _canalConnector.Ack(batchId);//如果程序突然关闭 cannal service 会关闭。这里就不会提交，下次重启应用消息会重复推送！
            return (canalBodyList.Count, result, groupCount);
        }


        private List<CanalBody> GetCanalBodyList(List<Entry> entrys)
        {
            var result = new List<CanalBody>();
            foreach (var entry in entrys)
            {
                if (entry.EntryType == EntryType.Transactionbegin || entry.EntryType == EntryType.Transactionend)
                {
                    continue;
                }

                //没有拿到db名称或者表名称的直接排除
                if (string.IsNullOrEmpty(entry.Header.SchemaName) || string.IsNullOrEmpty(entry.Header.TableName)) continue;


                RowChange rowChange = null;

                try
                {
                    //获取行变更
                    rowChange = RowChange.Parser.ParseFrom(entry.StoreValue);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"DbName:{entry.Header.SchemaName},TbName:{entry.Header.TableName} RowChange.Parser.ParseFrom error");
                    continue;
                }

                if (rowChange != null)
                {
                    //变更类型 insert/update/delete 等等
                    EventType eventType = rowChange.EventType;
                    //输出binlog信息 表名 数据库名 变更类型

                    //输出 insert/update/delete 变更类型列数据
                    foreach (var rowData in rowChange.RowDatas)
                    {

                        var dataChange = new DataChange
                        {
                            DbName = entry.Header.SchemaName,
                            TableName = entry.Header.TableName,
                            CanalDestination = _canalOption.Destination
                        };

                        if (eventType == EventType.Delete)
                        {
                            dataChange.EventType = "DELETE";
                            dataChange.BeforeColumnList = DoConvertDataColumn(rowData.BeforeColumns.ToList());
                        }
                        else if (eventType == EventType.Insert)
                        {
                            dataChange.EventType = "INSERT";
                            dataChange.AfterColumnList = DoConvertDataColumn(rowData.AfterColumns.ToList());
                        }
                        else if (eventType == EventType.Update)
                        {
                            dataChange.EventType = "UPDATE";
                            dataChange.BeforeColumnList = DoConvertDataColumn(rowData.BeforeColumns.ToList());
                            dataChange.AfterColumnList = DoConvertDataColumn(rowData.AfterColumns.ToList());
                        }
                        else
                        {
                            continue;
                        }

                        var cloumns = dataChange.AfterColumnList == null || !dataChange.AfterColumnList.Any()
                            ? dataChange.BeforeColumnList
                            : dataChange.AfterColumnList;

                        var primaryKey = cloumns.FirstOrDefault(r => r.IsKey);
                        if (primaryKey == null || string.IsNullOrEmpty(primaryKey.Value))
                        {
                            //没有主键
                            _logger.LogError($"DbName: {dataChange.DbName},TbName:{dataChange.TableName} without primaryKey :" + JsonConvert.SerializeObject(dataChange));
                            continue;
                        }

                        var message = new CanalBody(dataChange);
                        result.Add(message);
                    }
                }
            }

            return result;
        }


        private List<ColumnData> DoConvertDataColumn(List<Column> columns)
        {
            var rt = new List<ColumnData>();
            foreach (var c in columns)
            {
                var co = new ColumnData
                {
                    Name = c.Name,
                    IsKey = c.IsKey,
                    IsNull = c.IsNull,
                    Length = c.Length,
                    MysqlType = c.MysqlType,
                    SqlType = c.SqlType,
                    Updated = c.Updated,
                    Value = c.Value
                };
                rt.Add(co);
            }
            return rt;

        }

        private void UpdateFromEnv(CanalOption _can)
        {
            var host = _configuration["canal.host"];
            if (!string.IsNullOrEmpty(host))
            {
                _can.Host = host;
            }

            var port = _configuration["canal.port"];
            if (!string.IsNullOrEmpty(port))
            {
                _can.Port = int.Parse(port);
            }

            var destinations = _configuration["canal.destinations"];
            if (!string.IsNullOrEmpty(destinations))
            {
                _can.Destination = destinations;
            }
            var subcribe = _configuration["canal.subscribe"];
            if (!string.IsNullOrEmpty(subcribe))
            {
                _can.Subscribe = subcribe;
            }
            var dbUsername = _configuration["canal.dbUsername"];
            if (!string.IsNullOrEmpty(dbUsername))
            {
                _can.MysqlName = dbUsername;
            }

            var dbPassword = _configuration["canal.dbPassword"];
            if (!string.IsNullOrEmpty(dbPassword))
            {
                _can.MysqlPwd = dbPassword;
            }

            var timer = _configuration["canal.timer"];
            if (!string.IsNullOrEmpty(timer))
            {
                _can.Timer = int.Parse(timer);
            }

            var getCountsPerTimes = _configuration["canal.getCountsPerTimes"];
            if (!string.IsNullOrEmpty(getCountsPerTimes))
            {
                _can.GetCountsPerTimes = int.Parse(getCountsPerTimes);
            }

        }


        ///<summary>
        ///由秒数得到日期几天几小时。。。
        ///</summary
        ///<param name="t">秒数</param>
        ///<param name="type">0：转换后带秒，1:转换后不带秒</param>
        ///<returns>几天几小时几分几秒</returns>
        private string ParseTimeSeconds(int t, int type = 0)
        {
            string r = "";
            int day, hour, minute, second;
            if (t >= 86400) //天,
            {
                day = Convert.ToInt16(t / 86400);
                hour = Convert.ToInt16((t % 86400) / 3600);
                minute = Convert.ToInt16((t % 86400 % 3600) / 60);
                second = Convert.ToInt16(t % 86400 % 3600 % 60);
                if (type == 0)
                    r = day + ("D") + hour + ("H") + minute + ("M") + second + ("S");
                else
                    r = day + ("D") + hour + ("H") + minute + ("M");

            }
            else if (t >= 3600)//时,
            {
                hour = Convert.ToInt16(t / 3600);
                minute = Convert.ToInt16((t % 3600) / 60);
                second = Convert.ToInt16(t % 3600 % 60);
                if (type == 0)
                    r = hour + ("H") + minute + ("M") + second + ("S");
                else
                    r = hour + ("H") + minute + ("M");
            }
            else if (t >= 60)//分
            {
                minute = Convert.ToInt16(t / 60);
                second = Convert.ToInt16(t % 60);
                r = minute + ("M") + second + ("S");
            }
            else
            {
                second = Convert.ToInt16(t);
                r = second + ("S");
            }
            return r;
        }
    }

    class  ProcessResult
    {
        public long Total { get; set; }
        public long SuccessCount { get; set; }
        public string ProcessTime { get; set; }
    }
}
