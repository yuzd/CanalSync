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

        public int WaitTimeWhenError { get; set; }

        public bool PublisherStopWhenError { get; set; }

    }
}
