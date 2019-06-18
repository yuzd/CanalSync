using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Canal.Model;
using Canal.Server.Interface;

namespace Canal.Server.Models
{
    public class CanalBody: INotification
    {
        public CanalBody()
        {
            
        }
        public CanalBody(IList<DataChange> message,long batchId)
        {
            Message = message;
            BatchId = batchId;
        }

        public IList<DataChange> Message { get; }
        public long BatchId { get; }
    }
}
