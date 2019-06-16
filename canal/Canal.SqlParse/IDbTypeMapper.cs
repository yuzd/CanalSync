using AntData.ORM.Data;
using System.Collections.Generic;
using Canal.SqlParse.Models.canal;

namespace Canal.SqlParse
{
    public interface IDbTransfer
    {
        DbTransferResult TransferToDb(DataChange data);

    }

    public class DbTransferResult
    {
        public DbTransferResult(bool success,string msg)
        {
            Success = success;
            Msg = msg;
        }
        public bool Success { get; set; }
        public string Msg { get; set; }
    }
}
