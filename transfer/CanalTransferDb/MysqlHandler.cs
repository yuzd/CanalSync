using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Canal.Server.Interface;
using Canal.Server.Models;
using Canal.SqlParse;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
        public Task Handle(List<CanalBody> notificationList)
        {
            foreach (var notification in notificationList)
            {
                var message = notification.Message;

                //过滤
                if (_option.DbTables.Any() && !_option.DbTables.Contains(message.DbName + "." + message.TableName))
                {
                    return Task.CompletedTask;
                }

                var result = _dbTypeMapper.TransferToDb(message);
                if (!result.Success)
                {
                    _logger.LogError($"Topic:{message.CanalDestination + "." + message.DbName + "." + message.TableName},Message:{Newtonsoft.Json.JsonConvert.SerializeObject(message)},Error:{result.Msg}");
                }

                notification.Succ = true;
            }
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
