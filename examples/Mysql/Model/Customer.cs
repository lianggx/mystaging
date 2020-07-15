using MyStaging.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

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
