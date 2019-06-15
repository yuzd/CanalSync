using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Canal.Server.Interface;

namespace Canal.Server.Models
{
    public class CanalBody: INotification
    {
        public CanalBody(DataChange message)
        {
            Message = message;
        }

        public DataChange Message { get; }
        public bool Succ { get; set; }
    }
}
