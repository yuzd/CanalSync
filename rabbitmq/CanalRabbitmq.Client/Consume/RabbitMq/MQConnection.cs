using System.Text;
using CanalRabbitmq.Client.Models;
using RabbitMQ.Client;

namespace CanalRabbitmq.Client.Consume.RabbitMq
{
    public class MQConnection
    {
        private string vhost = string.Empty;
        private IConnection connection = null;
        private RabitMqOption config = null;
        /// <summary>
        ///  构造无 utf8 标记的编码转换器
        /// </summary>
        public static UTF8Encoding UTF8 { get; set; } = new UTF8Encoding(false);

        public MQConnection(RabitMqOption config, string vhost)
        {
            this.config = config;
            this.vhost = vhost;
        }

        public IConnection Connection
        {
            get
            {
                if (connection == null)
                {
                    ConnectionFactory factory = new ConnectionFactory
                    {
                        AutomaticRecoveryEnabled = true,
                        UserName = this.config.UserName,
                        Password = this.config.Password,
                        HostName = this.config.Host,
                        VirtualHost = this.vhost,
                        Port = this.config.Port
                    };
                    connection = factory.CreateConnection();
                }

                return connection;
            }
        }
    }
}