using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NpgsqlTypes;
using System.ComponentModel.DataAnnotations.Schema;
using MyStaging.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace Pgsql.Model
{
	[Table(name: "post", Schema = "public")]
	public partial class Post
	{
		[PrimaryKey]
		public Guid id { get; set; }
		[Required]
		public string title { get; set; }
		public JToken content { get; set; }
		[Column(TypeName = "public.et_data_state")]
		public et_data_state? state { get; set; }
		[Column(TypeName = "public.et_role")]
		public et_role? role { get; set; }
		[Column(TypeName = "json")]
		public JToken text { get; set; }
	}
}
