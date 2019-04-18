using CanalSharp.Client;
using CanalSharp.Client.Impl;
using Com.Alibaba.Otter.Canal.Protocol;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MysqlCanalMq.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AntData.ORM.Reflection;
using AspectCore.Extensions.Reflection;
using MysqlCanalMq.Common.RabitMQ;
using MysqlCanalMq.Common.StaticExt;
using MysqlCanalMq.Models.canal;
using Newtonsoft.Json;

namespace MysqlCanalMq.Canal
{
    public class CanalService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly CanalOption _canalOption;
        private readonly RabitMqOption _rabitMqOption;
        private ICanalConnector _canalConnector;
        private System.Threading.Timer _canalTimer;
        private IProduceRabitMq _produceRabitMq;
        public CanalService(ILogger<CanalService> logger, IOptions<CanalOption> canalOption, IOptions<RabitMqOption> rabitMqOption)
        {
            _logger = logger;
            _canalOption = canalOption?.Value;

            if (_canalOption == null)
            {
                throw new ArgumentNullException("Canal in appsettings.json is empty!");
            }

            if (string.IsNullOrEmpty(_canalOption.Host) || string.IsNullOrEmpty(_canalOption.Destination) ||
                string.IsNullOrEmpty(_canalOption.MysqlName)
                || string.IsNullOrEmpty(_canalOption.MysqlPwd) || _canalOption.Port < 1 || _canalOption.Timer < 1 || _canalOption.GetCountsPerTimes < 1)
            {
                throw new ArgumentNullException("Canal param in appsettings.json is not correct!");
            }


            if (rabitMqOption == null)
            {
                throw new ArgumentNullException("Rabit in appsettings.json is empty!");
            }

            _rabitMqOption = rabitMqOption?.Value;
            if (string.IsNullOrEmpty(_rabitMqOption.Host) || string.IsNullOrEmpty(_rabitMqOption.UserName) ||
                string.IsNullOrEmpty(_rabitMqOption.Password) || _rabitMqOption.Port < 1)
            {
                throw new ArgumentNullException("Rabit param in appsettings.json is not correct!");
            }

        }



        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _produceRabitMq = RabitMqFactory.CreateProduceRabitMq(_rabitMqOption);
                _canalConnector = CanalConnectors.NewSingleConnector(_canalOption.Host, _canalOption.Port, _canalOption.Destination, _canalOption.MysqlName, _canalOption.MysqlPwd);
                _canalConnector.Connect();
                _canalConnector.Subscribe(".*\\..*");
                _canalConnector.Rollback();
                _canalTimer = new System.Threading.Timer(CanalGetData, null, _canalOption.Timer * 1000, _canalOption.Timer * 1000);
                _logger.LogInformation("canal client start success...");
            }
            catch (Exception ex)
            {
                _logger.LogError("canal client start error...", ex);
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _canalTimer.Change(-1, -1);
                _canalConnector.Disconnect();
                _logger.LogInformation("canal client stop success...");
            }
            catch (Exception ex)
            {
                _logger.LogError("canal client stop error...", ex);
            }
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _canalConnector = null;
            _canalTimer.Dispose();
        }


        /// <summary>
        /// 获取数据
        /// </summary>
        private void CanalGetData(object state)
        {
            _canalTimer.Change(-1, -1);
            try
            {
                if (_canalConnector == null) return;
                var messageList = _canalConnector.GetWithoutAck(_canalOption.GetCountsPerTimes);
                var batchId = messageList.Id;
                if (batchId == -1 || messageList.Entries.Count <= 0)
                {
                    return;
                }

                SendToMQ(messageList.Entries, batchId);
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
        /// 输出数据
        /// </summary>
        /// <param name="entrys">一个entry表示一个数据库变更</param>
        /// <param name="batchId"></param>
        private void SendToMQ(List<Entry> entrys, long batchId)
        {
            var batchSuccess = true;
            foreach (var entry in entrys)
            {
                if (entry.EntryType == EntryType.Transactionbegin || entry.EntryType == EntryType.Transactionend)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(entry.Header.SchemaName) || string.IsNullOrEmpty(entry.Header.TableName)) continue;

                //排除不要监听的表
                if (_canalOption.Tables.Any() && !_canalOption.Tables.Contains(entry.Header.TableName))
                {
                    continue;
                }


                RowChange rowChange = null;

                try
                {
                    //获取行变更
                    rowChange = RowChange.Parser.ParseFrom(entry.StoreValue);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.ToString());
                }

                if (rowChange != null)
                {
                    //变更类型 insert/update/delete 等等
                    EventType eventType = rowChange.EventType;
                    //输出binlog信息 表名 数据库名 变更类型
                    //_logger.LogInformation($"================> binlog[{entry.Header.LogfileName}:{entry.Header.LogfileOffset}] , name[{entry.Header.SchemaName},{entry.Header.TableName}] , eventType :{eventType}");

                    //var modelType = MysqlCanalMqDbInfo.GetDbModelType(entry.Header.SchemaName, entry.Header.TableName);
                    //if (modelType == null)
                    //{
                    //    continue;
                    //}



                    //输出 insert/update/delete 变更类型列数据
                    foreach (var rowData in rowChange.RowDatas)
                    {

                        var dataChange = new DataChange
                        {
                            DbName = entry.Header.SchemaName,
                            TableName = entry.Header.TableName
                        };


                        if (eventType == EventType.Delete)
                        {
                            dataChange.EventType = "DELETE";
                            dataChange.AfterColumnList = DoConvertDataColumn(rowData.AfterColumns.ToList());
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

                        try
                        {
                            _produceRabitMq.Produce(dataChange, _canalOption.Destination);

                        }
                        catch (Exception e)
                        {
                            batchSuccess = false;
                            _logger.LogError(e, "canal produce to mq error:" + JsonConvert.SerializeObject(dataChange));
                        }
                    }
                }
            }

            if (batchSuccess)
            {
                _canalConnector.Ack(batchId);
            }
        }

        private List<ColumnData> DoConvertDataColumn(List<Column> columns)
        {
            var rt = new List<ColumnData>();
            foreach (var c in columns)
            {
                var co = new ColumnData();
                co.Name = c.Name;
                co.IsKey = c.IsKey;
                co.IsNull = c.IsNull;
                co.Length = c.Length;
                co.MysqlType = c.MysqlType;
                co.Updated = c.Updated;
                co.Value = c.Value;
                rt.Add(co);
            }

            return rt;

        }




        private object GetDbModel(System.Type type, IList<Column> cols)
        {
            try
            {
                if (cols == null || cols.Count < 1)
                {
                    return null;
                }
                var constructorInfo = type.GetTypeInfo().GetConstructor(new System.Type[0]);
                var reflector = constructorInfo.GetReflector();
                var obj = reflector.Invoke();
                var pros = type.GetCanWritePropertyInfo().ToDictionary(r => r.Name.ToLower(), y => y);
                if (pros.Count < 1)
                {
                    _logger.LogWarning("CanalService.ParseDbModel", $"类型:{type.Name} 没有可写的property");
                    return null;
                }
                foreach (var c in cols)
                {
                    var key = c.Name.ToLower();

                    if (!pros.ContainsKey(key))
                    {
                        continue;
                    }

                    var prop = pros[key];
                    var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    if (c.IsNull)
                    {
                        if (prop.ToString().Contains("System.Nullable"))
                        {
                            prop.FastSetValue(obj, null);
                        }
                        continue;
                    }
                    var value = TypeConvertUtils.Parse(c.Value, propType);
                    prop.FastSetValue(obj, value);
                }
                return obj;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("CanalService.ParseDbModel", ex);
                return null;
            }
        }
    }



}
