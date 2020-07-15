using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MySql.Data.Types;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MyStaging.DataAnnotations;

namespace Mysql.Model
{
	[Table(name: "test_id", Schema = "mystaging")]
	public partial class Test_id
	{
		public int id { get; set; }
		[Column(TypeName = "varchar(11)")]
		public string id2 { get; set; }
	}
}
