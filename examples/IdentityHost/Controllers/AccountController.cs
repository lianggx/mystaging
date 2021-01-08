using IdentityHost.Helpers;
using IdentityHost.Model;
using IdentityHost.Services;
using IdentityHost.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityHost.Controllers
{
    [Route("account"), ApiExplorerSettings(GroupName = "个人中心")]
    public class AccountController : BaseController
    {
        private const int ExpireTime = 60 * 60 * 1000;
        private readonly UserService userService;
        private readonly ResourceService resourceService;
        private readonly RoleService roleService;

        public AccountController(IConfiguration cfg,
            ILogger<AccountController> logger, IEnumerable<IManagerService> managerServices,ConnectionMultiplexer multiplexer) : base(cfg, logger, managerServices,multiplexer)
        {
            userService = GetService<UserService>();
            resourceService = GetService<ResourceService>();
            roleService = GetService<RoleService>();
        }

        /// <summary>
        ///  登入
        /// </summary>
        /// <remarks>
        /// <code>
        ///  detail:{
        ///      id:
        ///      img_face:
        ///      name:
        ///      code:
        ///     },
        ///     menus:[{
        ///         id:
        ///         name:
        ///         resource
        ///     }],
        ///     roles:[{
        ///           id:
        ///           name:
        ///     }]
        /// </code>
        /// </remarks>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] LoginViewModel model)
        {
            var password = SecurityHelper.GetSHA256SignString(model.Password);
            var user = userService.Detail(model.LoginName);

            if (user == null || user.State != 0)
                return APIReturn.记录不存在;
            else if (user.Password != password)
                return APIReturn.失败.SetMessage("用户名或者密码错误");
            else
                return await SignIn(user);
        }

        private async Task<APIReturn> SignIn(M_User user)
        {
            var token = Guid.NewGuid().ToString("N");
            var key = SignInKey + token;
            await redisClient.GetDatabase().StringSetAsync(key, user.Id);
            await redisClient.GetDatabase().KeyExpireAsync(key, TimeSpan.FromSeconds(ExpireTime));
            var detail = new
            {
                user.Id,
                user.ImgFace,
                user.Name,
                user.LoginName,
                token
            };

            return APIReturn.成功.SetData("detail", detail);
        }

        /// <summary>
        ///  登出
        /// </summary>
        /// <returns></returns>
        [HttpPost("signout")]
        public async Task<IActionResult> SignOut()
        {
            if (!string.IsNullOrEmpty(base.Token))
            {
                await redisClient.GetDatabase().KeyDeleteAsync(SignInKey + Token);
            }
            return APIReturn.成功;
        }

        /// <summary>
        ///  我的个人资料
        /// </summary>
        /// <remarks>
        /// <code>
        ///  detail:{
        ///      id: id
        ///      img_face: 头像
        ///      name: 姓名
        ///      code: 员工编号
        ///      bonus: 绩效奖金
        ///      dept: 部门
        ///      salary: 基础工资
        ///      phone: 手机号码
        ///      level: 职级
        ///      gender: 性别，null=未知，0=女，1=男
        ///     }
        /// </code>
        /// </remarks>
        /// <returns></returns>
        [HttpPost("myinfo")]
        public IActionResult MyInfo()
        {
            var user = userService.Detail(LoginUser.Id);
            // 查找角色和权限
            var roles = roleService.GetRoles(user.Id);

            // 角色可访问的页面菜单
            var menus = new List<M_Resource>();
            if (roles.Count > 0)
            {
                var roleid = roles.Select(f => f.Id).ToArray();
                menus = resourceService.ResourceByRole(roleid);

            }

            return APIReturn.成功.SetData("detail", new
            {
                user.Id,
                user.Name,
                user.LoginName,
                user.ImgFace
            }, "roles", roles.Select(f => new
            {
                f.Id,
                f.Name
            }), "menus", menus.Select(f => new
            {
                f.Id,
                f.ParentId,
                f.Name,
                f.Type,
                f.Content,
                f.Sort,
                f.Authorize,
                Children = f.Children?.Select(c => new
                {
                    c.Id,
                    c.ParentId,
                    c.Name,
                    c.Type,
                    c.Content,
                    c.Sort,
                    c.Authorize,
                })
            }));
        }
    }
}