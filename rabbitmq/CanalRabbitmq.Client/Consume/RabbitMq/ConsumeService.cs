using System;
using System.Linq;
using AntData.ORM.Data;
using Canal.SqlParse;
using Canal.SqlParse.Models;
using Canal.SqlParse.Models.canal;
using Canal.SqlParse.StaticExt;
using MysqlCanalMq.Server.RabbitMq;
using RabbitMQ.Client;

namespace MysqlCanalMq.Common.Consume.RabbitMq
{
    public class ConsumeService : MQServiceBase
    {
        public Action<MessageLevel, string, Exception> OnAction = null;
        private readonly IDbTransfer _dbTypeMapper;
        public ConsumeService(RabitMqOption config, string dbtable,IDbTransfer dbTypeMapper) : base(config)
        {
            _dbTypeMapper = dbTypeMapper;
            var queueName = !string.IsNullOrEmpty(config.CanalDestinationName) ? $"{config.CanalDestinationName}." : "";
            queueName += dbtable;
            base.Queues.Add(new QueueInfo()
            {
                ExchangeType = ExchangeType.Direct,
                Queue = queueName,
                RouterKey = queueName,
                OnReceived = this.OnReceived,
            });
        }

        public override string vHost => Config.VirtualHost;
        public override string Exchange => "canal";


        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="message"></param>
        public override void OnReceived(MessageBody message)
        {
            //如果不返回ACK 则 RabbitMQ 不会再发送数据
            message.Consumer.Model.BasicAck(message.BasicDeliver.DeliveryTag, true);
            if (string.IsNullOrEmpty(message.Content))
            {
                return;
            }
            try
            {
                DataChange data = message.Content.JsonToObject<DataChange>();
                var result = _dbTypeMapper.TransferToDb(data);
                if (!result.Item1)
                {
                    OnAction?.Invoke(MessageLevel.Error, $"TransferToDb Error", new Exception(result.Item2));
                }
            }
            catch (Exception ex)
            {
                OnAction?.Invoke(MessageLevel.Error, ex.Message + $"【{message.Content}】", ex);
            }

        }




    }
}
