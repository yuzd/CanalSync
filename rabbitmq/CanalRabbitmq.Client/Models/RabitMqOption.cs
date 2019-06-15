using System.Collections.Generic;

namespace MysqlCanalMq.Server.RabbitMq
{
    public class RabitMqOption
    {
        
        /// <summary>
        /// rabbitmq的服务器ip
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// rabbitmq的服务器端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// rabbitmq访问的虚路径
        /// </summary>
        public string VirtualHost { get; set; }
        
        /// <summary>
        /// rabbmitmq的连接账户
        /// </summary>
        public string UserName { get; set; }
        
        /// <summary>
        /// rabbitmq的连接密码
        /// </summary>
        public string Password { get; set; }


        /// <summary>
        /// 对应的canal-server的名称
        /// </summary>
        public string CanalDestinationName { get; set; }


        /// <summary>
        /// 指定要监听的db和表配置的
        /// 例如 test.person代表db名称为test 表名称为person
        /// </summary>
        public List<string> DbTables { get; set; }
    }

}
