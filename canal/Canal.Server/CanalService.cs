using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Canal.Model;
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
        private ICanalConnector _canalConnector;
        private bool _isDispose = false;
        private readonly IServiceScope _scope;
        private readonly IConfiguration _configuration;
        private readonly List<System.Type> _registerTypeList;

        private readonly ConcurrentQueue<long> _queue = new ConcurrentQueue<long>();
        private readonly ConcurrentQueue<CanalQueueData> _canalQueue = new ConcurrentQueue<CanalQueueData>();

        private volatile bool _resetFlag = false;
        private readonly AutoResetEvent _condition = new AutoResetEvent(false);
        public CanalService(ILogger<CanalService> logger, IOptions<CanalOption> canalOption, IServiceScopeFactory scopeFactory, IConfiguration configuration, CanalConsumeRegister register)
        {
            _logger = logger;
            _registerTypeList = new List<System.Type>();
            if (register.SingletonConsumeList != null && register.SingletonConsumeList.Any()) _registerTypeList.AddRange(register.SingletonConsumeList);
            if (register.ConsumeList != null && register.ConsumeList.Any()) _registerTypeList.AddRange(register.ConsumeList);
            if (!_registerTypeList.Any())
            {
                throw new ArgumentNullException(nameof(_registerTypeList));
            }

            _configuration = configuration;
            _canalOption = canalOption?.Value;
            if (_canalOption == null)
            {
                _canalOption = new CanalOption();
            }

            UpdateFromEnv(_canalOption);

            if (string.IsNullOrEmpty(_canalOption.Host) || string.IsNullOrEmpty(_canalOption.Destination) ||
                string.IsNullOrEmpty(_canalOption.MysqlName)
                || string.IsNullOrEmpty(_canalOption.MysqlPwd) || _canalOption.Port < 1 || _canalOption.GetCountsPerTimes < 1)
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
               
                AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;

                _logger.LogInformation("canal client start ...");
                LazyCanalGetEntities();
                LazyCanalDoWork();
                CanalServerAckStart();
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
                _canalConnector.Disconnect();
                _logger.LogInformation("canal client stop success...");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "canal client stop error...");
            }
            _canalConnector = null;
            _scope.Dispose();
        }

        private void CurrentDomainOnProcessExit(object sender, EventArgs e)
        {
            Dispose();
        }

        /// <summary>
        /// 异步ack
        /// </summary>
        private void CanalServerAckStart()
        {
            new Thread(() =>
            {
                _logger.LogInformation("canal-server ack worker thread start ...");
                while (!_isDispose)
                {
                    try
                    {
                        if (_canalConnector == null) continue;
                        if(!_queue.TryDequeue(out var batchId)) continue;
                        if (batchId > 0)
                        {

                            _canalConnector.Ack(batchId); //如果程序突然关闭 cannal service 会关闭。这里就不会提交，下次重启应用消息会重复推送！
                        }
                    }
                    catch (Exception)
                    {
                        //ignore
                    }
                }
            }).Start();
        }


        /// <summary>
        /// 异步 获取数据
        /// </summary>
        private void LazyCanalGetEntities()
        {
            new Thread(() =>
            {
                _logger.LogInformation("canal receive worker thread start ...");
                while (!_isDispose)
                {
                    try
                    {
                        if (!PreparedAndEnqueue()) continue;

                        //队列里面先储备5批次 超过 5批次的话 就开始消费一个批次 在储备
                        if (_canalQueue.Count >= 5)
                        {
                            _logger.LogInformation("canal receive worker waitOne ...");
                            _resetFlag = true;
                            _condition.WaitOne();
                            _resetFlag = false;
                            _logger.LogInformation("canal receive worker continue ...");
                        }
                    }
                    catch (IOException io)
                    {
                        _logger.LogError(io, "canal receive data err ...");
                        try
                        {
                            _canalConnector.Connect();
                            _canalConnector.Subscribe(_canalOption.Subscribe);
                        }
                        catch (Exception)
                        {
                            //ignore
                        }
                    }
                    catch (Exception e)
                    {
                        //ignore
                        _logger.LogError(e,"canal receive data err ...");
                    }
                }
            }).Start();
        }

        private bool PreparedAndEnqueue()
        {
            if (_canalConnector == null) return false;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var messageList = _canalConnector.GetWithoutAck(_canalOption.GetCountsPerTimes);
            var batchId = messageList.Id;
            if (batchId < 1)
            {
                return false;
            }

            CanalBody body = new CanalBody(null, batchId);
            if (messageList.Entries.Count <= 0)
            {
                _canalQueue.Enqueue(new CanalQueueData
                {
                    CanalBody = body
                });
                return false;
            }

            var canalBody = GetCanalBodyList(messageList.Entries, batchId);
            if (canalBody.Message==null || canalBody.Message.Count < 1)
            {
                _canalQueue.Enqueue(new CanalQueueData
                {
                    CanalBody = body
                });
                return false;
            }

            stopwatch.Stop();

            var doutime = (int) stopwatch.Elapsed.TotalSeconds;
            var time = doutime > 1 ? ParseTimeSeconds(doutime) : stopwatch.Elapsed.TotalMilliseconds + "ms";

            var canalQueueData = new CanalQueueData
            {
                Time = time,
                CanalBody = canalBody
            };

            _canalQueue.Enqueue(canalQueueData);

            return true;
        }


        private void LazyCanalDoWork()
        {
            new Thread(() =>
            {
                _logger.LogInformation("handler worker thread start ...");
                while (!_isDispose)
                {
                    try
                    {
                        if (_canalConnector == null) continue;

                        if(!_canalQueue.TryDequeue(out var canalData)) continue;

                        if (canalData == null) continue;


                        if (_resetFlag && _canalQueue.Count <= 1)
                        {
                            _condition.Set();
                            _resetFlag = false;
                        }

                       
                        if (canalData.CanalBody.Message == null)
                        {
                            if(canalData.CanalBody.BatchId > 0) _queue.Enqueue(canalData.CanalBody.BatchId);
                            continue;
                        }

                        _logger.LogInformation($"【batchId:{canalData.CanalBody.BatchId}】batchCount:{canalData.CanalBody.Message.Count},batchGetTime:{canalData.Time}");

                        Send(canalData.CanalBody);
                        _queue.Enqueue(canalData.CanalBody.BatchId);
                    }
                    catch (Exception)
                    {
                        //ignore
                        return;
                    }
                }
            }).Start();
        }

        /// <summary>
        /// 发送数据
        /// 如果handler处理失败就停止 保证消息
        /// </summary>
        /// <param name="canalBody">一个entry表示一个数据库变更</param>
        private void Send(CanalBody canalBody)
        {

            try
            {
                foreach (var type in _registerTypeList)
                {
                    var service = _scope.ServiceProvider.GetRequiredService(type) as INotificationHandler<CanalBody>;
                    service?.Handle(canalBody).ConfigureAwait(false).GetAwaiter().GetResult();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "canal produce error,end process!");
                Dispose();
                return;
            }

           
        }


        private CanalBody GetCanalBodyList(List<Entry> entrys,long batchId)
        {
            var result = new List<DataChange>();
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
                            dataChange.BeforeColumnList = rowData.BeforeColumns.ToList(); //DoConvertDataColumn();
                        }
                        else if (eventType == EventType.Insert)
                        {
                            dataChange.EventType = "INSERT";
                            dataChange.AfterColumnList = rowData.AfterColumns.ToList();// DoConvertDataColumn();
                        }
                        else if (eventType == EventType.Update)
                        {
                            dataChange.EventType = "UPDATE";
                            dataChange.BeforeColumnList = rowData.BeforeColumns.ToList();//DoConvertDataColumn();
                            dataChange.AfterColumnList = rowData.AfterColumns.ToList(); //DoConvertDataColumn();
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

                        result.Add(dataChange);
                    }
                }
            }

            return new CanalBody(result, batchId);
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

}
