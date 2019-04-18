using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CanalSharp.Common.Logging;
using DbModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MysqlCanalMq.Canal;
using MysqlCanalMq.Common.RabitMQ;
using MysqlCanalMq.Db;
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

            services.AddHostedService<CanalService>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.Configure<CanalOption>(Configuration.GetSection("Canal"));
            services.Configure<RabitMqOption>(Configuration.GetSection("Rabit"));

            //#region AntORM
            //services.AddMysqlEntitys<DB>("from", ops =>
            //{
            //    ops.IsEnableLogTrace = false;
            //});
            //MysqlCanalMqDbInfo.UseDb<DB>();
            //#endregion
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

            //#region AntORM

            //AntData.ORM.Common.Configuration.UseDBConfig(Configuration);

            //#endregion
            //设置 NLog
            CanalSharpLogManager.LoggerFactory.AddNLog();


        }
    }
}
