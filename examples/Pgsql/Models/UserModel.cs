using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyStaging.Mapping;
using NpgsqlTypes;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pgsql.Model
{
	[Table(name: "user", Schema = "public")]
	public partial class UserModel
	{
		[PrimaryKey]
		public string id { get; set; }
		public string loginname { get; set; }
		public string password { get; set; }
		public string nickname { get; set; }
		public bool? sex { get; set; }
		public int age { get; set; }
		public decimal money { get; set; }
		public DateTime createtime { get; set; }
		public decimal wealth { get; set; }
		public Et_role? role { get; set; }
		public string IP { get; set; }
	}
}
