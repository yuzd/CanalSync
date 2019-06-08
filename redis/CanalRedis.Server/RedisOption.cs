using System.Collections.Generic;

namespace CanalRedis.Server
{
    public class RedisOption
    {
        public string ConnectString { get; set; }
        public int ReconnectTimeout { get; set; }
        
        public List<string> DbTables { get; set; }
    }
}