using System;
using System.IO;
using System.Linq;
using Canal.Server;
using CanalRabbitmq.Client.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog.Extensions.Logging;

namespace CanalRabbitmq.Client
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
                connectionString = builderConfig["Mysql:ConnectionString"];
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
