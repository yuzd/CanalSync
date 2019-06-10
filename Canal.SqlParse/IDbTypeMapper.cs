using AntData.ORM.Data;
using System.Collections.Generic;
using Canal.SqlParse.Models.canal;

namespace Canal.SqlParse
{
    public interface IDbTypeMapper
    {

        (string, List<DataParameter>) GetInsertSql(string tabelName, IList<ColumnData> cols);

        (string, List<DataParameter>) GetDeleteSql(string tabelName, IList<ColumnData> cols);

        (string, List<DataParameter>) GetUpdateSql(string tabelName, IList<ColumnData> cols);

    }
}
