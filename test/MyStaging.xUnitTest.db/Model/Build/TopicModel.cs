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
	[EntityMapping(name: "topic", Schema = "public")]
	public partial class TopicModel
	{
		[PrimaryKey]
		public Guid Id { get; set; }

		public string Title { get; set; }

		public DateTime? Create_time { get; set; }

		public DateTime? Update_time { get; set; }

		public DateTime? Last_time { get; set; }

		public Guid? User_id { get; set; }

		public string Name { get; set; }

		public int? Age { get; set; }

		public bool? Sex { get; set; }

		public DateTime? Createtime { get; set; }

		public TimeSpan? Updatetime { get; set; }

		[NonDbColumnMapping, JsonIgnore] public MyStaging.xUnitTest.DAL.Topic.TopicUpdateBuilder UpdateBuilder { get { return new MyStaging.xUnitTest.DAL.Topic.TopicUpdateBuilder(model =>{MyStaging.Helpers.MyStagingUtils.CopyProperty<TopicModel>(this, model); ContextManager.CacheManager?.RemoveItemCache<TopicModel>(this.Id.ToString()); }, this.Id); } }

		public TopicModel Insert() { return MyStaging.xUnitTest.DAL.Topic.Insert(this); }

	}
}
