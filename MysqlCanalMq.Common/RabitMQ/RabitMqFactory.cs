using System;
using System.Collections.Generic;
using System.Text;
using MysqlCanalMq.Models;
using MysqlCanalMq.Models.canal;

namespace MysqlCanalMq.Common.RabitMQ
{
    public interface IProduceRabitMq
    {
        void Produce(DataChange message, string de = null);

    }

    public class RabitMqFactory
    {

        /// <summary>
        /// 创建一个生产者
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public static IProduceRabitMq CreateProduceRabitMq(RabitMqOption option)
        {
            return new RabitMq(option);
        }


    }
}
