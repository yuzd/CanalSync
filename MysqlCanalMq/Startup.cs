using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CanalSharp.Common.Logging;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using MysqlCanalMq.Canal;
using MysqlCanalMq.Canal.OutPut;
using MysqlCanalMq.Common.Produce;
using MysqlCanalMq.Common.Produce.RabbitMq;
using MysqlCanalMq.Models;
using NLog.Extensions.Logging;

namespace MysqlCanalMq
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            var outPutList = Configuration.GetSection("Canal:OutType").Get<List<string>>();
            if (!outPutList.Any())
            {
                throw new ArgumentNullException($"OutType in cannal setting can not be null or empty!");
            }
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
            services.Replace(new ServiceDescriptor(typeof(INotificationHandler<CanalBody>),typeof(RabbitHandler),ServiceLifetime.Singleton));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory logging)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            #region NLOG
            NLog.LogManager.LoadConfiguration("nlog.config");
            logging.AddNLog();
            #endregion

            //设置 NLog
            CanalSharpLogManager.LoggerFactory.AddNLog();


        }
    }
}
