using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AntData.ORM.Data;
using Canal.Server.Models;
using Canal.SqlParse;
using Canal.SqlParse.Models;
using Canal.SqlParse.StaticExt;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CanalTransferDb
{
    public class MysqlHandler : INotificationHandler<CanalBody>
    {
        private readonly ILogger _logger;
        private readonly DbContext<DB> _dbContext;
        private readonly IDbTypeMapper _dbTypeMapper;
        private readonly MysqlOption _option;
        public MysqlHandler(ILogger<MysqlHandler> logger,IOptions<MysqlOption> options, DbContext<DB> dbContext, IDbTypeMapper dbTypeMapper)
        {
            _logger = logger;
            _dbTypeMapper = dbTypeMapper;
            _dbContext = dbContext;
            _option = options?.Value;

            if (_option == null || _option.DbTables == null || !_option.DbTables.Any())
            {
                throw new ArgumentNullException(nameof(MysqlOption.DbTables));
            }

            _logger.LogInformation($"Mysql Produce Listening: {string.Join(",", _option.DbTables)}");
        }
        public Task Handle(CanalBody notification, CancellationToken cancellationToken)
        {
            var message = notification.Message;

            //过滤
            if (_option.DbTables.Any() && !_option.DbTables.Contains(message.DbName + "." + message.TableName))
            {
                return Task.CompletedTask;
            }

            var messageStr = JsonConvert.SerializeObject(message);
            Canal.SqlParse.Models.canal.DataChange data = messageStr.JsonToObject<Canal.SqlParse.Models.canal.DataChange>();
            var result = _dbTypeMapper.TransferToDb(this._dbContext, data);
            if (!result.Item1)
            {
                _logger.LogError($"Topic:{message.CanalDestination+"."+message.DbName+"."+message.TableName},Message:{messageStr},Error:{result.Item2}");
            }

            notification.Succ = true;
            return Task.CompletedTask;
        }

      
    }
}
