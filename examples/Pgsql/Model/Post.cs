﻿using MyStaging.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Pgsql.Model
{
    [Table(name: "post", Schema = "public")]
    public partial class Post
    {
        [PrimaryKey]
        public Guid id { get; set; }
        [Required]
        public string title { get; set; }
        public JsonElement content { get; set; }
        [Column(TypeName = "public.et_data_state")]
        public et_data_state? state { get; set; }
        [Column(TypeName = "public.et_role")]
        public et_role? role { get; set; }
        [Column(TypeName = "json")]
        public JsonElement text { get; set; }
    }
}
