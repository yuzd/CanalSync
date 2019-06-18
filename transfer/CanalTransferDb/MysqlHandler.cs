using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Canal.Model;
using Canal.Server.Interface;
using Canal.Server.Models;
using Canal.SqlParse;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CanalTransferDb
{

    public class MysqlHandler : INotificationHandler<CanalBody>
    {
        private readonly ILogger _logger;
        private readonly IDbTransfer _dbTypeMapper;
        private readonly MysqlOption _option;
        private readonly IConfiguration _configuration;

        public MysqlHandler(ILogger<MysqlHandler> logger,IOptions<MysqlOption> options,IDbTransfer dbTypeMapper, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _dbTypeMapper = dbTypeMapper;
            _option = options?.Value;
            if (_option == null)
            {
                _option = new MysqlOption();
            }

            if (_option.DbTables == null)
            {
                _option.DbTables = new List<string>();
            }

            UpdateFromEnv(_option);

            _logger.LogInformation(_option.DbTables.Any()
                ? $"Mysql Produce Listening: {string.Join(",", _option.DbTables)}"
                : $"Mysql Produce Listening");
        }
        public Task Handle(CanalBody body)
        {
            var notificationList = body.Message;
            var metric = new Dictionary<string, MetricCount>();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            foreach (var message in notificationList)
            {
                //过滤
                if (_option.DbTables!=null && !_option.DbTables.Contains(message.DbName + "." + message.TableName))
                {
                    IncrementMetric(metric, message, false);
                    continue;
                }

                var result = _dbTypeMapper.TransferToDb(message);
                if (!result.Success)
                {
                    _logger.LogError($"Topic:{message.CanalDestination + "." + message.DbName + "." + message.TableName},Message:{Newtonsoft.Json.JsonConvert.SerializeObject(message)},Error:{result.Msg}");
                }
                else
                {
                    IncrementMetric(metric, message);
                }
            }
            stopwatch.Stop();
            var doutime = (int)stopwatch.Elapsed.TotalSeconds;
            var time = doutime > 1 ? ParseTimeSeconds(doutime) : stopwatch.Elapsed.TotalMilliseconds + "ms";
            _logger.LogInformation($"【batchId:{body.BatchId}】processTime:{time}");
            foreach (var item in metric)
            {
                _logger.LogInformation($"batchId:{body.BatchId},process:{this.GetType().FullName},target:{item.Key},total:{item.Value.Total},success:{item.Value.SuccCount}");
            }
            return Task.CompletedTask;
        }


        private void IncrementMetric(Dictionary<string, MetricCount> result, DataChange dataChange,bool success = true)
        {
            var key = $"{dataChange.DbName}.{dataChange.TableName}.{dataChange.EventType}";
            if (!result.ContainsKey(key))
            {
                result[key] = new MetricCount();
            }
            var target = result[key];
            target.Total++;//总数
            if (success)
            {
                target.SuccCount++;//成功数
            }
        }

        private void UpdateFromEnv(MysqlOption _rab)
        {
            var dbTables = _configuration["mysql.dbTables"];
            if (!string.IsNullOrEmpty(dbTables))
            {
                _rab.DbTables = dbTables.Split(':').ToList();
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

    internal class MetricCount
    {
        public long Total { get; set; }
        public long SuccCount { get; set; }
    }
}
