using System;
using System.Linq;
using System.Text.Json;
using MySql.Data.Types;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MyStaging.DataAnnotations;

namespace Mysql.Model
{
	[Table(name: "customer", Schema = "mystaging")]
	public partial class Customer
	{
		[PrimaryKey(AutoIncrement = true)]
		public int Id { get; set; }
		public string Name { get; set; }
	}
}
