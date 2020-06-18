using MyStaging.Mapping;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyStaging.xUnitTest.Models
{
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
        public string IP { get; set; }
    }
}
