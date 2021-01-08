using MyStaging.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mysql.Model
{
    [Table(name: "M_Roleresource", Schema = "mystaging")]
	public partial class M_Roleresource
	{
		/// <summary>
		/// 角色编号
		/// </summary>
		[PrimaryKey]
		public int RoleId { get; set; }
		/// <summary>
		/// 资源编号
		/// </summary>
		[PrimaryKey]
		public int ResourceId { get; set; }
	}
}
