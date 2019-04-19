using System;
using System.Text;
using MysqlCanalMq.Common.Interface;
using MysqlCanalMq.Common.Models.canal;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace MysqlCanalMq.Common.Produce.RabbitMq
{
    public class RabitMqProduce : IProduce
    {
        private readonly ConnectionFactory _connectionFactory = null;
        public RabitMqProduce(RabitMqOption option)
        {
            _connectionFactory = new ConnectionFactory
            {
                HostName = option.Host,
                Port = option.Port,
                UserName = option.UserName,
                Password = option.Password,
                AutomaticRecoveryEnabled = true,
                VirtualHost = string.IsNullOrEmpty(option.VirtualHost) ? "/" : option.VirtualHost
            };
        }


        /// <summary>
        /// 单消息入队 开启确认模式
        /// 同步模式 保证消息一定是成功投递
        /// </summary>
        public void Produce(DataChange message)
        {
            if (message != null)
            {
                var topic = !string.IsNullOrEmpty(message.CanalDestination) ? $"{message.CanalDestination}." : "";
                topic += !string.IsNullOrEmpty(message.DbName) ? $"{message.DbName}." : "";
                topic += message.TableName;
                if (string.IsNullOrEmpty(topic)) throw new ArgumentNullException(nameof(topic));

                using (IConnection connection = _connectionFactory.CreateConnection())
                {
                    using (IModel channel = connection.CreateModel())
                    {
                        channel.ExchangeDeclare("canal", "direct", true, false, null);
                        string messageString = JsonConvert.SerializeObject(message);
                        byte[] body = Encoding.UTF8.GetBytes(messageString);
                        var properties = channel.CreateBasicProperties();
                        properties.Persistent = true; //使消息持久化
                        channel.QueueDeclare(topic, true, false, false);
                        channel.QueueBind(topic, "canal", topic);
                        channel.ConfirmSelect();
                        channel.BasicPublish("canal", topic, properties, body);
                        bool success = channel.WaitForConfirms(new TimeSpan(0, 0, 60));
                        if (!success)
                        {
                            throw new Exception("WaitForConfirms fail");
                        }
                    }
                }
            }
        }


    }
}
