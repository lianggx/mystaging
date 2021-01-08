using IdentityHost.Model;
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
    [Route("admin/role"), ApiExplorerSettings(GroupName = "管理员")]
    public class RoleController : BaseController
    {
        private readonly RoleService roleService;
        private readonly ResourceService resourceService;
        public RoleController(IConfiguration cfg, ILogger<RoleController> logger, IEnumerable<IManagerService> managerServices, ConnectionMultiplexer multiplexer) : base(cfg, logger, managerServices, multiplexer)
        {
            roleService = GetService<RoleService>();
            resourceService = GetService<ResourceService>();
        }

        /// <summary>
        ///  添加角色
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
        public IActionResult Add([FromBody] AddRoleViewModel model)
        {
            var role = roleService.Add(new M_Role { Name = model.Name });

            return APIReturn.成功.SetData("id", role.Id);
        }

        /// <summary>
        ///  编辑角色
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
        [HttpPost("edit")]
        public IActionResult Edit([FromBody] EditRoleViewModel model)
        {
            var role = roleService.Detail(model.Id);
            if (role == null)
                return APIReturn.记录不存在;

            role = roleService.EditName(role.Id, model.Name);

            return APIReturn.成功.SetData("id", role.Id);
        }

        /// <summary>
        ///  删除角色
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("delete")]
        public IActionResult Delete([FromBody] IdViewModel model)
        {
            var role = roleService.Detail(model.Id);
            if (role == null)
                return APIReturn.记录不存在;

            var success = roleService.Delete(role.Id);

            return success ? APIReturn.成功 : APIReturn.失败;
        }

        /// <summary>
        ///  角色详情
        /// </summary>
        /// <remarks>
        /// <code>
        /// data:[
        /// role:{
        ///            id:
        ///            name:  名称
        ///     },
        ///     resources:[{
        ///         id:
        ///         parent_id:
        ///         name:
        ///         resource:
        /// }]]
        /// </code>
        /// </remarks>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("detail")]
        public IActionResult Detail([FromBody] IdViewModel model)
        {
            var role = roleService.Detail(model.Id);
            if (role == null)
                return APIReturn.记录不存在;

            var resources = resourceService.ResourceByRole(role.Id);

            return APIReturn.成功.SetData("role", new
            {
                role.Id,
                role.Name
            }, "resources", resources.Select(f => new
            {
                f.Id,
                f.ParentId,
                f.Name,
                f.Content
            }));
        }

        /// <summary>
        ///  角色列表
        /// </summary>
        /// <remarks>
        /// <code>
        /// data:[{
        ///            id:
        ///            name:  名称
        ///     }]
        /// </code>
        /// </remarks>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("list")]
        public IActionResult List([FromBody] PageViewModel model)
        {
            var list = roleService.List(model.PageIndex, model.PageSize);

            return APIReturn.成功.SetData("list", list.Select(f => new
            {
                f.Id,
                f.Name
            }));
        }
    }
}
