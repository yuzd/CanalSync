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
using System.Threading;
using System.Threading.Tasks;

namespace MysqlCanalMq.Canal
{
    public class CanalService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly CanalOption _canalOption;
        private ICanalConnector _canalConnector;
        private System.Threading.Timer _canalTimer;

        public CanalService(ILogger<CanalService> logger, IOptions<CanalOption> canalOption)
        {
            _logger = logger;
            _canalOption = canalOption?.Value;
            if (_canalOption == null)
            {
                throw new ArgumentNullException("Canal in appsettings.json is empty!");
            }

            if (string.IsNullOrEmpty(_canalOption.Host) || string.IsNullOrEmpty(_canalOption.Destination) ||
                string.IsNullOrEmpty(_canalOption.MysqlName)
                || string.IsNullOrEmpty(_canalOption.MysqlPwd) || _canalOption.Port < 1)
            {
                throw new ArgumentNullException("Canal param in appsettings.json is not correct!");
            }
        }



        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _canalConnector = CanalConnectors.NewSingleConnector(_canalOption.Host, _canalOption.Port, _canalOption.Destination, _canalOption.MysqlName, _canalOption.MysqlPwd);
                _canalConnector.Connect();
                _canalConnector.Subscribe(".*\\..*");
                _canalTimer = new System.Threading.Timer(CanalGetData, null, 1000, 1000);
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
                var messageList = _canalConnector.Get(1000);
                var batchId = messageList.Id;
                if (batchId == -1 || messageList.Entries.Count <= 0)
                {
                    return;
                }

                PrintEntry(messageList.Entries);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError("canal get data error...", ex);
            }
            finally
            {
                _canalTimer.Change(1000, 1000);
            }
        }

        /// <summary>
        /// 输出数据
        /// </summary>
        /// <param name="entrys">一个entry表示一个数据库变更</param>
        private void PrintEntry(List<Entry> entrys)
        {
            foreach (var entry in entrys)
            {
                if (entry.EntryType == EntryType.Transactionbegin || entry.EntryType == EntryType.Transactionend)
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
                    _logger.LogInformation($"================> binlog[{entry.Header.LogfileName}:{entry.Header.LogfileOffset}] , name[{entry.Header.SchemaName},{entry.Header.TableName}] , eventType :{eventType}");

                    //输出 insert/update/delete 变更类型列数据
                    foreach (var rowData in rowChange.RowDatas)
                    {
                        if (eventType == EventType.Delete)
                        {
                            PrintColumn(rowData.BeforeColumns.ToList());
                        }
                        else if (eventType == EventType.Insert)
                        {
                            PrintColumn(rowData.AfterColumns.ToList());
                        }
                        else
                        {
                            _logger.LogInformation("-------> before");
                            PrintColumn(rowData.BeforeColumns.ToList());
                            _logger.LogInformation("-------> after");
                            PrintColumn(rowData.AfterColumns.ToList());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 输出每个列的详细数据
        /// </summary>
        /// <param name="columns"></param>
        private static void PrintColumn(List<Column> columns)
        {
            foreach (var column in columns)
            {
                //输出列明 列值 是否变更
                Console.WriteLine($"{column.Name} ： {column.Value}  update=  {column.Updated}");
            }
        }

    }
}
