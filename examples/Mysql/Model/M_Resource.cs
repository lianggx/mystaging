using MyStaging.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mysql.Model
{
    [Table(name: "M_Resource", Schema = "mystaging")]
	public partial class M_Resource
	{
		[PrimaryKey(AutoIncrement = true)]
		public int Id { get; set; }
		/// <summary>
		/// 上级编号
		/// </summary>
		public int? ParentId { get; set; }
		/// <summary>
		/// 资源名称
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// 资源内容
		/// </summary>
		public string Content { get; set; }
		/// <summary>
		/// 资源类型，0=API，1=网页元素
		/// </summary>
		public int Type { get; set; }
		/// <summary>
		/// 状态，0=正常，1=冻结，2=删除
		/// </summary>
		public int State { get; set; }
		/// <summary>
		/// 是否需要授权访问
		/// </summary>
		[Column(TypeName = "tinyint(1)")]
		public bool Authorize { get; set; }
		/// <summary>
		/// 排序号，按数字顺序排序
		/// </summary>
		public int Sort { get; set; }
		/// <summary>
		/// 创建时间
		/// </summary>
		public DateTime CreateTime { get; set; }
	}
}
