using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Canal.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MysqlCanalMq.Server.RabbitMq;

namespace MysqlCanalMq.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            NLog.LogManager.LoadConfiguration("nlog.config");

            var Configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"))
                .AddEnvironmentVariables().Build();


            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.UseCanalService(produce=> produce.RegisterSingleton<RabbitHandler>());

                    services.Configure<RabitMqOption>(Configuration.GetSection("Rabbit"));

                    services.AddSingleton<IConfiguration>(Configuration);

                });

            builder.Build().Run();

        }
    }
}
