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
    [Table(name: "article", Schema = "mystaging")]
    public partial class Article
    {
        [Column(TypeName = "char(36)")]
        public Guid AID { get; set; }
        [Column(TypeName = "tinyint(1)")]
        public bool State { get; set; }
        [PrimaryKey(AutoIncrement = true)]
        public int id { get; set; }
        public string userid { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public DateTime createtime { get; set; }
        public int? age { get; set; }
        [Column(TypeName = "double(10,2)")]
        public double? money { get; set; }
        public long total { get; set; }
        public string IP { get; set; }
    }
}
