using System.ComponentModel.DataAnnotations;

namespace IdentityHost.ViewModel
{
    public class AddRoleViewModel
    {
        /// <summary>
        ///  角色名称
        /// </summary>
        [Required] public string Name { get; set; }
    }

    public class EditRoleViewModel : AddRoleViewModel
    {
        /// <summary>
        ///  Id
        /// </summary>
        [Required] public int Id { get; set; }
    }
}
