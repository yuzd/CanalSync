using System;
using System.Collections.Generic;
using System.Text;

namespace MysqlCanalMq.Models
{
    public class CanalMqBody
    {
        public object DbModel { get; set; }
        public string EventType { get; set; }
        public string DbName { get; set; }
        public string TbName { get; set; }
    }
}
