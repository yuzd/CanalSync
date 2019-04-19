using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AntData.ORM.Data;
using DbModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MysqlCanalMq.Common.Consume;
using MysqlCanalMq.Common.Consume.RabbitMq;
using MysqlCanalMq.Common.Produce;
using MysqlCanalMq.Common.Produce.RabbitMq;
using MysqlCanalMq.Models;

namespace MysqlCanalMq.Client
{
    public class ConsumerService : IHostedService,IDisposable
    {
        private readonly ILogger _logger;
        private readonly RabitMqOption _rabitMqOption;
        private MQServcieManager _manager;
        private DbContext<DB> _dbContext;
        public ConsumerService(ILogger<ConsumerService> logger, IOptions<RabitMqOption> rabitMqOption,DbContext<DB> dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
            _rabitMqOption = rabitMqOption.Value;
            if (rabitMqOption == null)
            {
                throw new ArgumentNullException("Rabit in appsettings.json is empty!");
            }

            _rabitMqOption = rabitMqOption?.Value;
            if (string.IsNullOrEmpty(_rabitMqOption.Host) || string.IsNullOrEmpty(_rabitMqOption.UserName) ||
                string.IsNullOrEmpty(_rabitMqOption.Password) || _rabitMqOption.Port < 1 || string.IsNullOrEmpty(_rabitMqOption.CanalDestinationName))
            {
                throw new ArgumentNullException("Rabit param in appsettings.json is not correct!");
            }

            if (!ConsumeExtention.ConsumerTypeList.Any())
            {
                throw new ArgumentNullException("no consumer exist!");
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _manager = new MQServcieManager();

                foreach (var dbMapping in ConsumeExtention.ConsumerTypeList)
                {
                    _manager.AddService(new ConsumeService(_rabitMqOption, dbMapping.Key, _dbContext)
                    {
                        OnAction = OnActionOutput
                    });
                }
               
                _manager.OnAction = OnActionOutput;
                _manager.Start();
                AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;
                _logger.LogInformation("rabit consume client start succ...");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "rabit consume client start error...");
            }
            return Task.CompletedTask;
        }

        private void CurrentDomainOnProcessExit(object sender, EventArgs e)
        {
            Dispose();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Dispose();
            return Task.CompletedTask;
        }


        void OnActionOutput(MessageLevel level, string message, Exception ex)
        {
            if (level == MessageLevel.Error)
            {
                if (ex == null)
                {
                    _logger.LogError(message);
                    return;
                }
                _logger.LogError(ex, message);
                return;
            }

            if (level == MessageLevel.Information)
            {
                if (ex != null)
                {
                    _logger.LogInformation(ex, message);
                    return;
                }
                else
                {
                    _logger.LogInformation(message);
                    return;
                }
            }

            if (level == MessageLevel.Warning)
            {
                if (ex != null)
                {
                    _logger.LogWarning(ex,message);
                    return;
                }
                else
                {
                    _logger.LogWarning(message);
                    return;
                }
            }
        }

        private bool isDispose;
        public void Dispose()
        {
            try
            {
                if (isDispose) return;
                isDispose = true;
                _manager.Stop();
                _logger.LogInformation("rabit consume client stop succ...");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "rabit consume client stop error...");
            }
        }
    }

    public static class ConsumeExtention
    {
        public static ConcurrentDictionary<Type,bool> ConsumerTypeList = new ConcurrentDictionary<Type, bool>();

        public static IServiceCollection AddConsumer<T>(
            this IServiceCollection serviceCollection)
            where T : CanalMqBasic
        {
            if (serviceCollection == null)
                throw new ArgumentException("serviceCollection is null");

            if(ConsumerTypeList.TryGetValue(typeof(T),out var _))
            {
                return serviceCollection;
            }

            ConsumerTypeList.TryAdd(typeof(T), true);

            return serviceCollection;
        }
    }
}
