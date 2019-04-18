using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AntData.ORM;
using AntData.ORM.Data;
using AntData.ORM.Mapping;

using DbModels;
using MysqlCanalMq.Common.RabitMQ;
using MysqlCanalMq.Common.StaticExt;
using MysqlCanalMq.Db;
using MysqlCanalMq.Models;
using MysqlCanalMq.Models.canal;
using MysqlCanalMq.Common.Reflection;

namespace MysqlCanalMq.Common.Consume
{
    public class ConsumeService : MQServiceBase
    {
        public Action<MessageLevel, string, Exception> OnAction = null;
        private DbContext<DB> _dbContext;
        public ConsumeService(RabitMqOption config, Type dbmodelType, DbContext<DB> dbContext) : base(config)
        {
            _dbContext = dbContext;
            var queueName = GetTopick(dbmodelType, config.CanalDestinationName);
            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentException($"Type:{dbmodelType.Name} can not register as comsumer");
            }
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
            if (string.IsNullOrEmpty(message.Content))
            {
                message.Consumer.Model.BasicAck(message.BasicDeliver.DeliveryTag, true);
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

                var modelType = MysqlCanalMqDbInfo.GetDbModelType(data.DbName, data.TableName);
                if (modelType == null)
                {
                    OnAction?.Invoke(MessageLevel.Error, $"MysqlCanalMqDbInfo.GetDbModelType({data.DbName}, {data.TableName})", new Exception("modelType parse null"));
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
                    message.Consumer.Model.BasicAck(message.BasicDeliver.DeliveryTag, true);
                    return;
                }
                
                var entity = GetDbModel(modelType, cloumns);
                if (entity == null)
                {
                    return;
                }

                var sql = $"select count(*) from {data.TableName} where {primaryKey.Name} = @primaryValue";
                //判断是否主键已存在？
                var isExist = _dbContext.Execute<int>(sql, new { primaryValue = primaryKey.Value }) == 1;

                if (data.EventType.Equals("INSERT"))
                {
                    if (isExist)
                    {
                        message.Consumer.Model.BasicAck(message.BasicDeliver.DeliveryTag, true);
                        return;
                    }

                    var insertR = _dbContext.Insert(entity) > 0;
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
                        message.Consumer.Model.BasicAck(message.BasicDeliver.DeliveryTag, true);
                        return;
                    }

                    var deleteR = _dbContext.Delete(entity) > 0;
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
                        var insertR = _dbContext.Insert(entity) > 0;
                        if (!insertR)
                        {
                            OnAction?.Invoke(MessageLevel.Error, $"_dbContext.Update(entity) return error", new Exception(message.Content));
                            return;
                        }
                    }
                    else
                    {
                        var updateR = _dbContext.Update(entity) > 0;
                        if (!updateR)
                        {
                            OnAction?.Invoke(MessageLevel.Error, $"_dbContext.Update(entity) return error", new Exception(message.Content));
                            return;
                        }
                    }
                    
                }

                message.Consumer.Model.BasicAck(message.BasicDeliver.DeliveryTag, true);
            }
            catch (Exception ex)
            {
                OnAction?.Invoke(MessageLevel.Error, ex.Message + $"【{message.Content}】", ex);
            }

        }



        private CanalMqBasic GetDbModel(System.Type type, IList<ColumnData> cols)
        {
            try
            {
                if (cols == null || cols.Count < 1)
                {
                    return null;
                }
                var obj = type.FastNew();
                var pros = type.GetCanWritePropertyInfo().ToDictionary(r => r.Name.ToLower(), y => y);
                if (pros.Count < 1)
                {
                    OnAction?.Invoke(MessageLevel.Error, "ConsumeService.ParseDbModel", new Exception($"类型:{type.Name} 没有可写的property"));
                    return null;
                }
                foreach (var c in cols)
                {
                    var key = c.Name.ToLower();

                    if (!pros.ContainsKey(key))
                    {
                        continue;
                    }

                    var prop = pros[key];
                    var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    if (c.IsNull)
                    {
                        if (prop.ToString().Contains("System.Nullable"))
                        {
                            prop.FastSetValue(obj, null);
                        }
                        continue;
                    }
                    var value = TypeConvertUtils.Parse(c.Value, propType);
                    prop.FastSetValue(obj, value);
                }
                return obj as CanalMqBasic;
            }
            catch (Exception ex)
            {
                OnAction?.Invoke(MessageLevel.Error, "ConsumeService.ParseDbModel", ex);
                return null;
            }
        }
        private string GetTopick(Type type, string canalDestination = null)
        {
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
            topic += !string.IsNullOrEmpty(tbAttribute.Db) ? $"{tbAttribute.Db}." : "";
            topic += tbAttribute.Name;

            return topic;
        }
    }
}
