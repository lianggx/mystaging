using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NpgsqlTypes;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Pgsql.Model
{
	[Table(name: "post", Schema = "public")]
	public partial class PostModel
	{
		[Key]
		public Guid id { get; set; }
		[Required]
		public string title { get; set; }
		public JToken content { get; set; }
		[DataType("public.et_data_state")]
		public et_data_state? state { get; set; }
		[DataType("public.et_role")]
		public et_role? role { get; set; }
		[DataType("json")]
		public JToken text { get; set; }
	}
}
