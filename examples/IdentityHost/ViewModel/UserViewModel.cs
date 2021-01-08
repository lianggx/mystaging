using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IdentityHost.ViewModel
{
    public class AddM_UserViewModel
    {
        /// <summary>
        ///  姓名
        /// </summary>
        [Required]
        public string Name { get; set; }
        /// <summary>
        ///  头像
        /// </summary>
        public string ImgFace { get; set; }
        /// <summary>
        ///  登录名
        /// </summary>
        [Required, MinLength(1, ErrorMessage = "1-10位"), MaxLength(10, ErrorMessage = "长度1-10位")]
        public string LoginName { get; set; }
        /// <summary>
        ///  手机号码
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        ///  密码
        /// </summary>
        [Required, MinLength(6, ErrorMessage = "密码长度6-16位"), MaxLength(16, ErrorMessage = "密码长度6-16位")]
        public string Password { get; set; }
        /// <summary>
        /// 角色列表
        /// </summary>
        public List<int> Role { get; set; } = new List<int>();
    }

    public class EditUserViewModel
    {
        /// <summary>
        ///  用户编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///  姓名
        /// </summary>
        [Required]
        public string Name { get; set; }
        /// <summary>
        ///  头像
        /// </summary>
        public string ImgFace { get; set; }
        /// <summary>
        ///  手机号码
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 角色列表
        /// </summary>
        public List<int> Role { get; set; } = new List<int>();
    }
}
