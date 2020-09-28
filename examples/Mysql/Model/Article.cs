using System;
using System.Linq;
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
        /// <summary>
        ///  主键，自增
        /// </summary>
        [PrimaryKey(AutoIncrement = true)]
        public int id { get; set; }
        public int userid { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public DateTime createtime { get; set; }
        public string IP { get; set; }
    }
}
