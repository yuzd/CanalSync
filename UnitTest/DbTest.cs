using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AntData.ORM;
using AntData.ORM.Data;
using DbModels;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class DbTest
    {
        private static MysqlDbContext<DB> DB
        {
            get
            {
                var db = new MysqlDbContext<DB>("test");
                db.IsEnableLogTrace = true;
                db.OnLogTrace = OnCustomerTraceConnection;
                return db;
            }
        }
        private static void OnCustomerTraceConnection(CustomerTraceInfo customerTraceInfo)
        {
            try
            {
                string sql = customerTraceInfo.CustomerParams.Aggregate(customerTraceInfo.SqlText,
                    (current, item) => current.Replace(item.Key, item.Value.Value.ToString()));
                Trace.Write(sql + Environment.NewLine);
            }
            catch (Exception)
            {
                Trace.Write(customerTraceInfo.SqlText + Environment.NewLine);
                //ignore
            }
        }
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            var builderConfig = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json")).Build();
            AntData.ORM.Common.Configuration.UseDBConfig(builderConfig);
            AntData.ORM.Common.Configuration.Linq.AllowMultipleQuery = true;
        }
        [TestMethod]
        public void TestMethod1()
        {
            for (int i = 0; i < 10000; i++)
            {
                var p = new Person
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Name = "name" + i
                };
                DB.Insert(p);
            }
        }
    }
}
