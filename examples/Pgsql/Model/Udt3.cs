using MyStaging.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pgsql.Model
{
    [Table(name: "udt3", Schema = "public")]
    public partial class Udt3
    {
        [PrimaryKey(AutoIncrement = true)]
        public int id { get; set; }
        public string name { get; set; }
        public short age { get; set; }
        [Column(TypeName = "numeric")]
        public decimal? a2 { get; set; }
    }
}
