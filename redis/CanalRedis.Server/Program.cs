using System;
using System.IO;
using Canal.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog.Extensions.Logging;

namespace CanalRedis.Server
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
                    services.UseCanalService(produce=> produce.RegisterSingleton<RedisHandler>());

                    services.Configure<RedisOption>(Configuration.GetSection("Redis"));

                    services.AddLogging(config => config.AddNLog());

                    services.AddSingleton<IConfiguration>(Configuration);

                });

            builder.Build().Run();
        }
    }
}