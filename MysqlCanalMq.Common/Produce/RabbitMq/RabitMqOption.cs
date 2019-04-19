using System.Collections.Generic;

namespace MysqlCanalMq.Common.Produce.RabbitMq
{
    public class RabitMqOption
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public string VirtualHost { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }



        public string CanalDestinationName { get; set; }


        public List<string> DbTables { get; set; }
    }

}
