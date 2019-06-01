using AntData.ORM.Data;
using MysqlCanalMq.Common.Models.canal;
using System.Collections.Generic;

namespace MysqlCanalMq.Common.SqlParse
{
    public interface IDbTypeMapper
    {

        (string, List<DataParameter>) GetInsertSql(string tabelName, IList<ColumnData> cols);

        (string, List<DataParameter>) GetDeleteSql(string tabelName, IList<ColumnData> cols);

        (string, List<DataParameter>) GetUpdateSql(string tabelName, IList<ColumnData> cols);

    }
}
