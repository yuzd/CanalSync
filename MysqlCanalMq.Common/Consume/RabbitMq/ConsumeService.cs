using System;
using System.Linq;
using AntData.ORM.Data;
using MysqlCanalMq.Common.Models;
using MysqlCanalMq.Common.Models.canal;
using MysqlCanalMq.Common.Produce.RabbitMq;
using MysqlCanalMq.Common.SqlParse;
using MysqlCanalMq.Common.StaticExt;
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


                var sql = $"select count(*) from {data.TableName} where {primaryKey.Name} = @primaryValue";
                //判断是否主键已存在？
                var isExist = _dbContext.Execute<int>(sql, new { primaryValue = primaryKey.Value }) == 1;

                if (data.EventType.Equals("INSERT"))
                {
                    if (isExist)
                    {
                        return;
                    }

                    var insertSql = _dbTypeMapper.GetInsertSql(data.TableName, cloumns);
                    var insertR = _dbContext.Execute(insertSql.Item1, insertSql.Item2.ToArray()) > 0;

                    if (!insertR)
                    {
                        OnAction?.Invoke(MessageLevel.Error, $"_dbContext.Insert(entity) return error", new Exception(message.Content));
                        return;
                    }
                }
                else if (data.EventType.Equals("DELETE"))
                {
                    if (!isExist)
                    {
                        return;
                    }

                    var deleteSql = _dbTypeMapper.GetDeleteSql(data.TableName, cloumns);
                    var deleteR = _dbContext.Execute(deleteSql.Item1, deleteSql.Item2.ToArray()) > 0;
                    if (!deleteR)
                    {
                        OnAction?.Invoke(MessageLevel.Error, $"_dbContext.Delete(entity) return error", new Exception(message.Content));
                        return;
                    }
                }
                else if (data.EventType.Equals("UPDATE"))
                {
                    if (!isExist)
                    {
                        var insertSql = _dbTypeMapper.GetInsertSql(data.TableName, cloumns);
                        var insertR = _dbContext.Execute(insertSql.Item1, insertSql.Item2.ToArray()) > 0;
                        if (!insertR)
                        {
                            OnAction?.Invoke(MessageLevel.Error, $"_dbContext.Update(entity) return error", new Exception(message.Content));
                            return;
                        }
                    }
                    else
                    {
                        var updateSql = _dbTypeMapper.GetUpdateSql(data.TableName, cloumns);
                        var updateR = _dbContext.Execute(updateSql.Item1, updateSql.Item2.ToArray()) > 0;
                        if (!updateR)
                        {
                            OnAction?.Invoke(MessageLevel.Error, $"_dbContext.Update(entity) return error", new Exception(message.Content));
                            return;
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                OnAction?.Invoke(MessageLevel.Error, ex.Message + $"【{message.Content}】", ex);
            }

        }




    }
}
