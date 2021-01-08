using MyStaging.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mysql.Model
{
    [Table(name: "Article", Schema = "mystaging")]
	public partial class Article
	{
		[PrimaryKey(AutoIncrement = true)]
		public int Id { get; set; }
		[Column(TypeName = "tinyint(1)")]
		public bool State { get; set; }
		public int UserId { get; set; }
		public string Title { get; set; }
		public string Content { get; set; }
		public DateTime CreateTime { get; set; }
		public string IP { get; set; }
	}
}
