using IdentityHost.Model;
using IdentityHost.Properties;
using IdentityHost.Services;
using IdentityHost.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;

namespace IdentityHost.Controllers
{
    [Route("user"), ApiExplorerSettings(GroupName = "管理员")]
    public class UserController : BaseController
    {
        private readonly UserService userService;
        private readonly RoleService roleService;
        public UserController(IConfiguration cfg, ILogger<UserController> logger, IEnumerable<IManagerService> managerServices, ConnectionMultiplexer multiplexer) : base(cfg, logger, managerServices, multiplexer)
        {
            userService = GetService<UserService>();
            roleService = GetService<RoleService>();
        }

        /// <summary>
        ///  添加
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("add")]
        public IActionResult Add([FromBody] AddM_UserViewModel model)
        {
            var user = userService.Detail(model.LoginName);
            if (user != null)
                return APIReturn.失败.SetMessage(string.Format(Resource.AlreadyExists, model.LoginName));

            user = userService.Add(new M_User
            {
                LoginName = model.LoginName,
                Password = model.Password,
                Name = model.Name,
                ImgFace = model.ImgFace,
                Phone = model.Phone,
            }, model.Role);


            return APIReturn.成功.SetData("id", user.Id);
        }

        /// <summary>
        ///  编辑
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("edit")]
        public IActionResult Edit([FromBody] EditUserViewModel model)
        {
            var user = userService.Detail(model.Id);
            if (user == null)
                return APIReturn.记录不存在;

            userService.Edit(model.Id, model.Name, model.ImgFace, model.Phone, model.Role);

            return APIReturn.成功;
        }

        /// <summary>
        ///  删除
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("delete")]
        public IActionResult Delete([FromBody] IdViewModel model)
        {
            var user = userService.Detail(model.Id);
            if (user == null)
                return APIReturn.记录不存在;

            var result = userService.Delete(model.Id);

            return result ? APIReturn.成功 : APIReturn.失败;
        }

        /// <summary>
        ///  详情
        /// </summary>
        /// <remarks>
        /// <code>
        ///  data:{
        ///             detail:{
        ///             
        ///             },
        ///             roles:[{
        ///                     id:
        ///                     name:
        ///             }]
        /// }
        /// </code>
        /// </remarks>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("detail")]
        public IActionResult Detail([FromBody] IdViewModel model)
        {
            var user = userService.Detail(model.Id);
            if (user == null)
                return APIReturn.记录不存在;

            // 查询角色
            var roles = roleService.GetRoles(user.Id);

            return APIReturn.成功.SetData("detail", new
            {
                user.Name,
                user.LoginName,
                user.ImgFace,
                user.State,
                user.Phone,
                user.CreateTime
            }, "roles", roles.Select(f => new
            {
                f.Id,
                f.Name
            }));
        }

        /// <summary>
        ///  列表
        /// </summary>
        /// <remarks>
        /// <code>
        /// data:[{
        ///          Id:
        ///         code:
        ///         dept:
        ///         name:
        ///         title:
        ///         state:
        /// }]
        /// </code>
        /// </remarks>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("list")]
        public IActionResult List([FromBody] UserListViewModel model)
        {
            var result = userService.List(model.Name, model.State, model.PageIndex, model.PageSize);
            return APIReturn.成功.SetData("list", result.Select(f => new
            {
                f.Name,
                f.LoginName,
                f.ImgFace,
                f.State,
                f.Phone,
                f.CreateTime
            }));
        }
    }
}
