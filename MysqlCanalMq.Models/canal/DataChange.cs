
using System.Collections.Generic;

namespace MysqlCanalMq.Models.canal
{
    public class DataChange
    {
        public string DbName { get; set; }

        public string TableName { get; set; }

        public string EventType { get; set; }

        public IList<ColumnData> BeforeColumnList { get; set; }

        public IList<ColumnData> AfterColumnList { get; set; }
    }
}
