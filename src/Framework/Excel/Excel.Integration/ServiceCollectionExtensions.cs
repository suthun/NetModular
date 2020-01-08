﻿using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NetModular.Lib.Excel.Abstractions;
using NetModular.Lib.Utils.Core;
using NetModular.Lib.Utils.Core.Extensions;
using NetModular.Lib.Utils.Core.Helpers;
using NetModular.Lib.Utils.Core.Options;

namespace NetModular.Lib.Excel.Integration
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加Excel
        /// </summary>
        /// <param name="services"></param>
        /// <param name="environmentName"></param>
        /// <param name="moduleCommonOptions"></param>
        /// <returns></returns>
        public static IServiceCollection AddExcel(this IServiceCollection services, string environmentName, ModuleCommonOptions moduleCommonOptions)
        {
            var options = new ConfigurationHelper().Get<ExcelOptions>("Excel", environmentName);
            if (options == null)
                return services;

            if (options.TempPath.IsNull())
            {
                options.TempPath = Path.Combine(moduleCommonOptions.TempPath, "Excel");
            }

            services.AddSingleton(options);

            var assembly = AssemblyHelper.LoadByNameEndString($".Lib.Excel.{options.Provider.ToDescription()}");

            Check.NotNull(assembly, $"Excel实现程序集{options.Provider.ToDescription()}未找到");

            var configType = assembly.GetTypes().FirstOrDefault(m => m.Name.Equals("ServiceCollectionConfig"));
            if (configType != null)
            {
                var instance = (IServiceCollectionConfig)Activator.CreateInstance(configType);
                instance.Config(services, options);
            }

            return services;
        }
    }
}
