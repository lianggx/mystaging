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
    ///  Role 2 Resource
    /// </summary>
    [Route("admin/r2r"), ApiExplorerSettings(GroupName = "管理员")]
    public class R2RController : BaseController
    {
        private readonly RoleService roleService;
        public R2RController(IConfiguration cfg, ILogger<R2RController> logger, IEnumerable<IManagerService> managerServices, ConnectionMultiplexer multiplexer) : base(cfg, logger, managerServices, multiplexer)
        {
            roleService = GetService<RoleService>();
        }

        /// <summary>
        ///  添加角色资源权限
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("add")]
        public IActionResult Add([FromBody] R2RViewModel model)
        {
            var role = roleService.Detail(model.RoleId);
            if (role == null)
                return APIResult.记录不存在;

            var result = roleService.AddR2R(model.RoleId, model.ResourceId.ToArray());
            return result ? APIResult.成功 : APIResult.失败;
        }
    }
}
