using MyStaging.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityHost.Model
{
    [Table(name: "M_Role", Schema = "mystaging")]
	public partial class M_Role
	{
		/// <summary>
		/// 编号
		/// </summary>
		[PrimaryKey(AutoIncrement = true)]
		public int Id { get; set; }
		/// <summary>
		/// 名称
		/// </summary>
		[Required]
		public string Name { get; set; }
		/// <summary>
		/// 状态，0=正常，1=冻结，2=删除
		/// </summary>
		public int State { get; set; }
		/// <summary>
		/// 创建时间
		/// </summary>
		public DateTime CreateTime { get; set; }
	}
}
