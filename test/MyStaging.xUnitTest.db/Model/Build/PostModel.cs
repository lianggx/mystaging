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
		[PrimaryKey]
		public Guid Id { get; set; }

		public string Title { get; set; }

		public JToken Content { get; set; }

		public Et_data_state? State { get; set; }

		public Et_role? Role { get; set; }

		public JToken Text { get; set; }

		[NonDbColumnMapping, JsonIgnore] public MyStaging.xUnitTest.DAL.Post.PostUpdateBuilder UpdateBuilder { get { return new MyStaging.xUnitTest.DAL.Post.PostUpdateBuilder(model =>{MyStaging.Helpers.MyStagingUtils.CopyProperty<PostModel>(this, model); ContextManager.CacheManager?.RemoveItemCache<PostModel>(this.Id.ToString()); }, this.Id); } }

		public PostModel Insert() { return MyStaging.xUnitTest.DAL.Post.Insert(this); }

	}
}
