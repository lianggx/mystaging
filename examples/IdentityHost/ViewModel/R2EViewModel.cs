using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IdentityHost.ViewModel
{
    public class R2UViewModel
    {
        /// <summary>
        ///  角色Id
        /// </summary>
        public List<int> RoleId { get; set; } = new List<int>();
        /// <summary>
        ///  员工Id
        /// </summary>
        [Required] public int UserId { get; set; }
    }
}
