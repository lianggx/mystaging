using MyStaging.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mysql.Model
{
    [Table(name: "M_User", Schema = "mystaging")]
	public partial class M_User
	{
		/// <summary>
		/// 编号
		/// </summary>
		[PrimaryKey]
		public int Id { get; set; }
		/// <summary>
		/// 头像
		/// </summary>
		[Column(TypeName = "varchar(700)")]
		public string ImgFace { get; set; }
		/// <summary>
		/// 姓名
		/// </summary>
		[Required]
		public string Name { get; set; }
		/// <summary>
		/// 手机号码
		/// </summary>
		[Column(TypeName = "varchar(11)")]
		public string Phone { get; set; }
		/// <summary>
		/// 登录名
		/// </summary>
		[Required]
		public string LoginName { get; set; }
		/// <summary>
		/// 登录密码
		/// </summary>
		[Required]
		public string Password { get; set; }
		/// <summary>
		/// 状态，0=正常，1=未激活，2=冻结，3=删除
		/// </summary>
		public int State { get; set; }
		/// <summary>
		/// 创建时间
		/// </summary>
		public DateTime CreateTime { get; set; }
	}
}
