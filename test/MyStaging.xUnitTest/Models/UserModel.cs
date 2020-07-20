using MyStaging.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyStaging.xUnitTest.Models
{
    [Table(name: "user", Schema = "mystaging")]
    public partial class UserModel
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string LoginName { get; set; }
        public string Password { get; set; }
        public string Nickname { get; set; }
        public bool? Sex { get; set; }
        public int Age { get; set; }
        public decimal Money { get; set; }
        public DateTime CreateTime { get; set; }
        public decimal Wealth { get; set; }
        public string IP { get; set; }
    }
}
