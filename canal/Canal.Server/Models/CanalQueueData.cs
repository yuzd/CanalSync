using System;
using System.Collections.Generic;
using System.Text;

namespace Canal.Server.Models
{
    internal class CanalQueueData
    {
        public string Time { get; set; }
        public long BatchId { get; set; }
        public List<CanalBody> CanalBodyList { get; set; }
    }
}
