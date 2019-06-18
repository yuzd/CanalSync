using System;
using System.Collections.Generic;
using Com.Alibaba.Otter.Canal.Protocol;

namespace Canal.Model
{
    public class DataChange
    {
        public string DbName { get; set; }

        public string TableName { get; set; }

        public string EventType { get; set; }
        public string CanalDestination { get; set; }

        public List<Column> BeforeColumnList { get; set; }

        public List<Column> AfterColumnList { get; set; }
    }
}
