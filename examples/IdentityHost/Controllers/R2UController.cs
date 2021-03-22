using IdentityHost.Services;
using IdentityHost.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Collections.Generic;

namespace IdentityHost.Controllers
{
    /// <summary>
    ///  Role 2 User
    /// </summary>
    [Route("admin/r2e"), ApiExplorerSettings(GroupName = "管理员")]
    public class R2UController : BaseController
    {
        private readonly UserService userService;
        private readonly RoleService roleService;
        public R2UController(IConfiguration cfg, ILogger<R2UController> logger, IEnumerable<IManagerService> managerServices, ConnectionMultiplexer multiplexer) : base(cfg, logger, managerServices, multiplexer)
        {
            userService = GetService<UserService>();
            roleService = GetService<RoleService>();
        }

        /// <summary>
        ///  添加授权
        /// </summary>
        /// <remarks>
        /// <code>
        /// data:{
        ///     id:
        ///     }
        /// </code>
        /// </remarks>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("add")]
        public IActionResult Add([FromBody] R2UViewModel model)
        {
            var user = userService.Detail(model.UserId);
            if (user == null && user.State != 3)
                return APIResult.记录不存在;

            var result = roleService.AddR2U(user.Id, model.RoleId.ToArray());

            return result ? APIResult.成功 : APIResult.失败;
        }
    }
}
