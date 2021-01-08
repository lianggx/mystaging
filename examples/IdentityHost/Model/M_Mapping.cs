using MyStaging.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityHost.Model
{
    [Table(name: "M_Mapping", Schema = "mystaging")]
	public partial class M_Mapping
	{
		/// <summary>
		/// 用户编号
		/// </summary>
		[PrimaryKey]
		public int UserId { get; set; }
		/// <summary>
		/// 角色编号
		/// </summary>
		[PrimaryKey]
		public int RoleId { get; set; }
		/// <summary>
		/// 创建时间
		/// </summary>
		public DateTime CreateTime { get; set; }
	}
}
