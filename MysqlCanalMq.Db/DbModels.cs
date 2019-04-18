using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AntData.ORM;
using AntData.ORM.Data;
using AntData.ORM.Linq;
using AntData.ORM.Mapping;

using MysqlCanalMq.Models;

namespace DbModels
{
	/// <summary>
	/// Database       : test
	/// Data Source    : 192.168.220.128
	/// Server Version : 5.7.19-log
	/// </summary>
	public partial class DB : IEntity
	{
		public IQueryable<Person> People { get { return this.Get<Person>(); } }

		private readonly DataConnection con;

		public DataConnection DbContext
		{
			get { return this.con; }
		}

		public IQueryable<T> Get<T>()
			 where T : class
		{
			return this.con.GetTable<T>();
		}

		public DB(DataConnection con)
		{
			this.con = con;
		}
	}

	[Table(Db="test", Name="person")]
	public partial class Person : CanalMqBasic
	{
		#region Column

		/// <summary>
		/// 主键
		/// </summary>
		[Column("Id",   DataType=AntData.ORM.DataType.VarChar, Length=60, Comment="主键"), PrimaryKey, NotNull]
		public virtual string Id { get; set; } // varchar(60)

		/// <summary>
		/// 姓名
		/// </summary>
		[Column("Name", DataType=AntData.ORM.DataType.VarChar, Length=20, Comment="姓名"),    Nullable]
		public virtual string Name { get; set; } // varchar(20)

		#endregion
	}

	public static partial class TableExtensions
	{
		public static Person FindByBk(this IQueryable<Person> table, string Id)
		{
			return table.FirstOrDefault(t =>
				t.Id == Id);
		}

		public static async Task<Person> FindByBkAsync(this IQueryable<Person> table, string Id)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Id == Id);
		}
	}
}
