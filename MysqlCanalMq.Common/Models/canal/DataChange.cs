using System.Collections.Generic;

namespace MysqlCanalMq.Common.Models.canal
{
    public class DataChange
    {
        public string DbName { get; set; }

        public string TableName { get; set; }

        public string EventType { get; set; }
        public string CanalDestination { get; set; }

        public IList<ColumnData> BeforeColumnList { get; set; }

        public IList<ColumnData> AfterColumnList { get; set; }
    }
}
