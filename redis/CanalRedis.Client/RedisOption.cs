using System.Collections.Generic;

namespace CanalRedis.Client
{
    public class RedisOption
    {
        public string ConnectString { get; set; }
        public int ReconnectTimeout { get; set; }
        public string CanalDestinationName { get; set; }
        
        public List<string> DbTables { get; set; }
    }
}