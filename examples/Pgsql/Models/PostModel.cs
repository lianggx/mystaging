using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyStaging.Mapping;
using NpgsqlTypes;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pgsql.Model
{
	[Table(name: "post", Schema = "public")]
	public partial class PostModel
	{
		[PrimaryKey]
		public Guid id { get; set; }
		public string title { get; set; }
		public JToken content { get; set; }
		public Et_data_state? state { get; set; }
		public Et_role? role { get; set; }
		public JToken text { get; set; }
	}
}
