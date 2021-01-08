using MyStaging.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Pgsql.Model
{
    [Table(name: "article", Schema = "public")]
    public partial class Article
    {
        [PrimaryKey]
        public string id { get; set; }
        [PrimaryKey]
        public string userid { get; set; }
        public string title { get; set; }
        public JsonElement content { get; set; }
        public DateTime createtime { get; set; }
    }
}
