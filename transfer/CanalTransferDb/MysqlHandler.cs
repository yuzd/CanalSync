using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AntData.ORM.Data;
using Canal.Server.Interface;
using Canal.Server.Models;
using Canal.SqlParse;
using Canal.SqlParse.Models;
using Canal.SqlParse.StaticExt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CanalTransferDb
{

    public class MysqlHandler : INotificationHandler<CanalBody>
    {
        private readonly ILogger _logger;
        private readonly IDbTransfer _dbTypeMapper;
        private readonly MysqlOption _option;
        private readonly IConfiguration _configuration;

        public MysqlHandler(ILogger<MysqlHandler> logger,IOptions<MysqlOption> options,IDbTransfer dbTypeMapper, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _dbTypeMapper = dbTypeMapper;
            _option = options?.Value;
            if (_option == null)
            {
                _option = new MysqlOption();
            }
            UpdateFromEnv(_option);
            if (_option.DbTables == null || !_option.DbTables.Any())
            {
                throw new ArgumentNullException(nameof(MysqlOption.DbTables));
            }

            _logger.LogInformation($"Mysql Produce Listening: {string.Join(",", _option.DbTables)}");
        }
        public Task Handle(CanalBody notification)
        {
            var message = notification.Message;

            //过滤
            if (_option.DbTables.Any() && !_option.DbTables.Contains(message.DbName + "." + message.TableName))
            {
                return Task.CompletedTask;
            }

            var messageStr = JsonConvert.SerializeObject(message);
            Canal.SqlParse.Models.canal.DataChange data = messageStr.JsonToObject<Canal.SqlParse.Models.canal.DataChange>();
            var result = _dbTypeMapper.TransferToDb(data);
            if (!result.Success)
            {
                _logger.LogError($"Topic:{message.CanalDestination+"."+message.DbName+"."+message.TableName},Message:{messageStr},Error:{result.Msg}");
            }

            notification.Succ = true;
            return Task.CompletedTask;
        }

        private void UpdateFromEnv(MysqlOption _rab)
        {
            var dbTables = _configuration["mysql.dbTables"];
            if (!string.IsNullOrEmpty(dbTables))
            {
                _rab.DbTables = dbTables.Split(':').ToList();
            }
        }
    }
}
