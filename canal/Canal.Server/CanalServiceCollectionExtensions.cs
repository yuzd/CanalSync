using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Canal.Server.Interface;
using Canal.Server.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
namespace Canal.Server
{
    public static class CanalServiceCollectionExtensions
    {
        public static IServiceCollection AddCanalService(this IServiceCollection serviceCollection,Action<CanalConsumeRegister> register ) 
        {
            if (register == null) throw new ArgumentNullException(nameof(register));

            var registerModel = new CanalConsumeRegister();

            register(registerModel);

            if (!registerModel.ConsumeList.Any() && !registerModel.SingletonConsumeList.Any())
            {
                throw new ArgumentNullException(nameof(registerModel.ConsumeList));
            }

            serviceCollection.AddOptions();

            serviceCollection.TryAddSingleton<IConfigureOptions<CanalOption>, ConfigureCanalOption>();

            serviceCollection.AddTransient<IHostedService,CanalService>();


            if (registerModel.ConsumeList.Any())
            {
                foreach (var type in registerModel.ConsumeList)
                {
                    serviceCollection.TryAddTransient(type);
                }
            }

            if (registerModel.SingletonConsumeList.Any())
            {
                foreach (var type in registerModel.SingletonConsumeList)
                {
                    serviceCollection.TryAddSingleton(type);
                }
            }

            serviceCollection.AddSingleton(registerModel);

            return serviceCollection;

        }

    }

    public class CanalConsumeRegister
    {
        internal List<Type> ConsumeList { get; set; } = new List<Type>();

        internal List<Type> SingletonConsumeList { get; set; } = new List<Type>();

        /// <summary>
        /// 注册消费组件 单例模式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public CanalConsumeRegister RegisterSingleton<T>() where T : INotificationHandler<CanalBody>
        {
            var type = typeof(T);
            if (!SingletonConsumeList.Contains(type))
            {
                SingletonConsumeList.Add(type);
            }
            return this;
        }

        /// <summary>
        /// 注册消费主键 多例模式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public CanalConsumeRegister Register<T>() where T : INotificationHandler<CanalBody>
        {
            var type = typeof(T);
            if (!ConsumeList.Contains(type))
            {
                ConsumeList.Add(type);
            }

            return this;
        }

    }

    public class ConfigureCanalOption : IConfigureOptions<CanalOption>
    {
        private readonly IConfiguration configuration;

        public ConfigureCanalOption(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public void Configure(CanalOption options)
        {
            configuration.GetSection("Canal").Bind(options);
        }

    }


}
