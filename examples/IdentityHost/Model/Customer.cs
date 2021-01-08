using MyStaging.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityHost.Model
{
    [Table(name: "Customer", Schema = "mystaging")]
	public partial class Customer
	{
		[PrimaryKey(AutoIncrement = true)]
		public int Id { get; set; }
		public string Name { get; set; }
	}
}
