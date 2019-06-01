using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MysqlCanalMq.Common.Interface;
using MysqlCanalMq.Common.Produce.RabbitMq;
using MysqlCanalMq.Server.Models;
using Polly;

namespace MysqlCanalMq.Server.Canal.OutPut
{
    public class RabbitHandler : INotificationHandler<CanalBody>,IDisposable
    {
        private readonly ILogger _logger;
        private readonly RabitMqOption _rabitMqOption;
        private readonly IProduce _produceRabbitMq;
        private readonly IConfiguration _configuration;
        public RabbitHandler(ILogger<RabbitHandler> logger, IOptions<RabitMqOption> rabbitMqOption, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _rabitMqOption = rabbitMqOption.Value;
            if (_rabitMqOption == null)
            {
                _rabitMqOption = new RabitMqOption();
            }

            UpdateFromEnv(_rabitMqOption);

            if (string.IsNullOrEmpty(_rabitMqOption.Host) || string.IsNullOrEmpty(_rabitMqOption.UserName) ||
                string.IsNullOrEmpty(_rabitMqOption.Password) || _rabitMqOption.Port < 1)
            {
                throw new ArgumentNullException("Rabbit param in appsettings.json is not correct!");
            }

            try
            {

                _produceRabbitMq = OutPutFactory.CreateRabitMqProduce(_rabitMqOption);
                _logger.LogInformation("rabbit produce client start success!");
            }
            catch (Exception e)
            {
                logger.LogInformation(e,"rabbit produce client start err!");
                throw;
            }
        }

        public Task Handle(CanalBody notification, CancellationToken cancellationToken)
        {
            var message = notification.Message;
            //过滤
            if (_rabitMqOption.DbTables.Any() && !_rabitMqOption.DbTables.Contains(message.DbName + "." + message.TableName))
            {
                return Task.CompletedTask;
            }

            var ploicy = Policy.Handle<Exception>()
                .WaitAndRetry(new[]
                {
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(4),
                    TimeSpan.FromSeconds(6),
                    TimeSpan.FromSeconds(8),
                    TimeSpan.FromSeconds(15),
                    TimeSpan.FromSeconds(30)
                });
            try
            {
                
                ploicy.Execute(() => _produceRabbitMq.Produce(message));
            }
            catch (Exception)
            {
                _logger.LogError("rabbit mq send fail");
                throw;
            }
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _produceRabbitMq.Dispose();
        }


        private void UpdateFromEnv(RabitMqOption _rab)
        {
            var host = _configuration["rabbit.address"];
            if (!string.IsNullOrEmpty(host))
            {
                _rab.Host = host.Split(':')[0];
                _rab.Port = int.Parse(host.Split(':')[1]);
            }

            var virtualHost = _configuration["rabbit.virtualHost"];
            if (!string.IsNullOrEmpty(virtualHost))
            {
                _rab.VirtualHost = virtualHost;
            }

            var userName = _configuration["rabbit.userName"];
            if (!string.IsNullOrEmpty(userName))
            {
                _rab.UserName = userName;
            }

            var password = _configuration["rabbit.password"];
            if (!string.IsNullOrEmpty(password))
            {
                _rab.Password = password;
            }

            var dbTables = _configuration["rabbit.dbTables"];
            if (!string.IsNullOrEmpty(dbTables))
            {
                _rab.DbTables = dbTables.Split(':').ToList();
            }
            
            var confirmSelect = _configuration["canal.confirmSelect"];
            if (!string.IsNullOrEmpty(confirmSelect))
            {
                _rab.ConfirmSelect = confirmSelect.Equals("true") || confirmSelect.Equals("1");
            }

        }
    }



}
