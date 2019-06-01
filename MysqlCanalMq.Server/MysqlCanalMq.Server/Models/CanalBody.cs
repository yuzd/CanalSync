using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using MysqlCanalMq.Common.Models.canal;

namespace MysqlCanalMq.Server.Models
{
    public class CanalBody: INotification
    {
        public CanalBody(DataChange message)
        {
            Message = message;
        }

        public DataChange Message { get; }
    }
}
