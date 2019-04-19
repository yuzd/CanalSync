using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CanalSharp.Client;
using CanalSharp.Client.Impl;
using Com.Alibaba.Otter.Canal.Protocol;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MysqlCanalMq.Canal.OutPut;
using MysqlCanalMq.Common.Models.canal;
using MysqlCanalMq.Models;
using Newtonsoft.Json;

namespace MysqlCanalMq.Canal
{
    public class CanalService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly CanalOption _canalOption;
        private System.Threading.Timer _canalTimer;
        private ICanalConnector _canalConnector;
        private bool _isDispose = false;
        private readonly IServiceScope _scope;
        private readonly IMediator _mediator;
        public CanalService(ILogger<CanalService> logger, IOptions<CanalOption> canalOption, IServiceScopeFactory scopeFactory)
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

            _scope = scopeFactory.CreateScope();
            _mediator = _scope.ServiceProvider.GetRequiredService<IMediator>();
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _canalConnector = CanalConnectors.NewSingleConnector(_canalOption.Host, _canalOption.Port, _canalOption.Destination, _canalOption.MysqlName, _canalOption.MysqlPwd);
                _canalConnector.Connect();
                _canalConnector.Subscribe(".*\\..*");
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
                if (_canalConnector == null) return;
                var messageList = _canalConnector.GetWithoutAck(_canalOption.GetCountsPerTimes);
                var batchId = messageList.Id;
                if (batchId == -1 || messageList.Entries.Count <= 0)
                {
                    return;
                }

                Send(messageList.Entries, batchId);
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
        private void Send(List<Entry> entrys, long batchId)
        {
            var count = 0;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            foreach (var entry in entrys)
            {
                if (entry.EntryType == EntryType.Transactionbegin || entry.EntryType == EntryType.Transactionend)
                {
                    continue;
                }

                //没有拿到db名称或者表名称的直接排除
                if (string.IsNullOrEmpty(entry.Header.SchemaName) || string.IsNullOrEmpty(entry.Header.TableName)) continue;

                ////排除不要监听的表
                //if (_canalOption.DbTables.Any() && !_canalOption.DbTables.Contains(entry.Header.TableName))
                //{
                //    continue;
                //}


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

                        try
                        {
                            _mediator.Publish(new CanalBody(dataChange)).ConfigureAwait(false).GetAwaiter().GetResult();
                            count++;
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "canal produce error:" + JsonConvert.SerializeObject(dataChange));
                            Dispose();
                            return;
                        }
                    }
                }
            }

            _canalConnector.Ack(batchId);
            stopwatch.Stop();
            var doutime = (int)stopwatch.Elapsed.TotalSeconds;
            _logger.LogInformation($"batchId:{batchId},count:{count},time:{(doutime > 300 ? ((int)stopwatch.Elapsed.TotalMinutes) + "分" : doutime + "秒")} send to mq success");
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
    }
}
