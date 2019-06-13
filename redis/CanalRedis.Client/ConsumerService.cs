using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AntData.ORM.Data;
using Canal.SqlParse;
using Canal.SqlParse.Models;
using Canal.SqlParse.Models.canal;
using Canal.SqlParse.StaticExt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CanalRedis.Client
{
    public class ConsumerService : IHostedService,IDisposable
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IDbTypeMapper _dbTypeMapper;
        private readonly DbContext<DB> _dbContext;

        private readonly IDatabase Redis;
        private readonly ConnectionMultiplexer _conn;
        private readonly List<string> _topicList = new List<string>();
        public ConsumerService(ILogger<ConsumerService> logger, IOptions<RedisOption> redisOptions,DbContext<DB> dbContext, IConfiguration configuration, IDbTypeMapper dbTypeMapper)
        {
            _logger = logger;
            _configuration = configuration;
            _dbTypeMapper = dbTypeMapper;
            _dbContext = dbContext;
            var redisOptions1 = redisOptions?.Value;
            if (redisOptions1 == null)
            {
                redisOptions1 = new RedisOption();
            }

            UpdateFromEnv(redisOptions1);
            
            if (string.IsNullOrEmpty(redisOptions1.ConnectString))
            {
                throw new ArgumentNullException(nameof(RedisOption.ConnectString));
            }

            if (!redisOptions1.DbTables.Any())
            {
                throw new ArgumentNullException(nameof(RedisOption.DbTables));
            }


            ConfigurationOptions connectOptions = ConfigurationOptions.Parse(redisOptions1.ConnectString);
            if (redisOptions1.ReconnectTimeout < 5000) redisOptions1.ReconnectTimeout = 5000;
            connectOptions.ReconnectRetryPolicy = new LinearRetry(redisOptions1.ReconnectTimeout);

            _conn = ConnectionMultiplexer.Connect(connectOptions);
            Redis = _conn.GetDatabase();

            foreach (var dbtable in redisOptions1.DbTables)
            {
                var queueName = !string.IsNullOrEmpty(redisOptions1.CanalDestinationName) ? $"{redisOptions1.CanalDestinationName}." : "";
                queueName += dbtable;
                _topicList.Add(queueName);
            }

            _logger.LogInformation($"Redis [{redisOptions1.ConnectString}] connect success");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                //一个队列一个线程单独处理
                foreach (var topic in _topicList)
                {
                    new Thread(() =>
                    {
                        try
                        {
                            while (!isDispose)
                            {
                                ReviceQueue(topic);
                            }
                        }
                        catch (Exception)
                        {
                            //ignore
                        }
                    }).Start();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "redis consume client start error...");
            }
            return Task.CompletedTask;
        }


        private void ReviceQueue(string topic)
        {
            string message = null;
            try
            {
                message = Redis.ListRightPop(topic);
                if (string.IsNullOrEmpty(message)) return;

                DataChange data = message.JsonToObject<DataChange>();
                var result = _dbTypeMapper.TransferToDb(this._dbContext, data);
                if (!result.Item1)
                {
                    _logger.LogError($"Topic:{topic},Message:{message},Error:{result.Item2}");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Topic:{topic},Message:{message ?? ""}");
            }

        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            Dispose();
            return Task.CompletedTask;
        }



        private volatile bool isDispose;
        public void Dispose()
        {
            try
            {
                if (isDispose) return;
                isDispose = true;
                _conn.Dispose();
                _logger.LogInformation("redis consume client stop succ...");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "redis consume client stop error...");
            }
        }

        private void UpdateFromEnv(RedisOption _rab)
        {
            var host = _configuration["redis.connect"];
            if (!string.IsNullOrEmpty(host))
            {
                _rab.ConnectString = host;
            }

            var retryTimeout = _configuration["redis.reconnectTimeout"];
            if (!string.IsNullOrEmpty(retryTimeout))
            {
                _rab.ReconnectTimeout = int.Parse(retryTimeout);
            }

            var dbTables = _configuration["redis.dbTables"];
            if (!string.IsNullOrEmpty(dbTables))
            {
                _rab.DbTables = dbTables.Split(':').ToList();
            }

        }
    }

}
