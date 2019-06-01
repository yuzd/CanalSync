using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MysqlCanalMq.Common.Produce.RabbitMq;
using MysqlCanalMq.Server.Canal;
using MysqlCanalMq.Server.Canal.OutPut;
using MysqlCanalMq.Server.Models;

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

            var outPutList = Configuration.GetSection("Canal:OutType").Get<List<string>>();
            if (!outPutList.Any())
            {
                var outType = Configuration["canal.outType"];
                if (!string.IsNullOrEmpty(outType))
                {
                    outPutList = outType.Split(':').ToList();
                }
            }
            if (!outPutList.Any())
            {
                throw new ArgumentNullException($"OutType in cannal setting can not be null or empty!");
            }


            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<CanalOption>(Configuration.GetSection("Canal"));
                    foreach (var outType in outPutList)
                    {
                        switch (outType.ToLower())
                        {
                            case "rabbit":
                                services.Configure<RabitMqOption>(Configuration.GetSection("Rabbit"));
                                break;
                            default:
                                throw new NotSupportedException($"OutPutType:{outType} is not supported yet!");
                        }
                    }
                    services.AddHostedService<CanalService>();



                    services.AddMediatR();

                    //处理类改成单例模式
                    services.Replace(new ServiceDescriptor(typeof(INotificationHandler<CanalBody>), typeof(RabbitHandler), ServiceLifetime.Singleton));
                });

            builder.Build().Run();

            
        }
    }
}
