using System.Collections.Generic;
using Com.Alibaba.Otter.Canal.Protocol;

namespace Canal.Server.Models
{
    public class DataChange
    {
        public string DbName { get; set; }

        public string TableName { get; set; }

        public string EventType { get; set; }
        public string CanalDestination { get; set; }

        public IList<Column> BeforeColumnList { get; set; }

        public IList<Column> AfterColumnList { get; set; }
    }
}
