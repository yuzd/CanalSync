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
        private readonly DbContext<DB> _dbContext;
        private readonly IDbTypeMapper _dbTypeMapper;
        public ConsumeService(RabitMqOption config, string dbtable, DbContext<DB> dbContext, IDbTypeMapper dbTypeMapper) : base(config)
        {
            _dbContext = dbContext;
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

                if (data == null || string.IsNullOrEmpty(data.DbName) || string.IsNullOrEmpty(data.TableName) || string.IsNullOrEmpty(data.EventType))
                {
                    OnAction?.Invoke(MessageLevel.Error, "Content.JsonToObject<DataChange>()", new Exception(message.Content));
                    return;
                }


                var cloumns = data.AfterColumnList == null || !data.AfterColumnList.Any()
                    ? data.BeforeColumnList
                    : data.AfterColumnList;

                var primaryKey = cloumns.FirstOrDefault(r => r.IsKey);
                if (primaryKey == null || string.IsNullOrEmpty(primaryKey.Value))
                {
                    //没有主键
                    OnAction?.Invoke(MessageLevel.Error, $"revice data without primaryKey", new Exception(message.Content));
                    return;
                }
                var result = _dbTypeMapper.TransferToDb(this._dbContext, data);
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
