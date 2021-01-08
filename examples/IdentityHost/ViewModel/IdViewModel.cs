using System.ComponentModel.DataAnnotations;

namespace IdentityHost.ViewModel
{
    public class IdViewModel
    {
        /// <summary>
        ///  Id
        /// </summary>
        [Required] public int Id { get; set; }
    }
}
