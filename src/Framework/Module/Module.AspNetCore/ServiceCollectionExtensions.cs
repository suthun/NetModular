﻿using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
#if NETSTANDARD2_0
using Microsoft.AspNetCore.Hosting;
#endif
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
#if NETCOREAPP3_1
using Microsoft.Extensions.Hosting;
#endif
using NetModular.Lib.Module.Abstractions;
using NetModular.Lib.Utils.Core;
using NetModular.Lib.Utils.Core.Extensions;
using NetModular.Lib.Utils.Core.Helpers;
using NetModular.Lib.Utils.Core.Options;

namespace NetModular.Lib.Module.AspNetCore
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加模块
        /// </summary>
        /// <param name="services"></param>
        /// <param name="environmentName">环境名称</param>
        /// <param name="moduleCommonOptions"></param>
        /// <returns></returns>
        public static IModuleCollection AddModules(this IServiceCollection services, string environmentName, out ModuleCommonOptions moduleCommonOptions)
        {
            moduleCommonOptions = null;
            var modules = new ModuleCollection();
            services.AddSingleton<IModuleCollection>(modules);

            //通用配置
            var cfg = new ConfigurationHelper().Load("module", environmentName, true);
            if (cfg == null)
                return modules;

            var options = cfg.Get<ModuleCommonOptions>() ?? new ModuleCommonOptions();
            if (options.UploadPath.IsNull())
            {
                options.UploadPath = Path.Combine(AppContext.BaseDirectory, "Upload");
            }
            if (options.TempPath.IsNull())
            {
                options.TempPath = Path.Combine(AppContext.BaseDirectory, "Temp");
            }

            services.AddSingleton(options);

            services.Configure<ModuleCommonOptions>(m =>
            {
                m.UploadPath = options.UploadPath;
                m.TempPath = options.TempPath;
            });

            moduleCommonOptions = options;

            foreach (var module in modules)
            {
                if (module == null)
                    continue;

                services.AddApplicationServices(module);

                //加载模块配置项
                var optionsConfigureType = module.AssemblyDescriptor.Infrastructure.GetTypes().FirstOrDefault(m => typeof(IModuleOptionsConfigure).IsAssignableFrom(m));
                if (optionsConfigureType != null)
                {
                    ((IModuleOptionsConfigure)Activator.CreateInstance(optionsConfigureType)).ConfigOptions(services, cfg.GetSection(module.Id));
                }

                services.AddSingleton(module);
            }

            return modules;
        }

        /// <summary>
        /// 添加模块的自定义服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="modules"></param>
        /// <param name="env"></param>
        /// <returns></returns>
#if NETSTANDARD2_0
        public static IServiceCollection AddModuleServices(this IServiceCollection services, IModuleCollection modules, IHostingEnvironment env)
#elif NETCOREAPP3_1
        public static IServiceCollection AddModuleServices(this IServiceCollection services, IModuleCollection modules, IHostEnvironment env)
#endif
        {
            foreach (var module in modules)
            {
                //加载模块初始化器
                ((ModuleDescriptor)module).Initializer?.ConfigureServices(services, env);
            }

            return services;
        }

        /// <summary>
        /// 添加应用服务
        /// </summary>
        private static void AddApplicationServices(this IServiceCollection services, IModuleDescriptor module)
        {
            if (module.AssemblyDescriptor == null)
                return;

            var types = module.AssemblyDescriptor.Application.GetTypes();
            var interfaces = types.Where(t => t.FullName != null && t.IsInterface && t.FullName.EndsWith("Service", StringComparison.OrdinalIgnoreCase));
            foreach (var serviceType in interfaces)
            {
                var implementationType = types.FirstOrDefault(m => m != serviceType && serviceType.IsAssignableFrom(m));
                if (implementationType != null)
                {
                    services.Add(new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Singleton));
                }
            }
        }

        /// <summary>
        /// 自动注入单例服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="module"></param>
        private static void AddSingleton(this IServiceCollection services, IModuleDescriptor module)
        {
            if (module.AssemblyDescriptor != null && module.AssemblyDescriptor is ModuleAssemblyDescriptor descriptor)
            {
                services.AddSingletonFromAssembly(descriptor.Domain);
                services.AddSingletonFromAssembly(descriptor.Infrastructure);
                services.AddSingletonFromAssembly(descriptor.Application);
                services.AddSingletonFromAssembly(descriptor.Web);
                services.AddSingletonFromAssembly(descriptor.Api);
            }
        }
    }
}
