using System.ComponentModel.DataAnnotations;

namespace IdentityHost.ViewModel
{
    public class PasswordViewModel
    {
        /// <summary>
        ///  旧密码
        /// </summary>
        [Required, MinLength(6, ErrorMessage = "密码长度6-16位"), MaxLength(16, ErrorMessage = "密码长度6-16位")] 
        public string OldPassword { get; set; }
        /// <summary>
        ///  新密码
        /// </summary>
        [Required, MinLength(6, ErrorMessage = "密码长度6-16位"), MaxLength(16, ErrorMessage = "密码长度6-16位")] 
        public string NewPassword { get; set; }
        /// <summary>
        ///  重复新密码
        /// </summary>
        [Required, MinLength(6, ErrorMessage = "密码长度6-16位"), MaxLength(16, ErrorMessage = "密码长度6-16位"), Compare("NewPassword", ErrorMessage = "两次密码输入不一致")] 
        public string RePassword { get; set; }
    }
}
