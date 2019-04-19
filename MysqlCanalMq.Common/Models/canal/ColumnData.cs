namespace MysqlCanalMq.Common.Models.canal
{
    public class ColumnData
	{
        public string Name { get; set; }
        public int Length { get; set; }
        public string MysqlType { get; set; }
        public int SqlType { get; set; }
        public string Value { get; set; }
        public bool IsKey { get; set; }
        public bool Updated { get; set; }
        public bool IsNull { get; set; }

    }
}
