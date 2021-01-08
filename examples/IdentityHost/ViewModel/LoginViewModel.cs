using System.ComponentModel.DataAnnotations;

namespace IdentityHost.ViewModel
{
    public class LoginViewModel
    {
        /// <summary>
        ///  登录名
        /// </summary>
        [Required, MinLength(1, ErrorMessage = "账号长度1-10位"), MaxLength(6, ErrorMessage = "账号长度1-10位")]
        public string LoginName { get; set; }
        /// <summary>
        ///  密码
        /// </summary>
        [Required, MinLength(6, ErrorMessage = "密码长度6-16位"), MaxLength(16, ErrorMessage = "密码长度6-16位")]
        public string Password { get; set; }
    }
}
