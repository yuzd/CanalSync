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
using NLog.Extensions.Logging;

namespace CanalTransferDb
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
                    services.Configure<MysqlOption>(builderConfig.GetSection("Mysql"));

                    services.AddCanalService(produce => produce.RegisterSingleton<MysqlHandler>());

                    services.AddMysqlParseService(connectionString);


                    services.AddLogging(config => config.AddNLog());

                    services.AddSingleton<IConfiguration>(builderConfig);

                });

            builder.Build().Run();
        }
        
    }
}
