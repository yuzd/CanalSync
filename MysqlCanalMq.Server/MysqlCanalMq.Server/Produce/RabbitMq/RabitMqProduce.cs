using System;
using System.Collections.Concurrent;
using System.Text;
using Canal.Server.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace MysqlCanalMq.Server.RabbitMq
{
    public class RabitMqProduce 
    {
        private readonly RabitMqOption _rabitMqOption;
        private readonly IConnection _connection = null;
        private readonly ConcurrentDictionary<string, IModel> _channelDictionary = new ConcurrentDictionary<string, IModel>();
        public RabitMqProduce(RabitMqOption option)
        {
            _rabitMqOption = option;
            var connectionFactory = new ConnectionFactory
            {
                HostName = option.Host,
                Port = option.Port,
                UserName = option.UserName,
                Password = option.Password,
                AutomaticRecoveryEnabled = true,
                VirtualHost = string.IsNullOrEmpty(option.VirtualHost) ? "/" : option.VirtualHost
            };
            _connection = connectionFactory.CreateConnection();
        }


        /// <summary>
        /// 单消息入队 开启确认模式
        /// 同步模式 保证消息一定是成功投递
        /// </summary>
        public void Produce(DataChange message)
        {
            if (message == null)
            {
                return;
            }

            var topic = !string.IsNullOrEmpty(message.CanalDestination) ? $"{message.CanalDestination}." : "";
            topic += !string.IsNullOrEmpty(message.DbName) ? $"{message.DbName}." : "";
            topic += message.TableName;
            if (string.IsNullOrEmpty(topic)) throw new ArgumentNullException(nameof(topic));
            if (!_channelDictionary.TryGetValue(topic, out var channel))
            {
                channel = _connection.CreateModel();
                channel.ExchangeDeclare("canal", "direct", true, false, null);
                channel.QueueDeclare(topic, true, false, false);
                channel.QueueBind(topic, "canal", topic);
                if (_rabitMqOption.ConfirmSelect)
                {
                    channel.ConfirmSelect();
                }
                _channelDictionary.TryAdd(topic, channel);
            }

            string messageString = JsonConvert.SerializeObject(message);
            byte[] body = Encoding.UTF8.GetBytes(messageString);
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true; //使消息持久化
            channel.BasicPublish("canal", topic, properties, body);
            if (_rabitMqOption.ConfirmSelect)
            {
                channel.WaitForConfirmsOrDie();
            }
        }

        public void Dispose()
        {
            _connection?.Dispose();
            foreach (var channel in _channelDictionary)
            {
                channel.Value?.Dispose();
            }
        }
    }
}
