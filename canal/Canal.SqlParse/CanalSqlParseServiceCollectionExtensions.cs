using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Canal.SqlParse;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
namespace Canal.Server
{
    public static class CanalSqlParseServiceCollectionExtensions
    {
        public static IServiceCollection UseMysqlParseService(this IServiceCollection serviceCollection) 
        {

            serviceCollection.TryAddSingleton<IDbTypeMapper, MysqlDbTypeMapper>();

            return serviceCollection;

        }

    }



}
