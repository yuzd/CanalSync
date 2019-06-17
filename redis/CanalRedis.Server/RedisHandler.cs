using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Canal.Server.Interface;
using Canal.Server.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using StackExchange.Redis;

namespace CanalRedis.Server
{
    /// <summary>
    /// canal消费到redis的消息队列
    /// </summary>
    public class RedisHandler : INotificationHandler<CanalBody>, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IDatabase Redis;
        private readonly RedisOption _option;
        private readonly ConnectionMultiplexer _conn;
        private readonly IConfiguration _configuration;

        public RedisHandler(ILogger<RedisHandler> logger, IOptions<RedisOption> options, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _option = options?.Value;
            if (_option == null)
            {
                _option = new RedisOption();
            }

            UpdateFromEnv(_option);

            if (string.IsNullOrEmpty(_option.ConnectString))
            {
                throw new ArgumentNullException(nameof(RedisOption.ConnectString));
            }

            ConfigurationOptions connectOptions = ConfigurationOptions.Parse(_option.ConnectString);
            if (_option.ReconnectTimeout < 5000) _option.ReconnectTimeout = 5000;
            connectOptions.ReconnectRetryPolicy = new LinearRetry(_option.ReconnectTimeout);

            _conn = ConnectionMultiplexer.Connect(connectOptions);
            Redis = _conn.GetDatabase();

            _logger.LogInformation($"Redis [{_option.ConnectString}] connect success");
            _logger.LogInformation($"Redis Produce Listening: {string.Join(",", _option.DbTables)}");

            AppDomain.CurrentDomain.ProcessExit += (sender, args) => Dispose();
        }

        public Task Handle(CanalBody notification)
        {

            var message = notification.Message;


            //过滤
            if (_option.DbTables.Any() && !_option.DbTables.Contains(message.DbName + "." + message.TableName))
            {
                return Task.CompletedTask;
            }


            var topic = !string.IsNullOrEmpty(message.CanalDestination) ? $"{message.CanalDestination}." : "";
            topic += !string.IsNullOrEmpty(message.DbName) ? $"{message.DbName}." : "";
            topic += message.TableName;
            if (string.IsNullOrEmpty(topic)) throw new ArgumentNullException(nameof(topic));
            string messageString = JsonConvert.SerializeObject(message);

            var ploicy = Policy.Handle<Exception>()
                .WaitAndRetry(new[]
                {
                    TimeSpan.FromSeconds(6),
                    TimeSpan.FromSeconds(12),
                    TimeSpan.FromSeconds(18),
                    TimeSpan.FromSeconds(24),
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromSeconds(36)
                });
            try
            {

                ploicy.Execute(() =>
                {
                    //在队列的尾部添加
                    var length = Redis.ListRightPush(topic, messageString);
                    //设置队列的长度
                    Redis.StringSet(topic + ".length", "" + length);

                    notification.Succ = true;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"redis mq send fail，{message.CanalDestination + "," + message.DbName + "," + message.TableName}");
                throw;
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _conn.Dispose();
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