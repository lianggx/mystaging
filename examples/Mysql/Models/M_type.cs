using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MySql.Data.Types;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Mysql.Model
{
	[Table(name: "m_type", Schema = "mystaging")]
	public partial class M_type
	{
		[Key]
		public sbyte t1 { get; set; }
		public short t2 { get; set; }
		[Column(TypeName = "mediumint(9)")]
		public int? t3 { get; set; }
		public int? t4 { get; set; }
		public int? t5 { get; set; }
		public long? t6 { get; set; }
		public ulong? t7 { get; set; }
		public double? t8 { get; set; }
		public double? t9 { get; set; }
		public float? t10 { get; set; }
		[Column(TypeName = "decimal(10,2)")]
		public decimal? t11 { get; set; }
		public decimal? t12 { get; set; }
		public string t13 { get; set; }
		public string t14 { get; set; }
		[Column(TypeName = "date")]
		public DateTime? t15 { get; set; }
		public TimeSpan? t16 { get; set; }
		[Column(TypeName = "year(4)")]
		public DateTime? t17 { get; set; }
		[Column(TypeName = "timestamp(6)")]
		public DateTime? t18 { get; set; }
		public DateTime? t19 { get; set; }
		[Column(TypeName = "tinyblob")]
		public byte[] t20 { get; set; }
		[Column(TypeName = "blob")]
		public byte[] t21 { get; set; }
		[Column(TypeName = "mediumblob")]
		public byte[] t22 { get; set; }
		[Column(TypeName = "longblob")]
		public byte[] t23 { get; set; }
		[Column(TypeName = "tinytext")]
		public string t24 { get; set; }
		[Column(TypeName = "text")]
		public string t25 { get; set; }
		[Column(TypeName = "mediumtext")]
		public string t26 { get; set; }
		[Column(TypeName = "enum('')")]
		public string t27 { get; set; }
		public byte[] t29 { get; set; }
		[Column(TypeName = "varbinary(255)")]
		public byte[] t30 { get; set; }
		public JToken t39 { get; set; }
		/// <summary>
		/// BOOLEAN
		/// </summary>
		[Column(TypeName = "tinyint(1)")]
		public bool? T40 { get; set; }
		/// <summary>
		/// GUID
		/// </summary>
		[Column(TypeName = "char(36)")]
		public Guid? t41 { get; set; }
	}
}
