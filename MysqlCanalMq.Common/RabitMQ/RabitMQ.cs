using System;
using System.Collections.Generic;
using System.Linq;
using AntData.ORM.Mapping;
using EasyNetQ;
using MysqlCanalMq.Models;

namespace MysqlCanalMq.Common.RabitMQ
{
   

    internal class RabitMq : IDisposable, IConsumeRabitMq, IProduceRabitMq
    {
        private readonly IBus _eventBus;
        private readonly Dictionary<string, string> _consumeTypeList = new Dictionary<string, string>();
        public RabitMq(RabitMqOption option)
        {
            if (option == null)
            {
                throw new ArgumentNullException(nameof(RabitMqOption));
            }

            _eventBus = RabbitHutch.CreateBus(option.ToString());
        }

        /// <summary>
        /// 一个表一个队列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="evnetType"></param>
        /// <param name="message"></param>
        /// <param name="canalDestination"></param>
        public void Produce<T>(string evnetType, T message, string canalDestination = null) where T : CanalMqBasic
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            var topic = GetTopick<T>(canalDestination);
            if (string.IsNullOrEmpty(topic)) throw new ArgumentNullException(nameof(topic));
            _eventBus.Publish(new CanalMqBody
            {
                DbModel = message,
                EventType = evnetType
            }, (c) => c.WithQueueName(topic));
        }


        public IConsumeRabitMq AddConsume<T>(string canalDestination = null) where T : CanalMqBasic
        {
            var topic = GetTopick<T>(canalDestination);
            if (string.IsNullOrEmpty(topic)) throw new ArgumentNullException(nameof(topic));
            if (_consumeTypeList.ContainsKey(topic)) throw new ArgumentException($"{typeof(T).Name} is already exist!");
            _consumeTypeList.Add(topic, canalDestination);
            return this;
        }


        public void Consume(Action<CanalMqBody> action, string canalDestination = null)
        {
            foreach (KeyValuePair<string, string> item in _consumeTypeList)
            {
                if (string.IsNullOrEmpty(item.Key)) continue;

                _eventBus.Subscribe<CanalMqBody>(item.Key, action, c => c.WithQueueName(item.Key));
            }
        }

        private string GetTopick<T>(string canalDestination = null) where T : CanalMqBasic
        {
            var type = typeof(T);
            var tbAttribute = type.GetCustomAttributes(
                typeof(TableAttribute), true
            ).FirstOrDefault() as TableAttribute;


            if (tbAttribute == null)
            {
                throw new ArgumentNullException($"TableAttribute in Type:{type.Name} is null");
            }

            if (string.IsNullOrEmpty(tbAttribute.Name))
            {
                throw new ArgumentNullException($"TableAttribute in Type:{type.Name}.Name is null");
            }

            var topic = !string.IsNullOrEmpty(canalDestination) ? $"{canalDestination}." : "";
            topic += !string.IsNullOrEmpty(tbAttribute.Database) ? $"{tbAttribute.Database}." : "";
            topic += tbAttribute.Name;

            return topic;
        }

        public void Dispose()
        {
            _eventBus?.Dispose();
        }
    }
}
