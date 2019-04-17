using System;
using System.Collections.Generic;
using System.Text;
using EasyNetQ;

namespace MysqlCanalMq.Common.RabitMQ
{
    public class QueueNameConventions : Conventions
    {
        public QueueNameConventions(ITypeNameSerializer typeNameSerializer) : base(typeNameSerializer)
        {
            ErrorQueueNamingConvention = messageInfo => "CanalErrorQueue";
        }
    }
}
