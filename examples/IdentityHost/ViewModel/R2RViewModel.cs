using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IdentityHost.ViewModel
{
    public class R2RViewModel
    {
        /// <summary>
        ///  角色Id
        /// </summary>
        [Required] public int RoleId { get; set; }
        /// <summary>
        ///  资源Id
        /// </summary>
        public List<int> ResourceId { get; set; } = new List<int>();
    }
}
