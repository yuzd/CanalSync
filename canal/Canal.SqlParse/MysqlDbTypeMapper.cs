﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntData.ORM.Data;
using Canal.Model;
using Canal.SqlParse.Models;
using Com.Alibaba.Otter.Canal.Protocol;
using Microsoft.Extensions.Logging;
using Type = System.Type;

namespace Canal.SqlParse
{
    internal class MysqlDbTypeMapper : IDbTransfer
    {
        private static readonly Dictionary<string, (Type, AntData.ORM.DataType)> dic = new Dictionary<string, (Type, AntData.ORM.DataType)>();

        static MysqlDbTypeMapper()
        {
            dic.Add("bigint", (typeof(long), AntData.ORM.DataType.Int64));
            dic.Add("binary", (typeof(byte[]), AntData.ORM.DataType.Binary));
            dic.Add("bit", (typeof(bool), AntData.ORM.DataType.Boolean));
            dic.Add("blob", (typeof(byte[]), AntData.ORM.DataType.Blob));
            dic.Add("bool", (typeof(bool), AntData.ORM.DataType.Boolean));
            dic.Add("boolean", (typeof(bool), AntData.ORM.DataType.Boolean));
            dic.Add("char", (typeof(char), AntData.ORM.DataType.Char));
            dic.Add("date", (typeof(DateTime), AntData.ORM.DataType.Date));
            dic.Add("datetime", (typeof(DateTime), AntData.ORM.DataType.DateTime));
            dic.Add("decimal", (typeof(decimal), AntData.ORM.DataType.Decimal));
            dic.Add("double", (typeof(double), AntData.ORM.DataType.Double));
            dic.Add("enum", (typeof(char), AntData.ORM.DataType.NVarChar));
            dic.Add("float", (typeof(float), AntData.ORM.DataType.Single));
            dic.Add("int", (typeof(int), AntData.ORM.DataType.Int32));
            dic.Add("longblob", (typeof(byte[]), AntData.ORM.DataType.Binary));
            dic.Add("longtext", (typeof(string), AntData.ORM.DataType.Text));
            dic.Add("mediumblob", (typeof(byte[]), AntData.ORM.DataType.Binary));
            dic.Add("mediumint", (typeof(int), AntData.ORM.DataType.Int32));
            dic.Add("mediumtext", (typeof(string), AntData.ORM.DataType.Text));
            dic.Add("numeric", (typeof(decimal), AntData.ORM.DataType.Decimal));
            dic.Add("real", (typeof(double), AntData.ORM.DataType.Double));
            dic.Add("smallint", (typeof(short), AntData.ORM.DataType.Int16));
            dic.Add("text", (typeof(string), AntData.ORM.DataType.Text));
            dic.Add("time", (typeof(TimeSpan), AntData.ORM.DataType.Time));
            dic.Add("timestamp", (typeof(DateTime), AntData.ORM.DataType.Timestamp));
            dic.Add("tinyblob", (typeof(byte[]), AntData.ORM.DataType.Binary));
            dic.Add("tinyint", (typeof(sbyte), AntData.ORM.DataType.SByte));
            dic.Add("tinytext", (typeof(string), AntData.ORM.DataType.Text));
            dic.Add("varchar", (typeof(string), AntData.ORM.DataType.VarChar));
            dic.Add("varbinary", (typeof(byte[]), AntData.ORM.DataType.Binary));
            dic.Add("year", (typeof(int), AntData.ORM.DataType.Int32));
        }
        private readonly DbContext<DB> _dbContext;
        private readonly ILogger<MysqlDbTypeMapper> _logger;
        public MysqlDbTypeMapper(DbContext<DB> dbContext,ILogger<MysqlDbTypeMapper> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        /// <summary>
        /// 执行到db
        /// </summary>
        /// <returns></returns>
        public DbTransferResult TransferToDb(DataChange data)
        {
            try
            {
                if (data == null || string.IsNullOrEmpty(data.DbName) || string.IsNullOrEmpty(data.TableName) || string.IsNullOrEmpty(data.EventType))
                {
                    return new DbTransferResult(false, "param error");
                }

                List<Column> cloumns = data.AfterColumnList == null || !data.AfterColumnList.Any()
                    ? data.BeforeColumnList
                    : data.AfterColumnList;

                var primaryKey = cloumns.FirstOrDefault(r => r.IsKey);
                if (primaryKey == null || string.IsNullOrEmpty(primaryKey.Value))
                {
                    //没有主键
                    return new DbTransferResult(false, "data without primaryKey");
                }

                bool IsExist()
                {
                    var sql = $"select count(*) from {data.TableName} where `{primaryKey.Name}` = @primaryValue";
                    //判断是否主键已存在？
                    var isExist = _dbContext.Execute<int>(sql, new {primaryValue = primaryKey.Value}) == 1;
                    return isExist;
                }


                if (data.EventType.Equals("INSERT"))
                {
                    //if (IsExist())
                    //{
                    //    return new DbTransferResult(true, "insert sql, but primaryKey is exist");
                    //}

                    try
                    {
                        var insertSql = this.GetInsertSql(data.TableName, cloumns);
                        _dbContext.Execute(insertSql.Item1, insertSql.Item2.ToArray());
                    }
                    catch (Exception)
                    {
                        //ignore
                    }
                }
                else if (data.EventType.Equals("DELETE"))
                {
                    var deleteSql = this.GetDeleteSql(data.TableName, cloumns);
                    _dbContext.Execute(deleteSql.Item1, deleteSql.Item2.ToArray());
                }
                else if (data.EventType.Equals("UPDATE"))
                {
                    if (!IsExist())
                    {
                        var insertSql = this.GetInsertSql(data.TableName, cloumns);
                        _dbContext.Execute(insertSql.Item1, insertSql.Item2.ToArray());
                    }
                    else
                    {
                        var updateSql = this.GetUpdateSql(data.TableName, cloumns);
                        _dbContext.Execute(updateSql.Item1, updateSql.Item2.ToArray());
                    }
                }
                else
                {
                    return new DbTransferResult(false, "EventType is invalid");
                }

                return new DbTransferResult(true, string.Empty);
            }
            catch (Exception e)
            {
                _logger.LogError(e,nameof(TransferToDb));
                return new DbTransferResult(false,e.Message);
            }


        }

        private (string, List<DataParameter>) GetInsertSql(string tabelName, IList<Column> cols)
        {
            var sql = $"insert into {tabelName} ";
            var columnStrList = new List<string>();
            var valueStrList = new List<string>();
            var dataParamList = new List<DataParameter>();
            foreach (var column in cols)
            {
                if (column.IsNull) continue;
                var param = GetDataParameter(column);
                if (param == null)
                {
                    throw new NotSupportedException($"Table:{tabelName},Filed:{column.Name},MysqlType:{column.MysqlType} is not supported!");
                }
                columnStrList.Add(column.Name);
                valueStrList.Add("@" + column.Name);
                dataParamList.Add(param);
            }

            sql += $" (`{string.Join("`,`", columnStrList)}`) ";
            sql += $" VALUES ({string.Join(",", valueStrList)}) ";

            return (sql, dataParamList);
        }

        private (string, List<DataParameter>) GetDeleteSql(string tabelName, List<Column> cols)
        {
            var index = cols.First(r => r.IsKey);
            var param = GetDataParameter(index);
            if (param == null)
            {
                throw new NotSupportedException($"Table:{tabelName},Filed:{index.Name},MysqlType:{index.MysqlType} is not supported!");
            }
            var sql = $"delete from {tabelName} where `{index.Name}` = @{index.Name}";
            return (sql, new List<DataParameter> { param });
        }

        private (string, List<DataParameter>) GetUpdateSql(string tabelName, List<Column> cols)
        {
            var sql = $"update {tabelName} set ";
            var dataParamList = new List<DataParameter>();

            var index = cols.First(r => r.IsKey);
            var paramIndex = GetDataParameter(index);
            if (paramIndex == null)
            {
                throw new NotSupportedException($"Table:{tabelName},Filed:{index.Name},MysqlType:{index.MysqlType} is not supported!");
            }
            var pair = new List<string>();
            foreach (var column in cols.Where(r => !r.IsKey))
            {
                if (!column.Updated) continue;
                var param = GetDataParameter(column);
                if (param == null)
                {
                    throw new NotSupportedException($"Table:{tabelName},Filed:{column.Name},MysqlType:{column.MysqlType} is not supported!");
                }

                dataParamList.Add(param);
                pair.Add($" `{column.Name}` = @{column.Name} ");

            }
            sql += $" {string.Join(",", pair)} where `{index.Name}` = @{index.Name} ";
            dataParamList.Add(paramIndex);
            return (sql, dataParamList);
        }

        private static DataParameter GetDataParameter(Column column)
        {
            var columnTypeStr = column.MysqlType.ToLower().Split('(')[0];
            if (!dic.TryGetValue(columnTypeStr, out (Type, AntData.ORM.DataType) targetType))
            {
                return null;
            }

            if (column.MysqlType.ToLower().Equals("tinyint(1)"))
            {
                targetType = (typeof(bool), AntData.ORM.DataType.Boolean);
            }

            DataParameter param = new DataParameter();
            param.Name = column.Name;
            param.DataType = targetType.Item2;


            if (column.IsNull)
            {
                param.Value = null;
            }
            else
            {
                param.Value = Canal.SqlParse.StaticExt.TypeConvertUtils.Parse(column.Value, targetType.Item1);
            }

            return param;
        }
    }
}
