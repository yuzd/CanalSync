using System;
using System.IO;
using AntData.ORM.Data;
using DbModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MysqlCanalMq.Client;
using MysqlCanalMq.Common.RabitMQ;
using MysqlCanalMq.Db;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;

namespace MqClient
{
    class Program
    {
        static void Main(string[] args)
        {
            NLog.LogManager.LoadConfiguration("nlog.config");

            var builderConfig = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json")).Build();

            AntData.ORM.Common.Configuration.Linq.AllowMultipleQuery = true;

            AntData.ORM.Common.Configuration.UseDBConfig(builderConfig);

            MysqlCanalMqDbInfo.UseDb<DB>();

            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<ConsumerService>();
                    services.AddMysqlEntitys<DB>("to", ops =>
                    {
                        ops.IsEnableLogTrace = true;
                        ops.OnLogTrace= OnLogTrace;
                    });
                    services.AddLogging(config => config.AddNLog());

                    services.Configure<RabitMqOption>(builderConfig.GetSection("Rabit"));

                    services.AddConsumer<Person>();
                });

            builder.Build().Run();
        }

        private static void OnLogTrace(CustomerTraceInfo obj)
        {

        }
    }
}
