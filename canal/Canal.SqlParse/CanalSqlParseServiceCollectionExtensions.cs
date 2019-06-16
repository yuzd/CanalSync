using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AntData.ORM.DbEngine.Configuration;
using AntData.ORM.Enums;
using Canal.SqlParse;
using Canal.SqlParse.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
namespace Canal.Server
{
    public static class CanalSqlParseServiceCollectionExtensions
    {
        public static IServiceCollection AddMysqlParseService(this IServiceCollection serviceCollection,string connectionString) 
        {
            if(string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException(nameof(connectionString));

            List<DatabaseSettings> databaseSettingsList = new List<DatabaseSettings>();
            databaseSettingsList.Add(new DatabaseSettings
            {
                Name = "to",
                Provider = "mysql",
                ConnectionItemList = new List<ConnectionStringItem>
                {
                    new ConnectionStringItem
                    {
                        ConnectionString = connectionString,
                        Name = "to",
                        DatabaseType = DatabaseType.Master
                    }
                }
            });
            AntData.ORM.Common.Configuration.Linq.AllowMultipleQuery = true;
            AntData.ORM.Common.Configuration.DBSettings = new DBSettings()
            {
                DatabaseSettings = databaseSettingsList
            };
            serviceCollection.TryAddSingleton<IDbTransfer, MysqlDbTypeMapper>();
            serviceCollection.AddMysqlEntitys<DB>("to", ops =>
            {
                ops.IsEnableLogTrace = false;
            });
            return serviceCollection;

        }

    }



}
