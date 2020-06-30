using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NpgsqlTypes;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Pgsql.Model
{
	[Table(name: "article", Schema = "public")]
	public partial class ArticleModel
	{
		[Key]
		public string id { get; set; }
		[Key]
		public string userid { get; set; }
		public string title { get; set; }
		public JToken content { get; set; }
		public DateTime createtime { get; set; }
	}
}
