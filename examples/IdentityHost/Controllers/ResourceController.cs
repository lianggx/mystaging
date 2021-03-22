using IdentityHost.Model;
using IdentityHost.Services;
using IdentityHost.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using LocalResource = IdentityHost.Properties.Resource;

namespace IdentityHost.Controllers
{
    [Route("admin/resource"), ApiExplorerSettings(GroupName = "管理员")]
    public class ResourceController : BaseController
    {
        private readonly ResourceService resourceService;
        public ResourceController(IConfiguration cfg, ILogger<ResourceController> logger, IEnumerable<IManagerService> managerServices, ConnectionMultiplexer multiplexer) : base(cfg, logger, managerServices, multiplexer)
        {
            this.resourceService = GetService<ResourceService>();
        }

        /// <summary>
        ///  添加资源
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
        public IActionResult Add([FromBody] AddResourceViewModel model)
        {

            var resource = resourceService.Detail(model.Content);
            if (resource != null)
                return APIResult.失败.SetMessage(string.Format(LocalResource.AlreadyExists, model.Content));

            var check = CheckParent(model.ParentId);
            if (check.Code != APIResult.OK)
                return check;

            resource = resourceService.Add(
                new M_Resource
                {
                    Authorize = model.Authorize,
                    Name = model.Name,
                    Content = model.Content,
                    Type = model.Type,
                    ParentId = model.ParentId,
                    Sort = model.Sort
                });

            return APIResult.成功.SetData("id", resource.Id);
        }

        /// <summary>
        ///  编辑资源
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
        public IActionResult Edit([FromBody] EditResourceViewModel model)
        {
            var resource = resourceService.Detail(model.Id);
            if (resource == null)
                return APIResult.记录不存在;

            var check = CheckParent(model.ParentId);
            if (check.Code != APIResult.OK)
                return check;

            resource = resourceService.Edit(model.Id, model.Content, model.Name, model.Authorize, model.Type, model.ParentId, model.Sort);

            return resource != null ? APIResult.成功 : APIResult.失败;
        }

        private APIResult CheckParent(int? parentId)
        {
            if (parentId.HasValue)
            {
                var parent = resourceService.Detail(parentId.Value);
                if (parent == null)
                {
                    return APIResult.失败.SetMessage(LocalResource.ParentNotFound);
                }
                else if (parentId.HasValue)
                {
                    return APIResult.失败.SetMessage(LocalResource.NotSupport);
                }
            }

            return APIResult.成功;
        }

        /// <summary>
        ///  删除资源
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("delete")]
        public IActionResult Delete([FromBody] IdViewModel model)
        {
            var resource = resourceService.Detail(model.Id);
            if (resource == null)
                return APIResult.记录不存在;

            var result = resourceService.Delete(model.Id);

            return result ? APIResult.成功 : APIResult.失败;
        }

        /// <summary>
        ///  资源列表
        /// </summary>
        /// <remarks>
        /// <code>
        /// data:[{
        ///            id:
        ///            parent_id
        ///            name:  名称
        ///            type: 资源类型
        ///            resource: 资源内容
        ///            sort:排序号
        ///            authorize: 是否需要授权登录
        ///            children:资源列表
        ///     }]
        /// </code>
        /// </remarks>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("list")]
        public IActionResult List([FromBody] ResourceListViewModel model)
        {
            var list = resourceService.List(model.Type);

            return APIResult.成功.SetData("list", list.Select(f => new
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

        /// <summary>
        ///  顶级资源列表
        /// </summary>
        /// <remarks>
        /// <code>
        /// data:[{
        ///            id:
        ///            name:  名称
        ///            type: 资源类型
        ///            resource: 资源内容
        ///            sort:排序号
        ///            authorize: 是否需要授权登录
        ///     }]
        /// </code>
        /// </remarks>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("root")]
        public IActionResult Root([FromBody] ResourceRootViewModel model)
        {
            var list = resourceService.Root(model.Type);

            return APIResult.成功.SetData("list", list.Select(f => new
            {
                f.Id,
                f.Name,
                f.Type,
                f.Content,
                f.Sort,
                f.Authorize
            }));
        }
    }
}
