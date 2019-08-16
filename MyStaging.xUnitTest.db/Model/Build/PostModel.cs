using System;
using System.Linq;
using MyStaging.xUnitTest.DAL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyStaging.Mapping;
using NpgsqlTypes;
using MyStaging.Helpers;

namespace MyStaging.xUnitTest.Model
{
	[EntityMapping(name: "post", Schema = "public")]
	public partial class PostModel
	{
		/// <summary>
		/// 主键，编号
		/// </summary>
		[PrimaryKey]
		public Guid Id { get; set; }

		/// <summary>
		/// 标题
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// 内容
		/// </summary>
		public JToken Content { get; set; }

		/// <summary>
		/// 状态
		/// </summary>
		public Et_data_state State { get; set; }

		/// <summary>
		/// 角色
		/// </summary>
		public Et_role[] Role { get; set; }

		/// <summary>
		/// 文本
		/// </summary>
		public JToken Text { get; set; }

		[NonDbColumnMapping, JsonIgnore] public MyStaging.xUnitTest.DAL.Post.PostUpdateBuilder UpdateBuilder { get { return new MyStaging.xUnitTest.DAL.Post.PostUpdateBuilder(model =>{MyStaging.Helpers.MyStagingUtils.CopyProperty<PostModel>(this, model); PgSqlHelper.CacheManager?.RemoveItemCache<PostModel>(this.Id.ToString()); }, this.Id); } }

		public PostModel Insert() { return MyStaging.xUnitTest.DAL.Post.Insert(this); }

	}
}
