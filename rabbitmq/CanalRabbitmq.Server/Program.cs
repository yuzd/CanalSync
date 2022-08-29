using System;
using System.IO;
using Canal.Server;
using CanalRabbitmq.Server.Produce.RabbitMq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog.Extensions.Logging;

namespace CanalRabbitmq.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            NLog.LogManager.LoadConfiguration("NLog.Config");

            var Configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"))
                .AddEnvironmentVariables().Build();


            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging(config => config.AddNLog());

                    services.AddCanalService(produce=> produce.RegisterSingleton<RabbitHandler>());

                    services.Configure<RabitMqOption>(Configuration.GetSection("Rabbit"));

                    services.AddSingleton<IConfiguration>(Configuration);

                });

            builder.Build().Run();

        }
    }
}
