using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MysqlCanalMq.Common.Interface;
using MysqlCanalMq.Common.Produce.RabbitMq;

namespace MysqlCanalMq.Canal.OutPut
{
    public class OutPutFactory
    {
        /// <summary>
        /// 创建一个Rabbitmq生产者
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public static IProduce CreateRabitMqProduce(RabitMqOption option)
        {
            return new RabitMqProduce(option);
        }
    }
}
