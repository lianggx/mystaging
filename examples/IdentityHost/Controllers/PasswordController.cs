using IdentityHost.Helpers;
using IdentityHost.Services;
using IdentityHost.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityHost.Controllers
{
    [Route("password"), ApiExplorerSettings(GroupName = "个人中心")]
    public class PasswordController : BaseController
    {
        private readonly UserService userService;
        public PasswordController(IConfiguration cfg, ILogger<PasswordController> logger, IEnumerable<IManagerService> managerServices, ConnectionMultiplexer multiplexer) : base(cfg, logger, managerServices, multiplexer)
        {
            userService = GetService<UserService>();
        }

        /// <summary>
        ///  修改密码
        /// </summary>
        /// <remarks>
        /// 调用成功自动清除登录信息
        /// </remarks>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("edit")]
        public async Task<IActionResult> Edit([FromBody] PasswordViewModel model)
        {
            var password = SecurityHelper.GetSHA256SignString(model.OldPassword);
            if (LoginUser.Password != password)
                return APIReturn.失败.SetMessage("旧密码错误");

            var result = userService.UpdatePassword(LoginUser.Id, model.NewPassword);

            if (!string.IsNullOrEmpty(base.Token))
            {
                await redisClient.GetDatabase().KeyDeleteAsync(SignInKey + Token);
            }

            return result ? APIReturn.成功 : APIReturn.失败;
        }
    }
}
