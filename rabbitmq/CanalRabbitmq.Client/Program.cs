using System;
using System.IO;
using System.Linq;
using AntData.ORM.Data;
using Canal.Server;
using Canal.SqlParse;
using Canal.SqlParse.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MysqlCanalMq.Server.RabbitMq;
using NLog.Extensions.Logging;

namespace MysqlCanalMq.Client
{
    class Program
    {
        static void Main(string[] args)
        {


            NLog.LogManager.LoadConfiguration("NLog.Config");

            var builderConfig = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"))
                .AddEnvironmentVariables().Build();

            var connectionString = builderConfig["consume.db"];

            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = builderConfig["Mysql.ConnectionString"];
            }

            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<ConsumerService>();

                    services.AddMysqlParseService(connectionString);

                    services.AddLogging(config => config.AddNLog());

                    services.Configure<RabitMqOption>(builderConfig.GetSection("Rabit"));

                    services.AddSingleton<IConfiguration>(builderConfig);

                });

            builder.Build().Run();
        }

    }
}
