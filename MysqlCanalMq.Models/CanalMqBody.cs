using System;
using System.Collections.Generic;
using System.Text;

namespace MysqlCanalMq.Models
{
    public class CanalMqBody
    {
        public CanalMqBasic DbModel { get; set; }
        public string EventType { get; set; }
    }
}
