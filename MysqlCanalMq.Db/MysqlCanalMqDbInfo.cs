using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntData.ORM.Linq;
using AntData.ORM.Mapping;

namespace MysqlCanalMq.Db
{
    public class MysqlCanalMqDbInfo
    {
        private static readonly ConcurrentDictionary<string, Type> DbTypeList = new ConcurrentDictionary<string, Type>();
        private static readonly ConcurrentDictionary<Type, bool> DbTypeListInited = new ConcurrentDictionary<Type, bool>();
        private static object lockObj = new  object();

        /// <summary>
        /// 初始化db类型等参数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void UseDb<T>() where T : IEntity
        {
            var type = typeof(T);
            if (DbTypeListInited.TryGetValue(type, out bool _))
            {
                return;
            }
            lock (lockObj)
            {
                if (DbTypeListInited.TryGetValue(type, out bool _))
                {
                    return;
                }
                //获取该类型所有的 属性
                foreach (var prop in type.GetProperties())
                {
                    if(prop.PropertyType.GenericTypeArguments == null ||prop.PropertyType.GenericTypeArguments.Length!=1)continue;
                    var genericType = prop.PropertyType.GenericTypeArguments[0];
                    if (genericType == null) continue;
                    var tbAttribute = genericType.GetCustomAttributes(
                        typeof(TableAttribute), true
                    ).FirstOrDefault() as TableAttribute;
                    if (tbAttribute == null)
                    {
                        continue;
                    }
                    if (string.IsNullOrEmpty(tbAttribute.Name))
                    {
                        continue;
                    }
                    var topic = !string.IsNullOrEmpty(tbAttribute.Db) ? $"{tbAttribute.Db}." : "";
                    topic += tbAttribute.Name;
                    DbTypeList.TryAdd(topic, genericType);
                }

                DbTypeListInited.TryAdd(type, true);
            }
        }


        /// <summary>
        /// 根据db名词和表名称获取到类型
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="tbName"></param>
        /// <returns></returns>
        public static Type GetDbModelType(string dbName, string tbName)
        {
            //var typeEntity = typeof(T);
            //if (!DbTypeListInited.TryGetValue(typeEntity, out bool _))
            //{
            //    UseDb<T>();
            //}

            var topic = !string.IsNullOrEmpty(dbName) ? $"{dbName}." : "";
            topic += tbName;
            if (DbTypeList.TryGetValue(topic, out Type type))
            {
                return type;
            }
            return null;
        }


    }
}
