using System;
using System.Collections.Generic;
using System.Text;

namespace MysqlCanalMq.Common.RabitMQ
{
    public class RabitMqOption
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public string VirtualHost { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        /// <summary>
        /// 秒单位
        /// </summary>
        public int TimeOut { get; set; }

        /// <summary>
        /// 是否开启提交消息NCK模式
        /// </summary>
        public bool PublisherConfirm { get; set; }

        public override string ToString()
        {
            var connect = string.Empty;
            if (!string.IsNullOrEmpty(Host))
            {
                connect += $"host={Host + (Port > 0 ? $":{Port}" : "")};";
            }

            if (!string.IsNullOrEmpty(VirtualHost))
            {
                connect += $"virtualHost={VirtualHost};";
            }

            if (!string.IsNullOrEmpty(UserName))
            {
                connect += $"username={UserName};";
            }


            if (!string.IsNullOrEmpty(Password))
            {
                connect += $"password={Password};";
            }

            if (this.PublisherConfirm)
            {
                connect += $"publisherConfirms=true;";
            }

            if (TimeOut > 0)
            {
                connect += $"timeout={TimeOut};";
            }

            if (connect.EndsWith(";"))
            {
                connect = connect.Substring(0, connect.Length - 1);
            }
            return connect;
        }
    }
}
