using System;
using System.IO;
using System.Linq;
using AntData.ORM.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MysqlCanalMq.Common.Models;
using MysqlCanalMq.Common.SqlParse;
using MysqlCanalMq.Server.RabbitMq;
using NLog.Extensions.Logging;

namespace MysqlCanalMq.Client
{
    class Program
    {
        static void Main(string[] args)
        {


            NLog.LogManager.LoadConfiguration("nlog.config");

            var builderConfig = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"))
                .AddEnvironmentVariables().Build();

            AntData.ORM.Common.Configuration.Linq.AllowMultipleQuery = true;

            AntData.ORM.Common.Configuration.UseDBConfig(builderConfig);

            var connectionString = builderConfig["consume.db"];
            if (!string.IsNullOrEmpty(connectionString))
            {
                var dbConnectionConfig = AntData.ORM.Common.Configuration.DBSettings.DatabaseSettings.First().ConnectionItemList.First();
                dbConnectionConfig.ConnectionString = connectionString;
                Console.WriteLine(connectionString);
            }

            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<ConsumerService>();
                    services.AddMysqlEntitys<DB>("to", ops =>
                    {
                        ops.IsEnableLogTrace = false;
                        ops.OnLogTrace = OnLogTrace;
                    });

                    services.AddLogging(config => config.AddNLog());

                    services.Configure<RabitMqOption>(builderConfig.GetSection("Rabit"));

                    services.AddSingleton<IConfiguration>(builderConfig);

                    services.AddSingleton<IDbTypeMapper>(new MysqlDbTypeMapper());
                });

            builder.Build().Run();
        }

        private static void OnLogTrace(CustomerTraceInfo obj)
        {

        }
    }
}
