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
	/// Database       : lito
	/// Data Source    : test2019.litozoey.com
	/// Server Version : 5.7.24
	/// </summary>
	public partial class DB : IEntity
	{
		/// <summary>
		/// 品牌表
		/// </summary>
		public IQueryable<Brand>                     Brands                    { get { return this.Get<Brand>(); } }
		/// <summary>
		/// 购物车
		/// </summary>
		public IQueryable<Cart>                      Carts                     { get { return this.Get<Cart>(); } }
		/// <summary>
		/// 分类
		/// </summary>
		public IQueryable<Category>                  Categories                { get { return this.Get<Category>(); } }
		public IQueryable<CategoryImportRelation>    CategoryImportRelation    { get { return this.Get<CategoryImportRelation>(); } }
		/// <summary>
		/// 渠道表
		/// </summary>
		public IQueryable<Channel>                   Channels                  { get { return this.Get<Channel>(); } }
		public IQueryable<Color>                     Colors                    { get { return this.Get<Color>(); } }
		/// <summary>
		/// 配置表
		/// </summary>
		public IQueryable<Config>                    Configs                   { get { return this.Get<Config>(); } }
		/// <summary>
		/// 对渠道配置的折扣字典，用于自动上货
		/// </summary>
		public IQueryable<ConfigDiscount>            ConfigDiscount            { get { return this.Get<ConfigDiscount>(); } }
		/// <summary>
		/// 交付订单
		/// </summary>
		public IQueryable<DeliveryOrder>             DeliveryOrder             { get { return this.Get<DeliveryOrder>(); } }
		/// <summary>
		/// 商品详情表
		/// </summary>
		public IQueryable<Detail>                    Details                   { get { return this.Get<Detail>(); } }
		/// <summary>
		/// 详情翻译表
		/// </summary>
		public IQueryable<DetailTranslation>         DetailTranslation         { get { return this.Get<DetailTranslation>(); } }
		public IQueryable<FreightCategoryRelation>   FreightCategoryRelation   { get { return this.Get<FreightCategoryRelation>(); } }
		public IQueryable<FreightRegionTypeRelation> FreightRegionTypeRelation { get { return this.Get<FreightRegionTypeRelation>(); } }
		/// <summary>
		/// 运费模板
		/// </summary>
		public IQueryable<FreightTemplate>           FreightTemplate           { get { return this.Get<FreightTemplate>(); } }
		/// <summary>
		/// 商品
		/// </summary>
		public IQueryable<Good>                      Goods                     { get { return this.Get<Good>(); } }
		/// <summary>
		/// 商品分类关系
		/// </summary>
		public IQueryable<GoodCategory>              GoodCategory              { get { return this.Get<GoodCategory>(); } }
		/// <summary>
		/// js_report
		/// </summary>
		public IQueryable<JsReport>                  JsReport                  { get { return this.Get<JsReport>(); } }
		/// <summary>
		/// 城市
		/// </summary>
		public IQueryable<LocationCity>              LocationCity              { get { return this.Get<LocationCity>(); } }
		/// <summary>
		/// 国家
		/// </summary>
		public IQueryable<LocationCountry>           LocationCountry           { get { return this.Get<LocationCountry>(); } }
		/// <summary>
		/// 省
		/// </summary>
		public IQueryable<LocationProvince>          LocationProvince          { get { return this.Get<LocationProvince>(); } }
		/// <summary>
		/// Nlog
		/// </summary>
		public IQueryable<Nlog>                      Nlogs                     { get { return this.Get<Nlog>(); } }
		public IQueryable<OssRecordsHangzhou>        OssRecordsHangzhou        { get { return this.Get<OssRecordsHangzhou>(); } }
		/// <summary>
		/// 订单表，Pre-Order的意思是预下单，当订单支付后，会产生若干个DeliveryOrder（交付订单），交付订单才是真正和快递信息绑定的订单。
		/// </summary>
		public IQueryable<Preorder>                  Preorders                 { get { return this.Get<Preorder>(); } }
		public IQueryable<RegionType>                RegionType                { get { return this.Get<RegionType>(); } }
		public IQueryable<SalePrice>                 SalePrice                 { get { return this.Get<SalePrice>(); } }
		public IQueryable<SizeGuide>                 SizeGuide                 { get { return this.Get<SizeGuide>(); } }
		/// <summary>
		/// 款式
		/// </summary>
		public IQueryable<Sku>                       Skus                      { get { return this.Get<Sku>(); } }
		/// <summary>
		/// 系统菜单表
		/// </summary>
		public IQueryable<SystemMenu>                SystemMenu                { get { return this.Get<SystemMenu>(); } }
		/// <summary>
		/// 菜单按钮
		/// </summary>
		public IQueryable<SystemPageAction>          SystemPageAction          { get { return this.Get<SystemPageAction>(); } }
		/// <summary>
		/// 角色表
		/// </summary>
		public IQueryable<SystemRole>                SystemRole                { get { return this.Get<SystemRole>(); } }
		/// <summary>
		/// 后台系统用户表
		/// </summary>
		public IQueryable<SystemUsers>               SystemUsers               { get { return this.Get<SystemUsers>(); } }
		public IQueryable<Translation>               Translations              { get { return this.Get<Translation>(); } }
		/// <summary>
		/// 用户表
		/// </summary>
		public IQueryable<User>                      Users                     { get { return this.Get<User>(); } }
		/// <summary>
		/// 用户收件地址
		/// </summary>
		public IQueryable<UserAddress>               UserAddress               { get { return this.Get<UserAddress>(); } }
		/// <summary>
		/// 存储用户的身份认证信息，用于报关
		/// </summary>
		public IQueryable<UserCertificationInfo>     UserCertificationInfo     { get { return this.Get<UserCertificationInfo>(); } }

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

	/// <summary>
	/// 品牌表
	/// </summary>
	[Table(Database="lito", Comment="品牌表", Name="brand")]
	public partial class Brand : CanalMqBasic
	{
		#region Column

		/// <summary>
		/// 主键
		/// </summary>
		[Column("Tid",                DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="主键"), PrimaryKey, NotNull]
		public virtual string Tid { get; set; } // varchar(32)

		/// <summary>
		/// 英文名
		/// </summary>
		[Column("Name",               DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="英文名"), NotNull]
		public virtual string Name { get; set; } // varchar(255)

		/// <summary>
		/// 品牌介绍
		/// </summary>
		[Column("Description",        DataType=AntData.ORM.DataType.Text,     Length=4294967295, Comment="品牌介绍"),    Nullable]
		public virtual string Description { get; set; } // longtext

		/// <summary>
		/// 品牌所属国家
		/// </summary>
		[Column("Origin",             DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="品牌所属国家"),    Nullable]
		public virtual string Origin { get; set; } // varchar(255)

		/// <summary>
		/// 最后更新时间
		/// </summary>
		[Column("DataChangeLastTime", DataType=AntData.ORM.DataType.DateTime, Comment="最后更新时间"), NotNull]
		public virtual DateTime DataChangeLastTime // datetime
		{
			get { return _DataChangeLastTime; }
			set { _DataChangeLastTime = value; }
		}

		/// <summary>
		/// 分类集合
		/// </summary>
		[Column("Tags",               DataType=AntData.ORM.DataType.VarChar,  Length=1024, Comment="分类集合"),    Nullable]
		public virtual string Tags { get; set; } // varchar(1024)

		/// <summary>
		/// 分类ID集合
		/// </summary>
		[Column("TagIds",             DataType=AntData.ORM.DataType.VarChar,  Length=1024, Comment="分类ID集合"),    Nullable]
		public virtual string TagIds { get; set; } // varchar(1024)

		/// <summary>
		/// 模糊查询用
		/// </summary>
		[Column("SearchKey",          DataType=AntData.ORM.DataType.VarChar,  Length=1024, Comment="模糊查询用"),    Nullable]
		public virtual string SearchKey { get; set; } // varchar(1024)

		#endregion

		#region Field

		private DateTime _DataChangeLastTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;

		#endregion
	}

	/// <summary>
	/// 购物车
	/// </summary>
	[Table(Database="lito", Comment="购物车", Name="cart")]
	public partial class Cart : CanalMqBasic
	{
		#region Column

		/// <summary>
		/// 主键
		/// </summary>
		[Column("Tid",                DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="主键"), PrimaryKey, NotNull]
		public virtual string Tid { get; set; } // varchar(32)

		/// <summary>
		/// 用户Tid
		/// </summary>
		[Column("UserTid",            DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="用户Tid"), NotNull]
		public virtual string UserTid { get; set; } // varchar(32)

		/// <summary>
		/// SkuTid
		/// </summary>
		[Column("SkuTid",             DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="SkuTid"), NotNull]
		public virtual string SkuTid { get; set; } // varchar(32)

		/// <summary>
		/// 商品的Tid
		/// </summary>
		[Column("GoodTid",            DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="商品的Tid"),    Nullable]
		public virtual string GoodTid { get; set; } // varchar(32)

		/// <summary>
		/// 添加购物车时的单价
		/// </summary>
		[Column("PriceWhenAddToCart", DataType=AntData.ORM.DataType.Decimal,  Precision=10, Scale=0, Comment="添加购物车时的单价"), NotNull]
		public virtual decimal PriceWhenAddToCart { get; set; } // decimal(10,0)

		/// <summary>
		/// 件数
		/// </summary>
		[Column("Count",              DataType=AntData.ORM.DataType.Int32,    Comment="件数"), NotNull]
		public virtual int Count { get; set; } // int(11)

		/// <summary>
		/// 添加时间
		/// </summary>
		[Column("CreateTime",         DataType=AntData.ORM.DataType.DateTime, Comment="添加时间"), NotNull]
		public virtual DateTime CreateTime // datetime
		{
			get { return _CreateTime; }
			set { _CreateTime = value; }
		}

		/// <summary>
		/// 是否选中
		/// </summary>
		[Column("IsSelected",         DataType=AntData.ORM.DataType.Boolean,  Comment="是否选中"), NotNull]
		public virtual bool IsSelected { get; set; } // tinyint(1)

		/// <summary>
		/// 最后更新时间
		/// </summary>
		[Column("DataChangeLastTime", DataType=AntData.ORM.DataType.DateTime, Comment="最后更新时间"), NotNull]
		public virtual DateTime DataChangeLastTime // datetime
		{
			get { return _DataChangeLastTime; }
			set { _DataChangeLastTime = value; }
		}

		#endregion

		#region Field

		private DateTime _CreateTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;
		private DateTime _DataChangeLastTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;

		#endregion
	}

	/// <summary>
	/// 分类
	/// </summary>
	[Table(Database="lito", Comment="分类", Name="categories")]
	public partial class Category : CanalMqBasic
	{
		#region Column

		/// <summary>
		/// 主键
		/// </summary>
		[Column("Tid",                DataType=AntData.ORM.DataType.Int64,    Comment="主键"), PrimaryKey, Identity]
		public virtual long Tid { get; set; } // bigint(20)

		/// <summary>
		/// 最后更新时间
		/// </summary>
		[Column("DataChangeLastTime", DataType=AntData.ORM.DataType.DateTime, Comment="最后更新时间"), NotNull]
		public virtual DateTime DataChangeLastTime // datetime
		{
			get { return _DataChangeLastTime; }
			set { _DataChangeLastTime = value; }
		}

		/// <summary>
		/// 分类
		/// </summary>
		[Column("Name",               DataType=AntData.ORM.DataType.VarChar,  Length=100, Comment="分类"),    Nullable]
		public virtual string Name { get; set; } // varchar(100)

		/// <summary>
		/// 父分类Tid
		/// </summary>
		[Column("ParentTid",          DataType=AntData.ORM.DataType.Int64,    Comment="父分类Tid"), NotNull]
		public virtual long ParentTid { get; set; } // bigint(20)

		/// <summary>
		/// 等级
		/// </summary>
		[Column("Level",              DataType=AntData.ORM.DataType.Int32,    Comment="等级"), NotNull]
		public virtual int Level { get; set; } // int(11)

		/// <summary>
		/// 访问URL
		/// </summary>
		[Column("Url",                DataType=AntData.ORM.DataType.VarChar,  Length=100, Comment="访问URL"),    Nullable]
		public virtual string Url { get; set; } // varchar(100)

		/// <summary>
		/// 图片地址
		/// </summary>
		[Column("ImgUrl",             DataType=AntData.ORM.DataType.VarChar,  Length=1024, Comment="图片地址"),    Nullable]
		public virtual string ImgUrl { get; set; } // varchar(1024)

		/// <summary>
		/// node是否打开
		/// </summary>
		[Column("Open",               DataType=AntData.ORM.DataType.Boolean,  Comment="node是否打开"), NotNull]
		public virtual bool Open { get; set; } // tinyint(1)

		/// <summary>
		/// 分级名称拼接
		/// </summary>
		[Column("ParentArr",          DataType=AntData.ORM.DataType.VarChar,  Length=1024, Comment="分级名称拼接"),    Nullable]
		public virtual string ParentArr { get; set; } // varchar(1024)

		/// <summary>
		/// 分级Tid拼接
		/// </summary>
		[Column("TidArr",             DataType=AntData.ORM.DataType.VarChar,  Length=1024, Comment="分级Tid拼接"),    Nullable]
		public virtual string TidArr { get; set; } // varchar(1024)

		/// <summary>
		/// 排序
		/// </summary>
		[Column("OrderRule",          DataType=AntData.ORM.DataType.Int32,    Comment="排序"), NotNull]
		public virtual int OrderRule { get; set; } // int(11)

		/// <summary>
		/// 是否隐藏，隐藏分类用于在首页添加促销链接等功能
		/// </summary>
		[Column("IsHide",             DataType=AntData.ORM.DataType.Boolean,  Comment="是否隐藏，隐藏分类用于在首页添加促销链接等功能"), NotNull]
		public virtual bool IsHide { get; set; } // tinyint(1)

		/// <summary>
		/// 查询Key
		/// </summary>
		[Column("Key",                DataType=AntData.ORM.DataType.VarChar,  Length=50, Comment="查询Key"),    Nullable]
		public virtual string Key { get; set; } // varchar(50)

		/// <summary>
		/// 查询Value
		/// </summary>
		[Column("Value",              DataType=AntData.ORM.DataType.VarChar,  Length=1000, Comment="查询Value"),    Nullable]
		public virtual string Value { get; set; } // varchar(1000)

		#endregion

		#region Field

		private DateTime _DataChangeLastTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;

		#endregion
	}

	[Table(Database="lito", Name="category_import_relation")]
	public partial class CategoryImportRelation : CanalMqBasic
	{
		#region Column

		[Column("Tid",               DataType=AntData.ORM.DataType.Int64)  , PrimaryKey, Identity]
		public virtual long Tid { get; set; } // bigint(20)

		/// <summary>
		/// 源分类
		/// </summary>
		[Column("FromCategory",      DataType=AntData.ORM.DataType.VarChar, Length=255, Comment="源分类"), NotNull]
		public virtual string FromCategory { get; set; } // varchar(255)

		/// <summary>
		/// 年龄段
		/// </summary>
		[Column("Age",               DataType=AntData.ORM.DataType.Int32,   Comment="年龄段"), NotNull]
		public virtual int Age { get; set; } // int(11)

		/// <summary>
		/// 性别
		/// </summary>
		[Column("Gender",            DataType=AntData.ORM.DataType.Int32,   Comment="性别"), NotNull]
		public virtual int Gender { get; set; } // int(11)

		/// <summary>
		/// 目标分类Tid
		/// </summary>
		[Column("TargetCategoryTid", DataType=AntData.ORM.DataType.Int64,   Comment="目标分类Tid"), NotNull]
		public virtual long TargetCategoryTid { get; set; } // bigint(20)

		#endregion
	}

	/// <summary>
	/// 渠道表
	/// </summary>
	[Table(Database="lito", Comment="渠道表", Name="channel")]
	public partial class Channel : CanalMqBasic
	{
		#region Column

		/// <summary>
		/// 主键
		/// </summary>
		[Column("Tid",                DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="主键"), PrimaryKey, NotNull]
		public virtual string Tid { get; set; } // varchar(32)

		/// <summary>
		/// 最后更新时间
		/// </summary>
		[Column("DataChangeLastTime", DataType=AntData.ORM.DataType.DateTime, Comment="最后更新时间"),    Nullable]
		public virtual DateTime? DataChangeLastTime { get; set; } // datetime

		/// <summary>
		/// 渠道名称
		/// </summary>
		[Column("Name",               DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="渠道名称"),    Nullable]
		public virtual string Name { get; set; } // varchar(255)

		/// <summary>
		/// 最后更新人
		/// </summary>
		[Column("UpdateUser",         DataType=AntData.ORM.DataType.VarChar,  Length=20, Comment="最后更新人"),    Nullable]
		public virtual string UpdateUser { get; set; } // varchar(20)

		#endregion
	}

	[Table(Database="lito", Name="color")]
	public partial class Color : CanalMqBasic
	{
		#region Column

		/// <summary>
		/// 颜色的英文
		/// </summary>
		[Column("ColorNameEn", DataType=AntData.ORM.DataType.VarChar, Length=255, Comment="颜色的英文"), PrimaryKey, NotNull]
		public virtual string ColorNameEn { get; set; } // varchar(255)

		/// <summary>
		/// 颜色的中文
		/// </summary>
		[Column("ColorNameCh", DataType=AntData.ORM.DataType.VarChar, Length=255, Comment="颜色的中文"), NotNull]
		public virtual string ColorNameCh { get; set; } // varchar(255)

		/// <summary>
		/// 颜色代码，例如#FFFFFF
		/// </summary>
		[Column("ColorCode",   DataType=AntData.ORM.DataType.VarChar, Length=8, Comment="颜色代码，例如#FFFFFF"), NotNull]
		public virtual string ColorCode { get; set; } // varchar(8)

		#endregion
	}

	/// <summary>
	/// 配置表
	/// </summary>
	[Table(Database="lito", Comment="配置表", Name="config")]
	public partial class Config : CanalMqBasic
	{
		#region Column

		/// <summary>
		/// 主键
		/// </summary>
		[Column("Tid",                DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="主键"), PrimaryKey, NotNull]
		public virtual string Tid { get; set; } // varchar(32)

		/// <summary>
		/// 最后更新时间
		/// </summary>
		[Column("DataChangeLastTime", DataType=AntData.ORM.DataType.DateTime, Comment="最后更新时间"),    Nullable]
		public virtual DateTime? DataChangeLastTime { get; set; } // datetime

		/// <summary>
		/// Key
		/// </summary>
		[Column("Key",                DataType=AntData.ORM.DataType.VarChar,  Length=512, Comment="Key"),    Nullable]
		public virtual string Key { get; set; } // varchar(512)

		/// <summary>
		/// Value
		/// </summary>
		[Column("Value",              DataType=AntData.ORM.DataType.VarChar,  Length=2048, Comment="Value"),    Nullable]
		public virtual string Value { get; set; } // varchar(2048)

		/// <summary>
		/// 类型
		/// </summary>
		[Column("Type",               DataType=AntData.ORM.DataType.Int32,    Comment="类型"), NotNull]
		public virtual int Type { get; set; } // int(11)

		#endregion
	}

	/// <summary>
	/// 对渠道配置的折扣字典，用于自动上货
	/// </summary>
	[Table(Database="lito", Comment="对渠道配置的折扣字典，用于自动上货", Name="config_discount")]
	public partial class ConfigDiscount : CanalMqBasic
	{
		#region Column

		[Column("Tid",                DataType=AntData.ORM.DataType.VarChar,   Length=32), PrimaryKey, NotNull]
		public virtual string Tid { get; set; } // varchar(32)

		/// <summary>
		/// 渠道名称
		/// </summary>
		[Column("ChannelName",        DataType=AntData.ORM.DataType.VarChar,   Length=255, Comment="渠道名称"), NotNull]
		public virtual string ChannelName { get; set; } // varchar(255)

		/// <summary>
		/// 品牌
		/// </summary>
		[Column("Brand",              DataType=AntData.ORM.DataType.VarChar,   Length=255, Comment="品牌"), NotNull]
		public virtual string Brand { get; set; } // varchar(255)

		/// <summary>
		/// 渠道商的商品分类
		/// </summary>
		[Column("Category",           DataType=AntData.ORM.DataType.VarChar,   Length=255, Comment="渠道商的商品分类"), NotNull]
		public virtual string Category { get; set; } // varchar(255)

		/// <summary>
		/// 季节 3=春夏，12=秋冬
		/// </summary>
		[Column("Season",             DataType=AntData.ORM.DataType.Int32,     Comment="季节 3=春夏，12=秋冬"), NotNull]
		public virtual int Season { get; set; } // int(11)

		/// <summary>
		/// 四位年份数字
		/// </summary>
		[Column("Year",               DataType=AntData.ORM.DataType.Int32,     Comment="四位年份数字"), NotNull]
		public virtual int Year { get; set; } // int(11)

		/// <summary>
		/// 折扣，0.1~1
		/// </summary>
		[Column("Discount",           DataType=AntData.ORM.DataType.Decimal,   Precision=5, Scale=2, Comment="折扣，0.1~1"), NotNull]
		public virtual decimal Discount { get; set; } // decimal(5,2)

		[Column("DateChangeLastTime", DataType=AntData.ORM.DataType.Timestamp), NotNull]
		public virtual DateTime DateChangeLastTime // timestamp
		{
			get { return _DateChangeLastTime; }
			set { _DateChangeLastTime = value; }
		}

		#endregion

		#region Field

		private DateTime _DateChangeLastTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;

		#endregion
	}

	/// <summary>
	/// 交付订单
	/// </summary>
	[Table(Database="lito", Comment="交付订单", Name="delivery_order")]
	public partial class DeliveryOrder : CanalMqBasic
	{
		#region Column

		/// <summary>
		/// 主键
		/// </summary>
		[Column("Tid",                  DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="主键"), PrimaryKey, NotNull]
		public virtual string Tid { get; set; } // varchar(32)

		/// <summary>
		/// 关联的预下单订单的ID
		/// </summary>
		[Column("PreorderId",           DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="关联的预下单订单的ID"), NotNull]
		public virtual string PreorderId { get; set; } // varchar(255)

		/// <summary>
		/// 主题分类ID： 女士，男士，儿童
		/// </summary>
		[Column("RootCategoryTid",      DataType=AntData.ORM.DataType.Int64,    Comment="主题分类ID： 女士，男士，儿童"), NotNull]
		public virtual long RootCategoryTid { get; set; } // bigint(20)

		/// <summary>
		/// 收件人姓名
		/// </summary>
		[Column("ReceiverName",         DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="收件人姓名"),    Nullable]
		public virtual string ReceiverName { get; set; } // varchar(255)

		/// <summary>
		/// 证件号码
		/// </summary>
		[Column("IdNumber",             DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="证件号码"),    Nullable]
		public virtual string IdNumber { get; set; } // varchar(255)

		/// <summary>
		/// 手机号
		/// </summary>
		[Column("Tel",                  DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="手机号"),    Nullable]
		public virtual string Tel { get; set; } // varchar(255)

		/// <summary>
		/// 邮箱
		/// </summary>
		[Column("Email",                DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="邮箱"),    Nullable]
		public virtual string Email { get; set; } // varchar(255)

		/// <summary>
		/// 商品Tid
		/// </summary>
		[Column("GoodTid",              DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="商品Tid"), NotNull]
		public virtual string GoodTid { get; set; } // varchar(32)

		/// <summary>
		/// Sku的Tid
		/// </summary>
		[Column("SkuTid",               DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="Sku的Tid"), NotNull]
		public virtual string SkuTid { get; set; } // varchar(32)

		/// <summary>
		/// Sku在渠道商那的唯一编码
		/// </summary>
		[Column("SkuCode",              DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="Sku在渠道商那的唯一编码"), NotNull]
		public virtual string SkuCode { get; set; } // varchar(255)

		/// <summary>
		/// Sku的全球统一编码
		/// </summary>
		[Column("GlobalCode",           DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="Sku的全球统一编码"), NotNull]
		public virtual string GlobalCode { get; set; } // varchar(255)

		/// <summary>
		/// 颜色
		/// </summary>
		[Column("Color",                DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="颜色"),    Nullable]
		public virtual string Color { get; set; } // varchar(255)

		/// <summary>
		/// 尺码
		/// </summary>
		[Column("Size",                 DataType=AntData.ORM.DataType.VarChar,  Length=64, Comment="尺码"),    Nullable]
		public virtual string Size { get; set; } // varchar(64)

		/// <summary>
		/// 尺码类型
		/// </summary>
		[Column("SizeType",             DataType=AntData.ORM.DataType.VarChar,  Length=64, Comment="尺码类型"),    Nullable]
		public virtual string SizeType { get; set; } // varchar(64)

		/// <summary>
		/// 快递信息JSON
		/// </summary>
		[Column("Express",              DataType=AntData.ORM.DataType.Text,     Length=4294967295, Comment="快递信息JSON"),    Nullable]
		public virtual string Express { get; set; } // longtext

		/// <summary>
		/// 品牌名称
		/// </summary>
		[Column("BrandName",            DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="品牌名称"),    Nullable]
		public virtual string BrandName { get; set; } // varchar(255)

		/// <summary>
		/// 商品名称
		/// </summary>
		[Column("GoodsName",            DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="商品名称"),    Nullable]
		public virtual string GoodsName { get; set; } // varchar(255)

		/// <summary>
		/// 图片地址
		/// </summary>
		[Column("PhotoUrl",             DataType=AntData.ORM.DataType.VarChar,  Length=1024, Comment="图片地址"),    Nullable]
		public virtual string PhotoUrl { get; set; } // varchar(1024)

		/// <summary>
		/// 运输时效
		/// </summary>
		[Column("Timeliness",           DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="运输时效"),    Nullable]
		public virtual string Timeliness { get; set; } // varchar(255)

		/// <summary>
		/// 规则为preorderId-总订单数-当前编号，例如12345-2-1
		/// </summary>
		[Column("OrderId",              DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="规则为preorderId-总订单数-当前编号，例如12345-2-1"), NotNull]
		public virtual string OrderId { get; set; } // varchar(255)

		/// <summary>
		/// 订单状态，未知 = 0, 待付款 = 1, 待确认 = 2, 待收货 = 3, 已完成 = 4, 已关闭 = 5, 退款中 = 6, 已退款 = 7,售后申请中 = 8
		/// </summary>
		[Column("Status",               DataType=AntData.ORM.DataType.Int32,    Comment="订单状态，未知 = 0, 待付款 = 1, 待确认 = 2, 待收货 = 3, 已完成 = 4, 已关闭 = 5, 退款中 = 6, 已退款 = 7,售后申请中 = 8"), NotNull]
		public virtual int Status { get; set; } // int(11)

		/// <summary>
		/// 下单用户Id
		/// </summary>
		[Column("UserId",               DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="下单用户Id"), NotNull]
		public virtual string UserId { get; set; } // varchar(32)

		/// <summary>
		/// 税费，欧元，单位是分
		/// </summary>
		[Column("TaxEurCent",           DataType=AntData.ORM.DataType.Int32,    Comment="税费，欧元，单位是分"), NotNull]
		public virtual int TaxEurCent { get; set; } // int(11)

		/// <summary>
		/// 税费，人民币元，单位是分
		/// </summary>
		[Column("TaxRmbCent",           DataType=AntData.ORM.DataType.Int32,    Comment="税费，人民币元，单位是分"), NotNull]
		public virtual int TaxRmbCent { get; set; } // int(11)

		/// <summary>
		/// 运费，欧元，单位是分
		/// </summary>
		[Column("FreightEurCent",       DataType=AntData.ORM.DataType.Int32,    Comment="运费，欧元，单位是分"), NotNull]
		public virtual int FreightEurCent { get; set; } // int(11)

		/// <summary>
		/// 运费，人民币元，单位是分
		/// </summary>
		[Column("FreightRmbCent",       DataType=AntData.ORM.DataType.Int32,    Comment="运费，人民币元，单位是分"), NotNull]
		public virtual int FreightRmbCent { get; set; } // int(11)

		/// <summary>
		/// 售价，欧元，单位是分
		/// </summary>
		[Column("PriceEurCent",         DataType=AntData.ORM.DataType.Int32,    Comment="售价，欧元，单位是分"), NotNull]
		public virtual int PriceEurCent { get; set; } // int(11)

		/// <summary>
		/// 售价，人民币元，单位是分
		/// </summary>
		[Column("PriceRmbCent",         DataType=AntData.ORM.DataType.Int32,    Comment="售价，人民币元，单位是分"), NotNull]
		public virtual int PriceRmbCent { get; set; } // int(11)

		/// <summary>
		/// 售价含税，欧元，单位是分
		/// </summary>
		[Column("TotalPriceCentEur",    DataType=AntData.ORM.DataType.Int32,    Comment="售价含税，欧元，单位是分"), NotNull]
		public virtual int TotalPriceCentEur { get; set; } // int(11)

		/// <summary>
		/// 售价含税，人民币，单位是分
		/// </summary>
		[Column("TotalPriceCentRMB",    DataType=AntData.ORM.DataType.Int32,    Comment="售价含税，人民币，单位是分"), NotNull]
		public virtual int TotalPriceCentRMB { get; set; } // int(11)

		/// <summary>
		/// 吊牌价，欧元，单位是分
		/// </summary>
		[Column("InitialPriceCentEur",  DataType=AntData.ORM.DataType.Int32,    Comment="吊牌价，欧元，单位是分"), NotNull]
		public virtual int InitialPriceCentEur { get; set; } // int(11)

		/// <summary>
		/// 吊牌价，人民币，单位是分
		/// </summary>
		[Column("InitialPriceCentRmb",  DataType=AntData.ORM.DataType.Int32,    Comment="吊牌价，人民币，单位是分"), NotNull]
		public virtual int InitialPriceCentRmb { get; set; } // int(11)

		/// <summary>
		/// 发货时间
		/// </summary>
		[Column("SendingTimeUTC",       DataType=AntData.ORM.DataType.DateTime, Comment="发货时间"),    Nullable]
		public virtual DateTime? SendingTimeUTC { get; set; } // datetime

		/// <summary>
		/// 商品数量
		/// </summary>
		[Column("Count",                DataType=AntData.ORM.DataType.Int32,    Comment="商品数量"), NotNull]
		public virtual int Count { get; set; } // int(11)

		/// <summary>
		/// 渠道名
		/// </summary>
		[Column("ChannelName",          DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="渠道名"),    Nullable]
		public virtual string ChannelName { get; set; } // varchar(255)

		/// <summary>
		/// 进价
		/// </summary>
		[Column("PurchasePriceEurCent", DataType=AntData.ORM.DataType.Int32,    Comment="进价"), NotNull]
		public virtual int PurchasePriceEurCent { get; set; } // int(11)

		/// <summary>
		/// 下单时间
		/// </summary>
		[Column("CreateTime",           DataType=AntData.ORM.DataType.DateTime, Comment="下单时间"), NotNull]
		public virtual DateTime CreateTime // datetime
		{
			get { return _CreateTime; }
			set { _CreateTime = value; }
		}

		/// <summary>
		/// 额外的字段，用于某些渠道的特殊要求
		/// </summary>
		[Column("AddtionalField",       DataType=AntData.ORM.DataType.VarChar,  Length=1024, Comment="额外的字段，用于某些渠道的特殊要求"),    Nullable]
		public virtual string AddtionalField { get; set; } // varchar(1024)

		/// <summary>
		/// 最后更新时间
		/// </summary>
		[Column("DataChangeLastTime",   DataType=AntData.ORM.DataType.DateTime, Comment="最后更新时间"), NotNull]
		public virtual DateTime DataChangeLastTime // datetime
		{
			get { return _DataChangeLastTime; }
			set { _DataChangeLastTime = value; }
		}

		/// <summary>
		/// 客服的备注
		/// </summary>
		[Column("Comment",              DataType=AntData.ORM.DataType.Text,     Length=4294967295, Comment="客服的备注"),    Nullable]
		public virtual string Comment { get; set; } // longtext

		#endregion

		#region Field

		private DateTime _CreateTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;
		private DateTime _DataChangeLastTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;

		#endregion
	}

	/// <summary>
	/// 商品详情表
	/// </summary>
	[Table(Database="lito", Comment="商品详情表", Name="detail")]
	public partial class Detail : CanalMqBasic
	{
		#region Column

		/// <summary>
		/// 主键
		/// </summary>
		[Column("Tid",                DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="主键"), PrimaryKey, NotNull]
		public virtual string Tid { get; set; } // varchar(32)

		/// <summary>
		/// 原始内容，格式为JSON
		/// </summary>
		[Column("Content",            DataType=AntData.ORM.DataType.Text,     Length=4294967295, Comment="原始内容，格式为JSON"),    Nullable]
		public virtual string Content { get; set; } // longtext

		/// <summary>
		/// 最后更新时间
		/// </summary>
		[Column("DataChangeLastTime", DataType=AntData.ORM.DataType.DateTime, Comment="最后更新时间"),    Nullable]
		public virtual DateTime? DataChangeLastTime { get; set; } // datetime

		#endregion
	}

	/// <summary>
	/// 详情翻译表
	/// </summary>
	[Table(Database="lito", Comment="详情翻译表", Name="detail_translation")]
	public partial class DetailTranslation : CanalMqBasic
	{
		#region Column

		/// <summary>
		/// 主键
		/// </summary>
		[Column("Tid",                DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="主键"), PrimaryKey, NotNull]
		public virtual string Tid { get; set; } // varchar(32)

		/// <summary>
		/// 原文主键
		/// </summary>
		[Column("DetailTid",          DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="原文主键"), NotNull]
		public virtual string DetailTid { get; set; } // varchar(32)

		/// <summary>
		/// 翻译后的JSON
		/// </summary>
		[Column("Translation",        DataType=AntData.ORM.DataType.Text,     Length=4294967295, Comment="翻译后的JSON"),    Nullable]
		public virtual string Translation { get; set; } // longtext

		/// <summary>
		/// 哪种语言
		/// </summary>
		[Column("Language",           DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="哪种语言"), NotNull]
		public virtual string Language { get; set; } // varchar(255)

		/// <summary>
		/// 最后更新时间
		/// </summary>
		[Column("DataChangeLastTime", DataType=AntData.ORM.DataType.DateTime, Comment="最后更新时间"), NotNull]
		public virtual DateTime DataChangeLastTime // datetime
		{
			get { return _DataChangeLastTime; }
			set { _DataChangeLastTime = value; }
		}

		#endregion

		#region Field

		private DateTime _DataChangeLastTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;

		#endregion
	}

	[Table(Database="lito", Name="freight_category_relation")]
	public partial class FreightCategoryRelation : CanalMqBasic
	{
		#region Column

		[Column("Tid",                DataType=AntData.ORM.DataType.VarChar, Length=32), PrimaryKey, NotNull]
		public virtual string Tid { get; set; } // varchar(32)

		[Column("CategoryTid",        DataType=AntData.ORM.DataType.Int64)  , NotNull]
		public virtual long CategoryTid { get; set; } // bigint(20)

		[Column("FreightTemplateTid", DataType=AntData.ORM.DataType.VarChar, Length=32), NotNull]
		public virtual string FreightTemplateTid { get; set; } // varchar(32)

		#endregion
	}

	[Table(Database="lito", Name="freight_region_type_relation")]
	public partial class FreightRegionTypeRelation : CanalMqBasic
	{
		#region Column

		[Column("Tid",           DataType=AntData.ORM.DataType.VarChar, Length=32), PrimaryKey, NotNull]
		public virtual string Tid { get; set; } // varchar(32)

		/// <summary>
		/// 对应region_type表的Tid
		/// </summary>
		[Column("RegionTypeTid", DataType=AntData.ORM.DataType.VarChar, Length=32, Comment="对应region_type表的Tid"), NotNull]
		public virtual string RegionTypeTid { get; set; } // varchar(32)

		/// <summary>
		/// 运费模板名称
		/// </summary>
		[Column("Name",          DataType=AntData.ORM.DataType.VarChar, Length=255, Comment="运费模板名称"), NotNull]
		public virtual string Name { get; set; } // varchar(255)

		/// <summary>
		/// 运费，单位是欧元，分
		/// </summary>
		[Column("FreightCent",   DataType=AntData.ORM.DataType.Int32,   Comment="运费，单位是欧元，分"), NotNull]
		public virtual int FreightCent { get; set; } // int(11)

		#endregion
	}

	/// <summary>
	/// 运费模板
	/// </summary>
	[Table(Database="lito", Comment="运费模板", Name="freight_template")]
	public partial class FreightTemplate : CanalMqBasic
	{
		#region Column

		/// <summary>
		/// 主键
		/// </summary>
		[Column("Tid",                  DataType=AntData.ORM.DataType.VarChar, Length=32, Comment="主键"), PrimaryKey, NotNull]
		public virtual string Tid { get; set; } // varchar(32)

		/// <summary>
		/// 以逗号隔开的freight_region_type_relation Tid
		/// </summary>
		[Column("FreightRegionTidList", DataType=AntData.ORM.DataType.VarChar, Length=1024, Comment="以逗号隔开的freight_region_type_relation Tid"), NotNull]
		public virtual string FreightRegionTidList { get; set; } // varchar(1024)

		/// <summary>
		/// 模板名称
		/// </summary>
		[Column("Name",                 DataType=AntData.ORM.DataType.VarChar, Length=255, Comment="模板名称"), NotNull]
		public virtual string Name { get; set; } // varchar(255)

		#endregion
	}

	/// <summary>
	/// 商品
	/// </summary>
	[Table(Database="lito", Comment="商品", Name="goods")]
	public partial class Good : CanalMqBasic
	{
		#region Column

		/// <summary>
		/// 主键
		/// </summary>
		[Column("Tid",                DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="主键"), PrimaryKey, NotNull]
		public virtual string Tid { get; set; } // varchar(32)

		/// <summary>
		/// 商品编码(非必填，不是sku码)
		/// </summary>
		[Column("Code",               DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="商品编码(非必填，不是sku码)"),    Nullable]
		public virtual string Code { get; set; } // varchar(255)

		/// <summary>
		/// 商品名称
		/// </summary>
		[Column("DisplayName",        DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="商品名称"), NotNull]
		public virtual string DisplayName { get; set; } // varchar(255)

		/// <summary>
		/// 商品页标题
		/// </summary>
		[Column("Title",              DataType=AntData.ORM.DataType.VarChar,  Length=1024, Comment="商品页标题"), NotNull]
		public virtual string Title { get; set; } // varchar(1024)

		/// <summary>
		/// 所属品牌主键
		/// </summary>
		[Column("BrandTid",           DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="所属品牌主键"), NotNull]
		public virtual string BrandTid { get; set; } // varchar(32)

		/// <summary>
		/// 商品标签，以逗号隔开，首位必须以逗号开始和结束
		/// </summary>
		[Column("Tags",               DataType=AntData.ORM.DataType.VarChar,  Length=1024, Comment="商品标签，以逗号隔开，首位必须以逗号开始和结束"),    Nullable]
		public virtual string Tags { get; set; } // varchar(1024)

		/// <summary>
		/// 分类ID集合
		/// </summary>
		[Column("TagIds",             DataType=AntData.ORM.DataType.VarChar,  Length=1024, Comment="分类ID集合"),    Nullable]
		public virtual string TagIds { get; set; } // varchar(1024)

		/// <summary>
		/// 品牌名称
		/// </summary>
		[Column("BrandName",          DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="品牌名称"),    Nullable]
		public virtual string BrandName { get; set; } // varchar(255)

		/// <summary>
		/// 商品详情描述Tid
		/// </summary>
		[Column("DetailTid",          DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="商品详情描述Tid"),    Nullable]
		public virtual string DetailTid { get; set; } // varchar(32)

		/// <summary>
		/// 是否可用
		/// </summary>
		[Column("IsActive",           DataType=AntData.ORM.DataType.Boolean,  Comment="是否可用"), NotNull]
		public virtual bool IsActive { get; set; } // tinyint(1)

		/// <summary>
		/// 是否展示首页
		/// </summary>
		[Column("ShowIndex",          DataType=AntData.ORM.DataType.Boolean,  Comment="是否展示首页"), NotNull]
		public virtual bool ShowIndex { get; set; } // tinyint(1)

		/// <summary>
		/// 创建时间
		/// </summary>
		[Column("CreateTime",         DataType=AntData.ORM.DataType.DateTime, Comment="创建时间"), NotNull]
		public virtual DateTime CreateTime // datetime
		{
			get { return _CreateTime; }
			set { _CreateTime = value; }
		}

		/// <summary>
		/// 需要编辑
		/// </summary>
		[Column("NeedEdit",           DataType=AntData.ORM.DataType.Boolean,  Comment="需要编辑"), NotNull]
		public virtual bool NeedEdit { get; set; } // tinyint(1)

		/// <summary>
		/// 时装季
		/// </summary>
		[Column("CollectionYear",     DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="时装季"),    Nullable]
		public virtual string CollectionYear { get; set; } // varchar(255)

		/// <summary>
		/// 0=未指定，1=女，2=男，3=男女均可
		/// </summary>
		[Column("Gender",             DataType=AntData.ORM.DataType.Int32,    Comment="0=未指定，1=女，2=男，3=男女均可"), NotNull]
		public virtual int Gender { get; set; } // int(11)

		/// <summary>
		/// 0=未指定，1=成人，2=儿童，3=成人儿童均可
		/// </summary>
		[Column("AgeType",            DataType=AntData.ORM.DataType.Int32,    Comment="0=未指定，1=成人，2=儿童，3=成人儿童均可"), NotNull]
		public virtual int AgeType { get; set; } // int(11)

		/// <summary>
		/// 尺码指南的ID
		/// </summary>
		[Column("SizeGuideTid",       DataType=AntData.ORM.DataType.VarChar,  Length=20, Comment="尺码指南的ID"),    Nullable]
		public virtual string SizeGuideTid { get; set; } // varchar(20)

		/// <summary>
		/// 原始的分类类型，会用作后端上货管理的搜索条件，与用户端无关
		/// </summary>
		[Column("OriginalCategory",   DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="原始的分类类型，会用作后端上货管理的搜索条件，与用户端无关"),    Nullable]
		public virtual string OriginalCategory { get; set; } // varchar(255)

		/// <summary>
		/// 时装年份
		/// </summary>
		[Column("Year",               DataType=AntData.ORM.DataType.Int32,    Comment="时装年份"), NotNull]
		public virtual int Year { get; set; } // int(11)

		/// <summary>
		/// 时装季节，3=春夏，12=秋冬
		/// </summary>
		[Column("Season",             DataType=AntData.ORM.DataType.Int32,    Comment="时装季节，3=春夏，12=秋冬"), NotNull]
		public virtual int Season { get; set; } // int(11)

		/// <summary>
		/// 推荐等级
		/// </summary>
		[Column("Stars",              DataType=AntData.ORM.DataType.Int32,    Comment="推荐等级"), NotNull]
		public virtual int Stars { get; set; } // int(11)

		/// <summary>
		/// 最后更新时间
		/// </summary>
		[Column("DataChangeLastTime", DataType=AntData.ORM.DataType.DateTime, Comment="最后更新时间"), NotNull]
		public virtual DateTime DataChangeLastTime // datetime
		{
			get { return _DataChangeLastTime; }
			set { _DataChangeLastTime = value; }
		}

		#endregion

		#region Field

		private DateTime _CreateTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;
		private DateTime _DataChangeLastTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;

		#endregion
	}

	/// <summary>
	/// 商品分类关系
	/// </summary>
	[Table(Database="lito", Comment="商品分类关系", Name="good_category")]
	public partial class GoodCategory : CanalMqBasic
	{
		#region Column

		/// <summary>
		/// 主键
		/// </summary>
		[Column("Tid",                DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="主键"), PrimaryKey, NotNull]
		public virtual string Tid { get; set; } // varchar(32)

		/// <summary>
		/// 商品主键
		/// </summary>
		[Column("GoodTid",            DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="商品主键"), NotNull]
		public virtual string GoodTid { get; set; } // varchar(32)

		/// <summary>
		/// 分类主键
		/// </summary>
		[Column("CategoryTid",        DataType=AntData.ORM.DataType.Int64,    Comment="分类主键"), NotNull]
		public virtual long CategoryTid { get; set; } // bigint(20)

		/// <summary>
		/// 最后更新时间
		/// </summary>
		[Column("DataChangeLastTime", DataType=AntData.ORM.DataType.DateTime, Comment="最后更新时间"), NotNull]
		public virtual DateTime DataChangeLastTime // datetime
		{
			get { return _DataChangeLastTime; }
			set { _DataChangeLastTime = value; }
		}

		#endregion

		#region Field

		private DateTime _DataChangeLastTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;

		#endregion
	}

	/// <summary>
	/// js_report
	/// </summary>
	[Table(Database="lito", Comment="js_report", Name="js_report")]
	public partial class JsReport : CanalMqBasic
	{
		#region Column

		/// <summary>
		/// 主键
		/// </summary>
		[Column("Tid",                DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="主键"), PrimaryKey, NotNull]
		public virtual string Tid { get; set; } // varchar(32)

		/// <summary>
		/// 最后更新时间
		/// </summary>
		[Column("DataChangeLastTime", DataType=AntData.ORM.DataType.DateTime, Comment="最后更新时间"), NotNull]
		public virtual DateTime DataChangeLastTime // datetime
		{
			get { return _DataChangeLastTime; }
			set { _DataChangeLastTime = value; }
		}

		/// <summary>
		/// 上报错误时间
		/// </summary>
		[Column("ClientTime",         DataType=AntData.ORM.DataType.DateTime, Comment="上报错误时间"),    Nullable]
		public virtual DateTime? ClientTime { get; set; } // datetime

		/// <summary>
		/// APPID
		/// </summary>
		[Column("AppId",              DataType=AntData.ORM.DataType.VarChar,  Length=50, Comment="APPID"),    Nullable]
		public virtual string AppId { get; set; } // varchar(50)

		/// <summary>
		/// 暂时不适用
		/// </summary>
		[Column("Uin",                DataType=AntData.ORM.DataType.VarChar,  Length=50, Comment="暂时不适用"),    Nullable]
		public virtual string Uin { get; set; } // varchar(50)

		/// <summary>
		/// 错误信息
		/// </summary>
		[Column("Msg",                DataType=AntData.ORM.DataType.VarChar,  Length=1000, Comment="错误信息"),    Nullable]
		public virtual string Msg { get; set; } // varchar(1000)

		/// <summary>
		/// 错误发生文件
		/// </summary>
		[Column("Target",             DataType=AntData.ORM.DataType.VarChar,  Length=500, Comment="错误发生文件"),    Nullable]
		public virtual string Target { get; set; } // varchar(500)

		/// <summary>
		/// 错误发生行号
		/// </summary>
		[Column("RowNum",             DataType=AntData.ORM.DataType.Int64,    Comment="错误发生行号"),    Nullable]
		public virtual long? RowNum { get; set; } // bigint(20)

		/// <summary>
		/// 错误发生列号
		/// </summary>
		[Column("ColNum",             DataType=AntData.ORM.DataType.Int64,    Comment="错误发生列号"),    Nullable]
		public virtual long? ColNum { get; set; } // bigint(20)

		/// <summary>
		/// 错误来源处
		/// </summary>
		[Column("From",               DataType=AntData.ORM.DataType.VarChar,  Length=500, Comment="错误来源处"),    Nullable]
		public virtual string From { get; set; } // varchar(500)

		/// <summary>
		/// 错误等级
		/// </summary>
		[Column("Level",              DataType=AntData.ORM.DataType.Int32,    Comment="错误等级"),    Nullable]
		public virtual int? Level { get; set; } // int(11)

		/// <summary>
		/// 是否已读
		/// </summary>
		[Column("IsRead",             DataType=AntData.ORM.DataType.Boolean,  Comment="是否已读"), NotNull]
		public virtual bool IsRead { get; set; } // tinyint(1)

		/// <summary>
		/// 处理人
		/// </summary>
		[Column("FixUser",            DataType=AntData.ORM.DataType.VarChar,  Length=50, Comment="处理人"),    Nullable]
		public virtual string FixUser { get; set; } // varchar(50)

		/// <summary>
		/// 处理备注
		/// </summary>
		[Column("Remark",             DataType=AntData.ORM.DataType.VarChar,  Length=500, Comment="处理备注"),    Nullable]
		public virtual string Remark { get; set; } // varchar(500)

		#endregion

		#region Field

		private DateTime _DataChangeLastTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;

		#endregion
	}

	/// <summary>
	/// 城市
	/// </summary>
	[Table(Database="lito", Comment="城市", Name="location_city")]
	public partial class LocationCity : CanalMqBasic
	{
		#region Column

		[Column("Tid",        DataType=AntData.ORM.DataType.VarChar, Length=32), PrimaryKey, NotNull]
		public virtual string Tid { get; set; } // varchar(32)

		[Column("CityName",   DataType=AntData.ORM.DataType.VarChar, Length=50),    Nullable]
		public virtual string CityName { get; set; } // varchar(50)

		[Column("ZipCode",    DataType=AntData.ORM.DataType.VarChar, Length=50),    Nullable]
		public virtual string ZipCode { get; set; } // varchar(50)

		[Column("ProvinceID", DataType=AntData.ORM.DataType.VarChar, Length=32),    Nullable]
		public virtual string ProvinceID { get; set; } // varchar(32)

		#endregion
	}

	/// <summary>
	/// 国家
	/// </summary>
	[Table(Database="lito", Comment="国家", Name="location_country")]
	public partial class LocationCountry : CanalMqBasic
	{
		#region Column

		[Column("Tid",               DataType=AntData.ORM.DataType.VarChar, Length=32), PrimaryKey, NotNull]
		public virtual string Tid { get; set; } // varchar(32)

		/// <summary>
		/// 国家或地区的名称
		/// </summary>
		[Column("Name",              DataType=AntData.ORM.DataType.VarChar, Length=255, Comment="国家或地区的名称"), NotNull]
		public virtual string Name { get; set; } // varchar(255)

		/// <summary>
		/// 电信编码，国际长途需要
		/// </summary>
		[Column("PhoneNumberPrefix", DataType=AntData.ORM.DataType.VarChar, Length=8, Comment="电信编码，国际长途需要"), NotNull]
		public virtual string PhoneNumberPrefix { get; set; } // varchar(8)

		/// <summary>
		/// 洲（亚洲，欧洲等）
		/// </summary>
		[Column("Continent",         DataType=AntData.ORM.DataType.VarChar, Length=24, Comment="洲（亚洲，欧洲等）"), NotNull]
		public virtual string Continent { get; set; } // varchar(24)

		[Column("IsAvailable",       DataType=AntData.ORM.DataType.Boolean), NotNull]
		public virtual bool IsAvailable { get; set; } // tinyint(1)

		[Column("RegionTypeTid",     DataType=AntData.ORM.DataType.VarChar, Length=32),    Nullable]
		public virtual string RegionTypeTid { get; set; } // varchar(32)

		[Column("Order",             DataType=AntData.ORM.DataType.Int32)  , NotNull]
		public virtual int Order { get; set; } // int(11)

		#endregion
	}

	/// <summary>
	/// 省
	/// </summary>
	[Table(Database="lito", Comment="省", Name="location_province")]
	public partial class LocationProvince : CanalMqBasic
	{
		#region Column

		/// <summary>
		/// 省的ID
		/// </summary>
		[Column("Tid",          DataType=AntData.ORM.DataType.VarChar, Length=32, Comment="省的ID"), PrimaryKey, NotNull]
		public virtual string Tid { get; set; } // varchar(32)

		/// <summary>
		/// 省的名称
		/// </summary>
		[Column("ProvinceName", DataType=AntData.ORM.DataType.VarChar, Length=50, Comment="省的名称"), NotNull]
		public virtual string ProvinceName { get; set; } // varchar(50)

		/// <summary>
		/// 国家或地区ID
		/// </summary>
		[Column("CountryId",    DataType=AntData.ORM.DataType.VarChar, Length=32, Comment="国家或地区ID"), NotNull]
		public virtual string CountryId { get; set; } // varchar(32)

		/// <summary>
		/// 是否可配送
		/// </summary>
		[Column("IsAvailable",  DataType=AntData.ORM.DataType.Boolean, Comment="是否可配送"), NotNull]
		public virtual bool IsAvailable { get; set; } // tinyint(1)

		#endregion
	}

	/// <summary>
	/// Nlog
	/// </summary>
	[Table(Database="lito", Comment="Nlog", Name="nlog")]
	public partial class Nlog : CanalMqBasic
	{
		#region Column

		/// <summary>
		/// 主键
		/// </summary>
		[Column("Tid",                DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="主键"), PrimaryKey, NotNull]
		public virtual string Tid { get; set; } // varchar(32)

		/// <summary>
		/// 最后更新时间
		/// </summary>
		[Column("DataChangeLastTime", DataType=AntData.ORM.DataType.DateTime, Comment="最后更新时间"),    Nullable]
		public virtual DateTime? DataChangeLastTime { get; set; } // datetime

		/// <summary>
		/// 应用名称
		/// </summary>
		[Column("Application",        DataType=AntData.ORM.DataType.VarChar,  Length=50, Comment="应用名称"),    Nullable]
		public virtual string Application { get; set; } // varchar(50)

		/// <summary>
		/// LOG等级
		/// </summary>
		[Column("Level",              DataType=AntData.ORM.DataType.VarChar,  Length=20, Comment="LOG等级"),    Nullable]
		public virtual string Level { get; set; } // varchar(20)

		/// <summary>
		/// 所在的class名称
		/// </summary>
		[Column("Logger",             DataType=AntData.ORM.DataType.VarChar,  Length=250, Comment="所在的class名称"),    Nullable]
		public virtual string Logger { get; set; } // varchar(250)

		/// <summary>
		/// 内容
		/// </summary>
		[Column("Message",            DataType=AntData.ORM.DataType.VarChar,  Length=2048, Comment="内容"),    Nullable]
		public virtual string Message { get; set; } // varchar(2048)

		/// <summary>
		/// 错误内容
		/// </summary>
		[Column("Exception",          DataType=AntData.ORM.DataType.VarChar,  Length=2048, Comment="错误内容"),    Nullable]
		public virtual string Exception { get; set; } // varchar(2048)

		/// <summary>
		/// 请求地址
		/// </summary>
		[Column("Callsite",           DataType=AntData.ORM.DataType.VarChar,  Length=512, Comment="请求地址"),    Nullable]
		public virtual string Callsite { get; set; } // varchar(512)

		#endregion
	}

	[Table(Database="lito", Name="oss_records_hangzhou")]
	public partial class OssRecordsHangzhou : CanalMqBasic
	{
		#region Column

		/// <summary>
		/// 文件名
		/// </summary>
		[Column("Key",                     DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="文件名"), PrimaryKey, NotNull]
		public virtual string Key { get; set; } // varchar(255)

		/// <summary>
		/// 法兰克福可用区是否存在
		/// </summary>
		[Column("IsExistFr",               DataType=AntData.ORM.DataType.Boolean,  Comment="法兰克福可用区是否存在"), NotNull]
		public virtual bool IsExistFr { get; set; } // tinyint(1)

		/// <summary>
		/// 杭州可用区是否存在
		/// </summary>
		[Column("IsExist",                 DataType=AntData.ORM.DataType.Boolean,  Comment="杭州可用区是否存在"), NotNull]
		public virtual bool IsExist { get; set; } // tinyint(1)

		/// <summary>
		/// 源下载地址，可以为空
		/// </summary>
		[Column("SourceURL",               DataType=AntData.ORM.DataType.VarChar,  Length=2048, Comment="源下载地址，可以为空"),    Nullable]
		public virtual string SourceURL { get; set; } // varchar(2048)

		/// <summary>
		/// 创建时间
		/// </summary>
		[Column("CreateTime",              DataType=AntData.ORM.DataType.DateTime, Comment="创建时间"), NotNull]
		public virtual DateTime CreateTime // datetime
		{
			get { return _CreateTime; }
			set { _CreateTime = value; }
		}

		/// <summary>
		/// 二次检查的时间-杭州OSS
		/// </summary>
		[Column("DoubleCheckTimeHangzhou", DataType=AntData.ORM.DataType.DateTime, Comment="二次检查的时间-杭州OSS"), NotNull]
		public virtual DateTime DoubleCheckTimeHangzhou // datetime
		{
			get { return _DoubleCheckTimeHangzhou; }
			set { _DoubleCheckTimeHangzhou = value; }
		}

		/// <summary>
		/// 二次检查时间-法兰克福
		/// </summary>
		[Column("DoubleCheckTimeFr",       DataType=AntData.ORM.DataType.DateTime, Comment="二次检查时间-法兰克福"), NotNull]
		public virtual DateTime DoubleCheckTimeFr // datetime
		{
			get { return _DoubleCheckTimeFr; }
			set { _DoubleCheckTimeFr = value; }
		}

		#endregion

		#region Field

		private DateTime _CreateTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;
		private DateTime _DoubleCheckTimeHangzhou = System.Data.SqlTypes.SqlDateTime.MinValue.Value;
		private DateTime _DoubleCheckTimeFr = System.Data.SqlTypes.SqlDateTime.MinValue.Value;

		#endregion
	}

	/// <summary>
	/// 订单表，Pre-Order的意思是预下单，当订单支付后，会产生若干个DeliveryOrder（交付订单），交付订单才是真正和快递信息绑定的订单。
	/// </summary>
	[Table(Database="lito", Comment="订单表，Pre-Order的意思是预下单，当订单支付后，会产生若干个DeliveryOrder（交付订单），交付订单才是真正和快递信息绑定的订单。", Name="preorder")]
	public partial class Preorder : CanalMqBasic
	{
		#region Column

		[Column("Tid",                DataType=AntData.ORM.DataType.VarChar,  Length=32), PrimaryKey, NotNull]
		public virtual string Tid { get; set; } // varchar(32)

		/// <summary>
		/// 订单号
		/// </summary>
		[Column("OrderId",            DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="订单号"), NotNull]
		public virtual string OrderId { get; set; } // varchar(255)

		/// <summary>
		/// 用户的ID
		/// </summary>
		[Column("UserTid",            DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="用户的ID"), NotNull]
		public virtual string UserTid { get; set; } // varchar(32)

		/// <summary>
		/// 创建时间UTC
		/// </summary>
		[Column("CreateTimeUTC",      DataType=AntData.ORM.DataType.DateTime, Comment="创建时间UTC"), NotNull]
		public virtual DateTime CreateTimeUTC // datetime
		{
			get { return _CreateTimeUTC; }
			set { _CreateTimeUTC = value; }
		}

		/// <summary>
		/// 地址信息的JSON
		/// </summary>
		[Column("AddressInfoJSON",    DataType=AntData.ORM.DataType.Text,     Length=4294967295, Comment="地址信息的JSON"),    Nullable]
		public virtual string AddressInfoJSON { get; set; } // longtext

		/// <summary>
		/// 订单信息
		/// </summary>
		[Column("OrderDetailJSON",    DataType=AntData.ORM.DataType.Text,     Length=4294967295, Comment="订单信息"), NotNull]
		public virtual string OrderDetailJSON { get; set; } // longtext

		/// <summary>
		/// 数据签名
		/// </summary>
		[Column("Sign",               DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="数据签名"), NotNull]
		public virtual string Sign { get; set; } // varchar(255)

		/// <summary>
		/// 订单总金额，欧元单位是分
		/// </summary>
		[Column("TotalPriceEurCent",  DataType=AntData.ORM.DataType.Int32,    Comment="订单总金额，欧元单位是分"), NotNull]
		public virtual int TotalPriceEurCent { get; set; } // int(11)

		/// <summary>
		/// 订单总金额，人民币单位是分
		/// </summary>
		[Column("TotalPriceRMBCent",  DataType=AntData.ORM.DataType.Int32,    Comment="订单总金额，人民币单位是分"), NotNull]
		public virtual int TotalPriceRMBCent { get; set; } // int(11)

		/// <summary>
		/// 是否隐藏此订单，当用户支付后，会创建真实订单，此时的Preorder会被隐藏
		/// </summary>
		[Column("IsHide",             DataType=AntData.ORM.DataType.Boolean,  Comment="是否隐藏此订单，当用户支付后，会创建真实订单，此时的Preorder会被隐藏"), NotNull]
		public virtual bool IsHide { get; set; } // tinyint(1)

		/// <summary>
		/// 支付时间
		/// </summary>
		[Column("PayTimeUTC",         DataType=AntData.ORM.DataType.DateTime, Comment="支付时间"),    Nullable]
		public virtual DateTime? PayTimeUTC { get; set; } // datetime

		/// <summary>
		/// 支付金额，人民币，单位是分
		/// </summary>
		[Column("PayCentRMB",         DataType=AntData.ORM.DataType.Int32,    Comment="支付金额，人民币，单位是分"),    Nullable]
		public virtual int? PayCentRMB { get; set; } // int(11)

		/// <summary>
		/// 支付方式
		/// </summary>
		[Column("PayWay",             DataType=AntData.ORM.DataType.VarChar,  Length=64, Comment="支付方式"),    Nullable]
		public virtual string PayWay { get; set; } // varchar(64)

		/// <summary>
		/// 付款卡类型
		/// </summary>
		[Column("PayCardType",        DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="付款卡类型"),    Nullable]
		public virtual string PayCardType { get; set; } // varchar(255)

		/// <summary>
		/// 支付的时候减免的部分
		/// </summary>
		[Column("DiscountPayCentRMB", DataType=AntData.ORM.DataType.Int32,    Comment="支付的时候减免的部分"), NotNull]
		public virtual int DiscountPayCentRMB { get; set; } // int(11)

		/// <summary>
		/// 订单状态，未知 = 0, 待付款 = 1, 待确认 = 2, 待收货 = 3, 已完成 = 4, 已关闭 = 5, 退款中 = 6, 已退款 = 7,售后申请中 = 8
		/// </summary>
		[Column("Status",             DataType=AntData.ORM.DataType.Int32,    Comment="订单状态，未知 = 0, 待付款 = 1, 待确认 = 2, 待收货 = 3, 已完成 = 4, 已关闭 = 5, 退款中 = 6, 已退款 = 7,售后申请中 = 8"), NotNull]
		public virtual int Status { get; set; } // int(11)

		/// <summary>
		/// 银行流水号
		/// </summary>
		[Column("BankTradeNo",        DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="银行流水号"),    Nullable]
		public virtual string BankTradeNo { get; set; } // varchar(255)

		/// <summary>
		/// 原始回调信息
		/// </summary>
		[Column("RawData",            DataType=AntData.ORM.DataType.Text,     Length=4294967295, Comment="原始回调信息"),    Nullable]
		public virtual string RawData { get; set; } // longtext

		/// <summary>
		/// 税费，欧元，单位是分
		/// </summary>
		[Column("TaxEurCent",         DataType=AntData.ORM.DataType.Int32,    Comment="税费，欧元，单位是分"), NotNull]
		public virtual int TaxEurCent { get; set; } // int(11)

		/// <summary>
		/// 税费，人民币，单位是分
		/// </summary>
		[Column("TaxRmbCent",         DataType=AntData.ORM.DataType.Int32,    Comment="税费，人民币，单位是分"), NotNull]
		public virtual int TaxRmbCent { get; set; } // int(11)

		/// <summary>
		/// 运费，欧元，单位是分
		/// </summary>
		[Column("FreightEurCent",     DataType=AntData.ORM.DataType.Int32,    Comment="运费，欧元，单位是分"), NotNull]
		public virtual int FreightEurCent { get; set; } // int(11)

		/// <summary>
		/// 运费，人民币，单位是分
		/// </summary>
		[Column("FreightRmbCent",     DataType=AntData.ORM.DataType.Int32,    Comment="运费，人民币，单位是分"), NotNull]
		public virtual int FreightRmbCent { get; set; } // int(11)

		/// <summary>
		/// 商品总价，不含税，欧元，单位是分
		/// </summary>
		[Column("GoodPriceEurCent",   DataType=AntData.ORM.DataType.Int32,    Comment="商品总价，不含税，欧元，单位是分"), NotNull]
		public virtual int GoodPriceEurCent { get; set; } // int(11)

		/// <summary>
		/// 商品总价，不含税，人民币，单位是分
		/// </summary>
		[Column("GoodPriceRmbCent",   DataType=AntData.ORM.DataType.Int32,    Comment="商品总价，不含税，人民币，单位是分"), NotNull]
		public virtual int GoodPriceRmbCent { get; set; } // int(11)

		/// <summary>
		/// 订单包含的购物车列表Tid，用逗号隔开
		/// </summary>
		[Column("CartIds",            DataType=AntData.ORM.DataType.VarChar,  Length=1024, Comment="订单包含的购物车列表Tid，用逗号隔开"), NotNull]
		public virtual string CartIds { get; set; } // varchar(1024)

		/// <summary>
		/// 用户留言
		/// </summary>
		[Column("Comment",            DataType=AntData.ORM.DataType.Text,     Length=4294967295, Comment="用户留言"),    Nullable]
		public virtual string Comment { get; set; } // longtext

		/// <summary>
		/// 客服的备注，不会给客人展示
		/// </summary>
		[Column("InternalComment",    DataType=AntData.ORM.DataType.Text,     Length=4294967295, Comment="客服的备注，不会给客人展示"),    Nullable]
		public virtual string InternalComment { get; set; } // longtext

		[Column("DataChangeLastTime", DataType=AntData.ORM.DataType.DateTime), NotNull]
		public virtual DateTime DataChangeLastTime // datetime
		{
			get { return _DataChangeLastTime; }
			set { _DataChangeLastTime = value; }
		}

		#endregion

		#region Field

		private DateTime _CreateTimeUTC = System.Data.SqlTypes.SqlDateTime.MinValue.Value;
		private DateTime _DataChangeLastTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;

		#endregion
	}

	[Table(Database="lito", Name="region_type")]
	public partial class RegionType : CanalMqBasic
	{
		#region Column

		[Column("Tid",  DataType=AntData.ORM.DataType.VarChar, Length=32), PrimaryKey, NotNull]
		public virtual string Tid { get; set; } // varchar(32)

		/// <summary>
		/// 区域的名称，例如“中国大陆”
		/// </summary>
		[Column("Name", DataType=AntData.ORM.DataType.VarChar, Length=255, Comment="区域的名称，例如“中国大陆”"), NotNull]
		public virtual string Name { get; set; } // varchar(255)

		#endregion
	}

	[Table(Database="lito", Name="sale_price")]
	public partial class SalePrice : CanalMqBasic
	{
		#region Column

		/// <summary>
		/// 主键
		/// </summary>
		[Column("Tid",                DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="主键"), PrimaryKey, NotNull]
		public virtual string Tid { get; set; } // varchar(32)

		/// <summary>
		/// Sku编码
		/// </summary>
		[Column("SkuTid",             DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="Sku编码"), NotNull]
		public virtual string SkuTid { get; set; } // varchar(32)

		/// <summary>
		/// 用户等级，0~7
		/// </summary>
		[Column("UserLevel",          DataType=AntData.ORM.DataType.Int32,    Comment="用户等级，0~7"), NotNull]
		public virtual int UserLevel { get; set; } // int(11)

		/// <summary>
		/// 售价，欧元，单位是分
		/// </summary>
		[Column("SalePriceEurCent",   DataType=AntData.ORM.DataType.Int32,    Comment="售价，欧元，单位是分"), NotNull]
		public virtual int SalePriceEurCent { get; set; } // int(11)

		/// <summary>
		/// 创建人
		/// </summary>
		[Column("CreateBy",           DataType=AntData.ORM.DataType.VarChar,  Length=20, Comment="创建人"), NotNull]
		public virtual string CreateBy { get; set; } // varchar(20)

		/// <summary>
		/// 最后更新时间
		/// </summary>
		[Column("DataChangeLastTime", DataType=AntData.ORM.DataType.DateTime, Comment="最后更新时间"), NotNull]
		public virtual DateTime DataChangeLastTime // datetime
		{
			get { return _DataChangeLastTime; }
			set { _DataChangeLastTime = value; }
		}

		/// <summary>
		/// 最后更新人
		/// </summary>
		[Column("UpdateBy",           DataType=AntData.ORM.DataType.VarChar,  Length=20, Comment="最后更新人"),    Nullable]
		public virtual string UpdateBy { get; set; } // varchar(20)

		#endregion

		#region Field

		private DateTime _DataChangeLastTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;

		#endregion
	}

	[Table(Database="lito", Name="size_guide")]
	public partial class SizeGuide : CanalMqBasic
	{
		#region Column

		[Column("Tid",                DataType=AntData.ORM.DataType.VarChar,   Length=32), PrimaryKey, NotNull]
		public virtual string Tid { get; set; } // varchar(32)

		/// <summary>
		/// 尺码指南的JSON
		/// </summary>
		[Column("Json",               DataType=AntData.ORM.DataType.Text,      Length=4294967295, Comment="尺码指南的JSON"),    Nullable]
		public virtual string Json { get; set; } // longtext

		[Column("DataChangeLastTime", DataType=AntData.ORM.DataType.Timestamp), NotNull]
		public virtual DateTime DataChangeLastTime // timestamp
		{
			get { return _DataChangeLastTime; }
			set { _DataChangeLastTime = value; }
		}

		#endregion

		#region Field

		private DateTime _DataChangeLastTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;

		#endregion
	}

	/// <summary>
	/// 款式
	/// </summary>
	[Table(Database="lito", Comment="款式", Name="sku")]
	public partial class Sku : CanalMqBasic
	{
		#region Column

		/// <summary>
		/// 主键
		/// </summary>
		[Column("Tid",                DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="主键"), PrimaryKey, NotNull]
		public virtual string Tid { get; set; } // varchar(32)

		/// <summary>
		/// 商品主键
		/// </summary>
		[Column("GoodsTid",           DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="商品主键"), NotNull]
		public virtual string GoodsTid { get; set; } // varchar(32)

		/// <summary>
		/// 尺码，纯数字
		/// </summary>
		[Column("Size",               DataType=AntData.ORM.DataType.VarChar,  Length=8, Comment="尺码，纯数字"),    Nullable]
		public virtual string Size { get; set; } // varchar(8)

		/// <summary>
		/// 尺码类型编号（欧码，标码等）
		/// </summary>
		[Column("SizeType",           DataType=AntData.ORM.DataType.VarChar,  Length=16, Comment="尺码类型编号（欧码，标码等）"),    Nullable]
		public virtual string SizeType { get; set; } // varchar(16)

		/// <summary>
		/// 颜色
		/// </summary>
		[Column("Color",              DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="颜色"),    Nullable]
		public virtual string Color { get; set; } // varchar(255)

		/// <summary>
		/// 渠道商的SkuID，在同一个渠道商内部是唯一的
		/// </summary>
		[Column("Code",               DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="渠道商的SkuID，在同一个渠道商内部是唯一的"), NotNull]
		public virtual string Code { get; set; } // varchar(255)

		/// <summary>
		/// 库存数量
		/// </summary>
		[Column("Stock",              DataType=AntData.ORM.DataType.Int32,    Comment="库存数量"), NotNull]
		public virtual int Stock { get; set; } // int(11)

		/// <summary>
		/// 原始折扣
		/// </summary>
		[Column("DiscountOriginal",   DataType=AntData.ORM.DataType.Decimal,  Precision=3, Scale=2, Comment="原始折扣"), NotNull]
		public virtual decimal DiscountOriginal { get; set; } // decimal(3,2)

		/// <summary>
		/// 折扣
		/// </summary>
		[Column("Discount",           DataType=AntData.ORM.DataType.Decimal,  Precision=3, Scale=2, Comment="折扣"), NotNull]
		public virtual decimal Discount { get; set; } // decimal(3,2)

		/// <summary>
		/// 原始售价，自动上货的售价
		/// </summary>
		[Column("PriceEurOriginal",   DataType=AntData.ORM.DataType.Decimal,  Precision=10, Scale=3, Comment="原始售价，自动上货的售价"), NotNull]
		public virtual decimal PriceEurOriginal { get; set; } // decimal(10,3)

		/// <summary>
		/// 价格，单位是欧元，可以被人工修改
		/// </summary>
		[Column("PriceEUR",           DataType=AntData.ORM.DataType.Decimal,  Precision=10, Scale=3, Comment="价格，单位是欧元，可以被人工修改"), NotNull]
		public virtual decimal PriceEUR { get; set; } // decimal(10,3)

		/// <summary>
		/// 进价
		/// </summary>
		[Column("PurchasePrice",      DataType=AntData.ORM.DataType.Decimal,  Precision=10, Scale=3, Comment="进价"), NotNull]
		public virtual decimal PurchasePrice { get; set; } // decimal(10,3)

		/// <summary>
		/// 原价
		/// </summary>
		[Column("InitialPrice",       DataType=AntData.ORM.DataType.Decimal,  Precision=10, Scale=3, Comment="原价"), NotNull]
		public virtual decimal InitialPrice { get; set; } // decimal(10,3)

		/// <summary>
		/// 材质
		/// </summary>
		[Column("Material",           DataType=AntData.ORM.DataType.VarChar,  Length=1024, Comment="材质"),    Nullable]
		public virtual string Material { get; set; } // varchar(1024)

		/// <summary>
		/// 渠道名称
		/// </summary>
		[Column("ChannelName",        DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="渠道名称"),    Nullable]
		public virtual string ChannelName { get; set; } // varchar(255)

		/// <summary>
		/// 最后更新时间
		/// </summary>
		[Column("DataChangeLastTime", DataType=AntData.ORM.DataType.DateTime, Comment="最后更新时间"), NotNull]
		public virtual DateTime DataChangeLastTime // datetime
		{
			get { return _DataChangeLastTime; }
			set { _DataChangeLastTime = value; }
		}

		/// <summary>
		/// 统一码
		/// </summary>
		[Column("GlobalCode",         DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="统一码"),    Nullable]
		public virtual string GlobalCode { get; set; } // varchar(255)

		/// <summary>
		/// 图片JSON
		/// </summary>
		[Column("Photos",             DataType=AntData.ORM.DataType.Text,     Length=4294967295, Comment="图片JSON"),    Nullable]
		public virtual string Photos { get; set; } // longtext

		/// <summary>
		/// 是否可用
		/// </summary>
		[Column("IsActive",           DataType=AntData.ORM.DataType.Boolean,  Comment="是否可用"), NotNull]
		public virtual bool IsActive { get; set; } // tinyint(1)

		/// <summary>
		/// 排序
		/// </summary>
		[Column("OrderRule",          DataType=AntData.ORM.DataType.Int32,    Comment="排序"), NotNull]
		public virtual int OrderRule { get; set; } // int(11)

		/// <summary>
		/// 商品简介
		/// </summary>
		[Column("Description",        DataType=AntData.ORM.DataType.Text,     Length=4294967295, Comment="商品简介"),    Nullable]
		public virtual string Description { get; set; } // longtext

		/// <summary>
		/// 图片是否全部在阿里云OSS上
		/// </summary>
		[Column("IsPicChecked",       DataType=AntData.ORM.DataType.Boolean,  Comment="图片是否全部在阿里云OSS上"), NotNull]
		public virtual bool IsPicChecked { get; set; } // tinyint(1)

		/// <summary>
		/// 运费模板Tid
		/// </summary>
		[Column("FreightTemplateTid", DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="运费模板Tid"),    Nullable]
		public virtual string FreightTemplateTid { get; set; } // varchar(32)

		/// <summary>
		/// 销量
		/// </summary>
		[Column("Sales",              DataType=AntData.ORM.DataType.Int32,    Comment="销量"), NotNull]
		public virtual int Sales { get; set; } // int(11)

		/// <summary>
		/// 下架理由
		/// </summary>
		[Column("ReasonForDisable",   DataType=AntData.ORM.DataType.VarChar,  Length=1024, Comment="下架理由"),    Nullable]
		public virtual string ReasonForDisable { get; set; } // varchar(1024)

		/// <summary>
		/// 供应商特有的额外信息
		/// </summary>
		[Column("AddtionalField",     DataType=AntData.ORM.DataType.VarChar,  Length=2048, Comment="供应商特有的额外信息"),    Nullable]
		public virtual string AddtionalField { get; set; } // varchar(2048)

		#endregion

		#region Field

		private DateTime _DataChangeLastTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;

		#endregion
	}

	/// <summary>
	/// 系统菜单表
	/// </summary>
	[Table(Database="lito", Comment="系统菜单表", Name="system_menu")]
	public partial class SystemMenu : CanalMqBasic
	{
		#region Column

		/// <summary>
		/// MenuId
		/// </summary>
		[Column("Tid",                DataType=AntData.ORM.DataType.Int64,    Comment="MenuId"), PrimaryKey, Identity]
		public virtual long Tid { get; set; } // bigint(20)

		/// <summary>
		/// 最后更新时间
		/// </summary>
		[Column("DataChangeLastTime", DataType=AntData.ORM.DataType.DateTime, Comment="最后更新时间"), NotNull]
		public virtual DateTime DataChangeLastTime // datetime
		{
			get { return _DataChangeLastTime; }
			set { _DataChangeLastTime = value; }
		}

		/// <summary>
		/// 是否可用
		/// </summary>
		[Column("IsActive",           DataType=AntData.ORM.DataType.Boolean,  Comment="是否可用"), NotNull]
		public virtual bool IsActive { get; set; } // tinyint(1)

		/// <summary>
		/// 父节点Id
		/// </summary>
		[Column("ParentTid",          DataType=AntData.ORM.DataType.Int64,    Comment="父节点Id"), NotNull]
		public virtual long ParentTid { get; set; } // bigint(20)

		/// <summary>
		/// 名称
		/// </summary>
		[Column("Name",               DataType=AntData.ORM.DataType.VarChar,  Length=50, Comment="名称"),    Nullable]
		public virtual string Name { get; set; } // varchar(50)

		/// <summary>
		/// 展示的图标
		/// </summary>
		[Column("Ico",                DataType=AntData.ORM.DataType.VarChar,  Length=100, Comment="展示的图标"),    Nullable]
		public virtual string Ico { get; set; } // varchar(100)

		/// <summary>
		/// 连接地址
		/// </summary>
		[Column("Url",                DataType=AntData.ORM.DataType.VarChar,  Length=200, Comment="连接地址"),    Nullable]
		public virtual string Url { get; set; } // varchar(200)

		/// <summary>
		/// 排序
		/// </summary>
		[Column("OrderRule",          DataType=AntData.ORM.DataType.Int32,    Comment="排序"),    Nullable]
		public virtual int? OrderRule { get; set; } // int(11)

		/// <summary>
		/// 等级
		/// </summary>
		[Column("Level",              DataType=AntData.ORM.DataType.Int32,    Comment="等级"),    Nullable]
		public virtual int? Level { get; set; } // int(11)

		/// <summary>
		/// 样式
		/// </summary>
		[Column("Class",              DataType=AntData.ORM.DataType.VarChar,  Length=100, Comment="样式"),    Nullable]
		public virtual string Class { get; set; } // varchar(100)

		#endregion

		#region Field

		private DateTime _DataChangeLastTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;

		#endregion
	}

	/// <summary>
	/// 菜单按钮
	/// </summary>
	[Table(Database="lito", Comment="菜单按钮", Name="system_page_action")]
	public partial class SystemPageAction : CanalMqBasic
	{
		#region Column

		/// <summary>
		/// 主键
		/// </summary>
		[Column("Tid",                DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="主键"), PrimaryKey, NotNull]
		public virtual string Tid { get; set; } // varchar(32)

		/// <summary>
		/// 最后更新时间
		/// </summary>
		[Column("DataChangeLastTime", DataType=AntData.ORM.DataType.DateTime, Comment="最后更新时间"), NotNull]
		public virtual DateTime DataChangeLastTime // datetime
		{
			get { return _DataChangeLastTime; }
			set { _DataChangeLastTime = value; }
		}

		/// <summary>
		/// 访问路径
		/// </summary>
		[Column("MenuTid",            DataType=AntData.ORM.DataType.Int64,    Comment="访问路径"), NotNull]
		public virtual long MenuTid { get; set; } // bigint(20)

		/// <summary>
		/// ActionId
		/// </summary>
		[Column("ActionId",           DataType=AntData.ORM.DataType.VarChar,  Length=100, Comment="ActionId"),    Nullable]
		public virtual string ActionId { get; set; } // varchar(100)

		/// <summary>
		/// ActionName
		/// </summary>
		[Column("ActionName",         DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="ActionName"),    Nullable]
		public virtual string ActionName { get; set; } // varchar(255)

		/// <summary>
		/// ControlName
		/// </summary>
		[Column("ControlName",        DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="ControlName"),    Nullable]
		public virtual string ControlName { get; set; } // varchar(255)

		#endregion

		#region Field

		private DateTime _DataChangeLastTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;

		#endregion
	}

	/// <summary>
	/// 角色表
	/// </summary>
	[Table(Database="lito", Comment="角色表", Name="system_role")]
	public partial class SystemRole : CanalMqBasic
	{
		#region Column

		/// <summary>
		/// 最后更新时间
		/// </summary>
		[Column("DataChangeLastTime", DataType=AntData.ORM.DataType.DateTime, Comment="最后更新时间"), NotNull]
		public virtual DateTime DataChangeLastTime // datetime
		{
			get { return _DataChangeLastTime; }
			set { _DataChangeLastTime = value; }
		}

		/// <summary>
		/// 角色名称
		/// </summary>
		[Column("RoleName",           DataType=AntData.ORM.DataType.VarChar,  Length=100, Comment="角色名称"),    Nullable]
		public virtual string RoleName { get; set; } // varchar(100)

		/// <summary>
		/// 描述
		/// </summary>
		[Column("Description",        DataType=AntData.ORM.DataType.VarChar,  Length=200, Comment="描述"),    Nullable]
		public virtual string Description { get; set; } // varchar(200)

		/// <summary>
		/// 是否可用
		/// </summary>
		[Column("IsActive",           DataType=AntData.ORM.DataType.Boolean,  Comment="是否可用"), NotNull]
		public virtual bool IsActive { get; set; } // tinyint(1)

		/// <summary>
		/// 菜单权限
		/// </summary>
		[Column("MenuRights",         DataType=AntData.ORM.DataType.VarChar,  Length=150, Comment="菜单权限"),    Nullable]
		public virtual string MenuRights { get; set; } // varchar(150)

		/// <summary>
		/// 主键
		/// </summary>
		[Column("Tid",                DataType=AntData.ORM.DataType.Int64,    Comment="主键"), PrimaryKey, Identity]
		public virtual long Tid { get; set; } // bigint(20)

		/// <summary>
		/// 按钮等权限
		/// </summary>
		[Column("ActionList",         DataType=AntData.ORM.DataType.Text,     Length=4294967295, Comment="按钮等权限"),    Nullable]
		public virtual string ActionList { get; set; } // longtext

		/// <summary>
		/// 创建者
		/// </summary>
		[Column("CreateUser",         DataType=AntData.ORM.DataType.VarChar,  Length=20, Comment="创建者"),    Nullable]
		public virtual string CreateUser { get; set; } // varchar(20)

		/// <summary>
		/// 创建者的角色Tid
		/// </summary>
		[Column("CreateRoleTid",      DataType=AntData.ORM.DataType.Int64,    Comment="创建者的角色Tid"), NotNull]
		public virtual long CreateRoleTid { get; set; } // bigint(20)

		#endregion

		#region Field

		private DateTime _DataChangeLastTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;

		#endregion
	}

	/// <summary>
	/// 后台系统用户表
	/// </summary>
	[Table(Database="lito", Comment="后台系统用户表", Name="system_users")]
	public partial class SystemUsers : CanalMqBasic
	{
		#region Column

		/// <summary>
		/// 主键
		/// </summary>
		[Column("Tid",                DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="主键"), PrimaryKey, NotNull]
		public virtual string Tid { get; set; } // varchar(32)

		/// <summary>
		/// 最后更新时间
		/// </summary>
		[Column("DataChangeLastTime", DataType=AntData.ORM.DataType.DateTime, Comment="最后更新时间"), NotNull]
		public virtual DateTime DataChangeLastTime // datetime
		{
			get { return _DataChangeLastTime; }
			set { _DataChangeLastTime = value; }
		}

		/// <summary>
		/// 是否可用
		/// </summary>
		[Column("IsActive",           DataType=AntData.ORM.DataType.Boolean,  Comment="是否可用"), NotNull]
		public virtual bool IsActive { get; set; } // tinyint(1)

		/// <summary>
		/// 登陆名
		/// </summary>
		[Column("Eid",                DataType=AntData.ORM.DataType.VarChar,  Length=36, Comment="登陆名"),    Nullable]
		public virtual string Eid { get; set; } // varchar(36)

		/// <summary>
		/// 用户名
		/// </summary>
		[Column("UserName",           DataType=AntData.ORM.DataType.VarChar,  Length=50, Comment="用户名"),    Nullable]
		public virtual string UserName { get; set; } // varchar(50)

		/// <summary>
		/// 密码
		/// </summary>
		[Column("Pwd",                DataType=AntData.ORM.DataType.VarChar,  Length=50, Comment="密码"),    Nullable]
		public virtual string Pwd { get; set; } // varchar(50)

		/// <summary>
		/// 手机号
		/// </summary>
		[Column("Phone",              DataType=AntData.ORM.DataType.VarChar,  Length=20, Comment="手机号"),    Nullable]
		public virtual string Phone { get; set; } // varchar(20)

		/// <summary>
		/// 登陆IP
		/// </summary>
		[Column("LoginIp",            DataType=AntData.ORM.DataType.VarChar,  Length=30, Comment="登陆IP"),    Nullable]
		public virtual string LoginIp { get; set; } // varchar(30)

		/// <summary>
		/// 菜单权限
		/// </summary>
		[Column("MenuRights",         DataType=AntData.ORM.DataType.VarChar,  Length=150, Comment="菜单权限"),    Nullable]
		public virtual string MenuRights { get; set; } // varchar(150)

		/// <summary>
		/// 角色Tid(一个人只有一个角色)
		/// </summary>
		[Column("RoleTid",            DataType=AntData.ORM.DataType.Int64,    Comment="角色Tid(一个人只有一个角色)"), NotNull]
		public virtual long RoleTid { get; set; } // bigint(20)

		/// <summary>
		/// 最后登录系统时间
		/// </summary>
		[Column("LastLoginTime",      DataType=AntData.ORM.DataType.DateTime, Comment="最后登录系统时间"),    Nullable]
		public virtual DateTime? LastLoginTime { get; set; } // datetime

		/// <summary>
		/// 登录的浏览器信息
		/// </summary>
		[Column("UserAgent",          DataType=AntData.ORM.DataType.VarChar,  Length=500, Comment="登录的浏览器信息"),    Nullable]
		public virtual string UserAgent { get; set; } // varchar(500)

		/// <summary>
		/// 创建的角色名称
		/// </summary>
		[Column("CreateRoleName",     DataType=AntData.ORM.DataType.VarChar,  Length=500, Comment="创建的角色名称"),    Nullable]
		public virtual string CreateRoleName { get; set; } // varchar(500)

		/// <summary>
		/// 创建者
		/// </summary>
		[Column("CreateUser",         DataType=AntData.ORM.DataType.VarChar,  Length=50, Comment="创建者"),    Nullable]
		public virtual string CreateUser { get; set; } // varchar(50)

		#endregion

		#region Field

		private DateTime _DataChangeLastTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;

		#endregion
	}

	[Table(Database="lito", Name="translation")]
	public partial class Translation : CanalMqBasic
	{
		#region Column

		[Column("Tid",    DataType=AntData.ORM.DataType.VarChar, Length=32), PrimaryKey, NotNull]
		public virtual string Tid { get; set; } // varchar(32)

		/// <summary>
		/// 不填就是通用的意思
		/// </summary>
		[Column("Column", DataType=AntData.ORM.DataType.VarChar, Length=255, Comment="不填就是通用的意思"),    Nullable]
		public virtual string Column { get; set; } // varchar(255)

		/// <summary>
		/// 英文单词
		/// </summary>
		[Column("WordEn", DataType=AntData.ORM.DataType.VarChar, Length=255, Comment="英文单词"), NotNull]
		public virtual string WordEn { get; set; } // varchar(255)

		/// <summary>
		/// 中文
		/// </summary>
		[Column("WordCh", DataType=AntData.ORM.DataType.VarChar, Length=255, Comment="中文"), NotNull]
		public virtual string WordCh { get; set; } // varchar(255)

		#endregion
	}

	/// <summary>
	/// 用户表
	/// </summary>
	[Table(Database="lito", Comment="用户表", Name="user")]
	public partial class User : CanalMqBasic
	{
		#region Column

		/// <summary>
		/// 主键
		/// </summary>
		[Column("Tid",                DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="主键"), PrimaryKey, NotNull]
		public virtual string Tid { get; set; } // varchar(32)

		/// <summary>
		/// 邮箱
		/// </summary>
		[Column("Email",              DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="邮箱"),    Nullable]
		public virtual string Email { get; set; } // varchar(255)

		/// <summary>
		/// 登录用户名
		/// </summary>
		[Column("LoginName",          DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="登录用户名"),    Nullable]
		public virtual string LoginName { get; set; } // varchar(255)

		/// <summary>
		/// 手机号的国家码，例如中国是86
		/// </summary>
		[Column("PhoneRegionCode",    DataType=AntData.ORM.DataType.VarChar,  Length=16, Comment="手机号的国家码，例如中国是86"),    Nullable]
		public virtual string PhoneRegionCode { get; set; } // varchar(16)

		/// <summary>
		/// 手机号码
		/// </summary>
		[Column("PhoneNumber",        DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="手机号码"),    Nullable]
		public virtual string PhoneNumber { get; set; } // varchar(255)

		/// <summary>
		/// 密码MD5加密
		/// </summary>
		[Column("PasswordMD5",        DataType=AntData.ORM.DataType.VarChar,  Length=64, Comment="密码MD5加密"), NotNull]
		public virtual string PasswordMD5 { get; set; } // varchar(64)

		/// <summary>
		/// 所在城市
		/// </summary>
		[Column("Address",            DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="所在城市"),    Nullable]
		public virtual string Address { get; set; } // varchar(255)

		/// <summary>
		/// 是否为男性
		/// </summary>
		[Column("IsMale",             DataType=AntData.ORM.DataType.Boolean,  Comment="是否为男性"), NotNull]
		public virtual bool IsMale { get; set; } // tinyint(1)

		/// <summary>
		/// 出生日期
		/// </summary>
		[Column("Birthday",           DataType=AntData.ORM.DataType.DateTime, Comment="出生日期"),    Nullable]
		public virtual DateTime? Birthday { get; set; } // datetime

		/// <summary>
		/// 名字（不含姓）
		/// </summary>
		[Column("FirstName",          DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="名字（不含姓）"),    Nullable]
		public virtual string FirstName { get; set; } // varchar(255)

		/// <summary>
		/// 姓氏
		/// </summary>
		[Column("LastName",           DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="姓氏"),    Nullable]
		public virtual string LastName { get; set; } // varchar(255)

		/// <summary>
		/// 上次登录的时间，UTC
		/// </summary>
		[Column("LastLoginTimeUTC",   DataType=AntData.ORM.DataType.DateTime, Comment="上次登录的时间，UTC"), NotNull]
		public virtual DateTime LastLoginTimeUTC // datetime
		{
			get { return _LastLoginTimeUTC; }
			set { _LastLoginTimeUTC = value; }
		}

		/// <summary>
		/// 上次登录的IP
		/// </summary>
		[Column("LastLoginIP",        DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="上次登录的IP"),    Nullable]
		public virtual string LastLoginIP { get; set; } // varchar(32)

		/// <summary>
		/// 用户等级
		/// </summary>
		[Column("Level",              DataType=AntData.ORM.DataType.Int32,    Comment="用户等级"), NotNull]
		public virtual int Level { get; set; } // int(2)

		/// <summary>
		/// 注册时间
		/// </summary>
		[Column("CreateTimeUTC",      DataType=AntData.ORM.DataType.DateTime, Comment="注册时间"), NotNull]
		public virtual DateTime CreateTimeUTC // datetime
		{
			get { return _CreateTimeUTC; }
			set { _CreateTimeUTC = value; }
		}

		/// <summary>
		/// 最后更新时间
		/// </summary>
		[Column("DataChangeLastTime", DataType=AntData.ORM.DataType.DateTime, Comment="最后更新时间"), NotNull]
		public virtual DateTime DataChangeLastTime // datetime
		{
			get { return _DataChangeLastTime; }
			set { _DataChangeLastTime = value; }
		}

		#endregion

		#region Field

		private DateTime _LastLoginTimeUTC = System.Data.SqlTypes.SqlDateTime.MinValue.Value;
		private DateTime _CreateTimeUTC = System.Data.SqlTypes.SqlDateTime.MinValue.Value;
		private DateTime _DataChangeLastTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;

		#endregion
	}

	/// <summary>
	/// 用户收件地址
	/// </summary>
	[Table(Database="lito", Comment="用户收件地址", Name="user_address")]
	public partial class UserAddress : CanalMqBasic
	{
		#region Column

		/// <summary>
		/// 主键
		/// </summary>
		[Column("Tid",                DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="主键"), PrimaryKey, NotNull]
		public virtual string Tid { get; set; } // varchar(32)

		/// <summary>
		/// 最后更新时间
		/// </summary>
		[Column("DataChangeLastTime", DataType=AntData.ORM.DataType.DateTime, Comment="最后更新时间"), NotNull]
		public virtual DateTime DataChangeLastTime // datetime
		{
			get { return _DataChangeLastTime; }
			set { _DataChangeLastTime = value; }
		}

		/// <summary>
		/// 用户表主键
		/// </summary>
		[Column("UserTid",            DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="用户表主键"),    Nullable]
		public virtual string UserTid { get; set; } // varchar(32)

		/// <summary>
		/// 姓氏
		/// </summary>
		[Column("FirstName",          DataType=AntData.ORM.DataType.VarChar,  Length=50, Comment="姓氏"),    Nullable]
		public virtual string FirstName { get; set; } // varchar(50)

		/// <summary>
		/// 名字
		/// </summary>
		[Column("LastName",           DataType=AntData.ORM.DataType.VarChar,  Length=50, Comment="名字"),    Nullable]
		public virtual string LastName { get; set; } // varchar(50)

		/// <summary>
		/// 国家
		/// </summary>
		[Column("Country",            DataType=AntData.ORM.DataType.VarChar,  Length=50, Comment="国家"),    Nullable]
		public virtual string Country { get; set; } // varchar(50)

		/// <summary>
		/// 省
		/// </summary>
		[Column("Province",           DataType=AntData.ORM.DataType.VarChar,  Length=50, Comment="省"),    Nullable]
		public virtual string Province { get; set; } // varchar(50)

		/// <summary>
		/// 城市
		/// </summary>
		[Column("City",               DataType=AntData.ORM.DataType.VarChar,  Length=50, Comment="城市"),    Nullable]
		public virtual string City { get; set; } // varchar(50)

		/// <summary>
		/// 地址
		/// </summary>
		[Column("Address",            DataType=AntData.ORM.DataType.VarChar,  Length=200, Comment="地址"),    Nullable]
		public virtual string Address { get; set; } // varchar(200)

		/// <summary>
		/// 街道
		/// </summary>
		[Column("Street",             DataType=AntData.ORM.DataType.VarChar,  Length=200, Comment="街道"),    Nullable]
		public virtual string Street { get; set; } // varchar(200)

		/// <summary>
		/// 附加地址
		/// </summary>
		[Column("AttachAddress",      DataType=AntData.ORM.DataType.VarChar,  Length=200, Comment="附加地址"),    Nullable]
		public virtual string AttachAddress { get; set; } // varchar(200)

		/// <summary>
		/// 邮编
		/// </summary>
		[Column("PostCode",           DataType=AntData.ORM.DataType.VarChar,  Length=20, Comment="邮编"),    Nullable]
		public virtual string PostCode { get; set; } // varchar(20)

		/// <summary>
		/// 电话
		/// </summary>
		[Column("Phone",              DataType=AntData.ORM.DataType.VarChar,  Length=20, Comment="电话"),    Nullable]
		public virtual string Phone { get; set; } // varchar(20)

		/// <summary>
		/// 是否默认
		/// </summary>
		[Column("IsDefault",          DataType=AntData.ORM.DataType.Boolean,  Comment="是否默认"), NotNull]
		public virtual bool IsDefault { get; set; } // tinyint(1)

		#endregion

		#region Field

		private DateTime _DataChangeLastTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;

		#endregion
	}

	/// <summary>
	/// 存储用户的身份认证信息，用于报关
	/// </summary>
	[Table(Database="lito", Comment="存储用户的身份认证信息，用于报关", Name="user_certification_info")]
	public partial class UserCertificationInfo : CanalMqBasic
	{
		#region Column

		[Column("Tid",                DataType=AntData.ORM.DataType.VarChar,  Length=32), PrimaryKey, NotNull]
		public virtual string Tid { get; set; } // varchar(32)

		/// <summary>
		/// 创建者的用户ID
		/// </summary>
		[Column("OwnerUserId",        DataType=AntData.ORM.DataType.VarChar,  Length=32, Comment="创建者的用户ID"), NotNull]
		public virtual string OwnerUserId { get; set; } // varchar(32)

		/// <summary>
		/// 名
		/// </summary>
		[Column("FirstName",          DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="名"), NotNull]
		public virtual string FirstName { get; set; } // varchar(255)

		/// <summary>
		/// 姓
		/// </summary>
		[Column("LastName",           DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="姓"), NotNull]
		public virtual string LastName { get; set; } // varchar(255)

		/// <summary>
		/// 身份证号
		/// </summary>
		[Column("IdNumber",           DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="身份证号"), NotNull]
		public virtual string IdNumber { get; set; } // varchar(255)

		/// <summary>
		/// 身份证正面照片
		/// </summary>
		[Column("IdImgFont",          DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="身份证正面照片"),    Nullable]
		public virtual string IdImgFont { get; set; } // varchar(255)

		/// <summary>
		/// 身份证背面照片
		/// </summary>
		[Column("IdImgBack",          DataType=AntData.ORM.DataType.VarChar,  Length=255, Comment="身份证背面照片"),    Nullable]
		public virtual string IdImgBack { get; set; } // varchar(255)

		/// <summary>
		/// 是否已经人工验证通过了
		/// </summary>
		[Column("IsCertificated",     DataType=AntData.ORM.DataType.Boolean,  Comment="是否已经人工验证通过了"), NotNull]
		public virtual bool IsCertificated { get; set; } // tinyint(1)

		[Column("DataChangeLastTime", DataType=AntData.ORM.DataType.DateTime), NotNull]
		public virtual DateTime DataChangeLastTime // datetime
		{
			get { return _DataChangeLastTime; }
			set { _DataChangeLastTime = value; }
		}

		#endregion

		#region Field

		private DateTime _DataChangeLastTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;

		#endregion
	}

	public static partial class TableExtensions
	{
		public static Brand FindByBk(this IQueryable<Brand> table, string Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<Brand> FindByBkAsync(this IQueryable<Brand> table, string Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static Cart FindByBk(this IQueryable<Cart> table, string Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<Cart> FindByBkAsync(this IQueryable<Cart> table, string Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static Category FindByBk(this IQueryable<Category> table, long Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<Category> FindByBkAsync(this IQueryable<Category> table, long Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static CategoryImportRelation FindByBk(this IQueryable<CategoryImportRelation> table, long Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<CategoryImportRelation> FindByBkAsync(this IQueryable<CategoryImportRelation> table, long Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static Channel FindByBk(this IQueryable<Channel> table, string Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<Channel> FindByBkAsync(this IQueryable<Channel> table, string Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static Color FindByBk(this IQueryable<Color> table, string ColorNameEn)
		{
			return table.FirstOrDefault(t =>
				t.ColorNameEn == ColorNameEn);
		}

		public static async Task<Color> FindByBkAsync(this IQueryable<Color> table, string ColorNameEn)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.ColorNameEn == ColorNameEn);
		}

		public static Config FindByBk(this IQueryable<Config> table, string Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<Config> FindByBkAsync(this IQueryable<Config> table, string Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static ConfigDiscount FindByBk(this IQueryable<ConfigDiscount> table, string Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<ConfigDiscount> FindByBkAsync(this IQueryable<ConfigDiscount> table, string Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static DeliveryOrder FindByBk(this IQueryable<DeliveryOrder> table, string Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<DeliveryOrder> FindByBkAsync(this IQueryable<DeliveryOrder> table, string Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static Detail FindByBk(this IQueryable<Detail> table, string Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<Detail> FindByBkAsync(this IQueryable<Detail> table, string Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static DetailTranslation FindByBk(this IQueryable<DetailTranslation> table, string Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<DetailTranslation> FindByBkAsync(this IQueryable<DetailTranslation> table, string Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static FreightCategoryRelation FindByBk(this IQueryable<FreightCategoryRelation> table, string Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<FreightCategoryRelation> FindByBkAsync(this IQueryable<FreightCategoryRelation> table, string Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static FreightRegionTypeRelation FindByBk(this IQueryable<FreightRegionTypeRelation> table, string Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<FreightRegionTypeRelation> FindByBkAsync(this IQueryable<FreightRegionTypeRelation> table, string Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static FreightTemplate FindByBk(this IQueryable<FreightTemplate> table, string Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<FreightTemplate> FindByBkAsync(this IQueryable<FreightTemplate> table, string Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static Good FindByBk(this IQueryable<Good> table, string Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<Good> FindByBkAsync(this IQueryable<Good> table, string Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static GoodCategory FindByBk(this IQueryable<GoodCategory> table, string Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<GoodCategory> FindByBkAsync(this IQueryable<GoodCategory> table, string Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static JsReport FindByBk(this IQueryable<JsReport> table, string Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<JsReport> FindByBkAsync(this IQueryable<JsReport> table, string Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static LocationCity FindByBk(this IQueryable<LocationCity> table, string Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<LocationCity> FindByBkAsync(this IQueryable<LocationCity> table, string Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static LocationCountry FindByBk(this IQueryable<LocationCountry> table, string Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<LocationCountry> FindByBkAsync(this IQueryable<LocationCountry> table, string Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static LocationProvince FindByBk(this IQueryable<LocationProvince> table, string Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<LocationProvince> FindByBkAsync(this IQueryable<LocationProvince> table, string Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static Nlog FindByBk(this IQueryable<Nlog> table, string Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<Nlog> FindByBkAsync(this IQueryable<Nlog> table, string Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static OssRecordsHangzhou FindByBk(this IQueryable<OssRecordsHangzhou> table, string Key)
		{
			return table.FirstOrDefault(t =>
				t.Key == Key);
		}

		public static async Task<OssRecordsHangzhou> FindByBkAsync(this IQueryable<OssRecordsHangzhou> table, string Key)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Key == Key);
		}

		public static Preorder FindByBk(this IQueryable<Preorder> table, string Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<Preorder> FindByBkAsync(this IQueryable<Preorder> table, string Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static RegionType FindByBk(this IQueryable<RegionType> table, string Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<RegionType> FindByBkAsync(this IQueryable<RegionType> table, string Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static SalePrice FindByBk(this IQueryable<SalePrice> table, string Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<SalePrice> FindByBkAsync(this IQueryable<SalePrice> table, string Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static SizeGuide FindByBk(this IQueryable<SizeGuide> table, string Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<SizeGuide> FindByBkAsync(this IQueryable<SizeGuide> table, string Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static Sku FindByBk(this IQueryable<Sku> table, string Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<Sku> FindByBkAsync(this IQueryable<Sku> table, string Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static SystemMenu FindByBk(this IQueryable<SystemMenu> table, long Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<SystemMenu> FindByBkAsync(this IQueryable<SystemMenu> table, long Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static SystemPageAction FindByBk(this IQueryable<SystemPageAction> table, string Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<SystemPageAction> FindByBkAsync(this IQueryable<SystemPageAction> table, string Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static SystemRole FindByBk(this IQueryable<SystemRole> table, long Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<SystemRole> FindByBkAsync(this IQueryable<SystemRole> table, long Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static SystemUsers FindByBk(this IQueryable<SystemUsers> table, string Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<SystemUsers> FindByBkAsync(this IQueryable<SystemUsers> table, string Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static Translation FindByBk(this IQueryable<Translation> table, string Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<Translation> FindByBkAsync(this IQueryable<Translation> table, string Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static User FindByBk(this IQueryable<User> table, string Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<User> FindByBkAsync(this IQueryable<User> table, string Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static UserAddress FindByBk(this IQueryable<UserAddress> table, string Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<UserAddress> FindByBkAsync(this IQueryable<UserAddress> table, string Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static UserCertificationInfo FindByBk(this IQueryable<UserCertificationInfo> table, string Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<UserCertificationInfo> FindByBkAsync(this IQueryable<UserCertificationInfo> table, string Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}
	}
}
