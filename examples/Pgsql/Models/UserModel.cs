using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NpgsqlTypes;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Pgsql.Model
{
	[Table(name: "user", Schema = "public")]
	public partial class UserModel
	{
		[Key]
		[StringLength(36)]
		public string id { get; set; }
		[Required]
		[StringLength(200)]
		public string loginname { get; set; }
		[StringLength(50)]
		public string password { get; set; }
		public string nickname { get; set; }
		public bool? sex { get; set; }
		public int age { get; set; }
		[StringLength(10, MinimumLength = 2)]
		[DataType("numeric")]
		public decimal money { get; set; }
		public DateTime createtime { get; set; }
		[DataType("money")]
		public decimal wealth { get; set; }
		[DataType("public.et_role")]
		public et_role? role { get; set; }
		public string IP { get; set; }
		public string[] citys { get; set; }
		public byte? sex2 { get; set; }
		public System.Collections.BitArray sex3 { get; set; }
		[DataType("float4")]
		public double? sex4 { get; set; }
		[DataType("float8")]
		public double? sex5 { get; set; }
		public TimeSpan? sex6 { get; set; }
		[DataType("time")]
		public TimeSpan? sex7 { get; set; }
		[DataType("date")]
		public DateTime? sex8 { get; set; }
		public DateTimeOffset? sex9 { get; set; }
		[DataType("timestamptz")]
		public DateTime? sex10 { get; set; }
		[DataType("text")]
		public string sex11 { get; set; }
		public short? sex12 { get; set; }
		public long? sex13 { get; set; }
		[StringLength(1)]
		[DataType("bpchar")]
		public string sex14 { get; set; }
		[DataType("float4")]
		public double? sex15 { get; set; }
	}
}
