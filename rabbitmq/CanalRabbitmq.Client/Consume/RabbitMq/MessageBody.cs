using System;
using RabbitMQ.Client.Events;

namespace CanalRabbitmq.Client.Consume.RabbitMq
{
    public class MessageBody
    {
        public EventingBasicConsumer Consumer { get; set; }
        public BasicDeliverEventArgs BasicDeliver { get; set; }
        /// <summary>
        ///  0成功
        /// </summary>
        public int Code { get; set; }
        public string Content { get; set; }
        public string ErrorMessage { get; set; }
        public bool Error { get; set; }
        public Exception Exception { get; set; }
    }
}
