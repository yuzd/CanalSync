using System;
using System.Collections.Generic;
using System.Linq;
using Canal.Server.Models;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
namespace Canal.Server
{
    public static class CanalServiceCollectionExtensions
    {
        public static IServiceCollection UseCanalService(this IServiceCollection serviceCollection,Action<CanalConsumeRegister> register ) 
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

            var typeList = new List<Type>();

            if(registerModel.ConsumeList.Any()) typeList.AddRange(registerModel.ConsumeList);
            if(registerModel.SingletonConsumeList.Any()) typeList.AddRange(registerModel.SingletonConsumeList);

            serviceCollection.AddMediatR(typeList.ToArray());

            foreach (var handler in registerModel.SingletonConsumeList)
            {
                //处理类改成单例模式
                serviceCollection.Replace(new ServiceDescriptor(typeof(INotificationHandler<CanalBody>), handler, ServiceLifetime.Singleton));
            }

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
        public void RegisterSingleton<T>() where T : INotificationHandler<CanalBody>
        {
            var type = typeof(T);
            if (!SingletonConsumeList.Contains(type))
            {
                SingletonConsumeList.Add(type);
            }
        }

        /// <summary>
        /// 注册消费主键 多例模式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Register<T>() where T : INotificationHandler<CanalBody>
        {
            var type = typeof(T);
            if (!ConsumeList.Contains(type))
            {
                ConsumeList.Add(type);
            }
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
