using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyStaging.Mapping;
using NpgsqlTypes;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pgsql.Model
{
	[Table(name: "topic", Schema = "public")]
	public partial class TopicModel
	{
		[PrimaryKey]
		public Guid id { get; set; }
		public string title { get; set; }
		public DateTime? create_time { get; set; }
		public DateTime? update_time { get; set; }
		public DateTime? last_time { get; set; }
		public Guid? user_id { get; set; }
		public string name { get; set; }
		public int? age { get; set; }
		public bool? sex { get; set; }
		public DateTime? createtime { get; set; }
		public TimeSpan? updatetime { get; set; }
	}
}
