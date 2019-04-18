using System;
using System.Collections.Generic;
using System.Text;
using MysqlCanalMq.Models.canal;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace MysqlCanalMq.Common.RabitMQ
{
    internal class RabitMq : IProduceRabitMq
    {
        private readonly ConnectionFactory _connectionFactory = null;
        public RabitMq(RabitMqOption option)
        {
            _connectionFactory = new ConnectionFactory();
            _connectionFactory.HostName = option.Host;
            _connectionFactory.Port = option.Port;
            _connectionFactory.UserName = option.UserName;
            _connectionFactory.Password = option.Password;
            _connectionFactory.AutomaticRecoveryEnabled = true;
            _connectionFactory.VirtualHost = string.IsNullOrEmpty(option.VirtualHost) ? "/" : option.VirtualHost;
        }


        /// <summary>
        /// 单消息入队 开启确认模式
        /// 同步模式 保证消息一定是成功投递
        /// </summary>
        public void Produce(DataChange message, string de = null)
        {
            try
            {
                if (message != null)
                {
                    var topic = !string.IsNullOrEmpty(de) ? $"{de}." : "";
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
                            channel.QueueDeclare(topic, false, false, false);
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
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


    }
}
