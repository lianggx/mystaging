using System;
using System.Linq;
using System.Text.Json;
using MySql.Data.Types;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MyStaging.DataAnnotations;

namespace Mysql.Model
{
	[Table(name: "article", Schema = "mystaging")]
	public partial class Article
	{
		[Column(TypeName = "tinyint(1)")]
		public bool State { get; set; }
		[PrimaryKey(AutoIncrement = true)]
		public int Id { get; set; }
		public int UserId { get; set; }
		public string Title { get; set; }
		public string Content { get; set; }
		public DateTime CreateTime { get; set; }
		public string IP { get; set; }
	}
}
