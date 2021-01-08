using MyStaging.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mysql.Model
{
    [Table(name: "M_Accesslog", Schema = "mystaging")]
	public partial class M_Accesslog
	{
		[PrimaryKey(AutoIncrement = true)]
		public int Id { get; set; }
		/// <summary>
		/// 用户编号
		/// </summary>
		public int? UserId { get; set; }
		/// <summary>
		/// 资源内容
		/// </summary>
		public string Resource { get; set; }
		/// <summary>
		/// 资源编号
		/// </summary>
		public int? ResourceId { get; set; }
		/// <summary>
		/// 请求内容
		/// </summary>
		[Column(TypeName = "text")]
		public string ReqContent { get; set; }
		/// <summary>
		/// 响应内容
		/// </summary>
		[Column(TypeName = "text")]
		public string ResContent { get; set; }
		/// <summary>
		/// 客户端IP地址
		/// </summary>
		public string IP { get; set; }
		/// <summary>
		/// 响应代码
		/// </summary>
		public int? Code { get; set; }
		/// <summary>
		/// 备注
		/// </summary>
		public string Remark { get; set; }
		/// <summary>
		/// 创建时间
		/// </summary>
		public DateTime CreateTime { get; set; }
	}
}
