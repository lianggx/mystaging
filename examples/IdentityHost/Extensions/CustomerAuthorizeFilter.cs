using IdentityHost.Model;
using IdentityHost.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocalResource = IdentityHost.Properties.Resource;

namespace IdentityHost.Extensions
{
    public class CustomerAuthorizeFilter : AuthorizeFilter
    {
        protected const string SignInKey = "SignInKey_";
        private static readonly AuthorizationPolicy _policy_ = new AuthorizationPolicy(new[] { new DenyAnonymousAuthorizationRequirement() }, new string[] { });
        private M_Resource resource;
        private int userId;
        private readonly RoleService roleService;
        private readonly ResourceService resourceService;
        private readonly AccessLogService accessLogService;
        private readonly ConnectionMultiplexer redisClient;

        public CustomerAuthorizeFilter(IEnumerable<IManagerService> managerServices, ConnectionMultiplexer multiplexer) : base(_policy_)
        {
            roleService = GetService<RoleService>(managerServices);
            resourceService = GetService<ResourceService>(managerServices);
            accessLogService = GetService<AccessLogService>(managerServices);
            redisClient = multiplexer;
        }

        public T GetService<T>(IEnumerable<IManagerService> managerServices) => (T)managerServices.FirstOrDefault(f => f.ServiceName == typeof(T).Name);

        public override async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var env = (IWebHostEnvironment)context.HttpContext.RequestServices.GetService(typeof(IWebHostEnvironment));
            if (env.EnvironmentName == Environments.Development) // 测试环境不予以校验
            {
                return;
            }
            else
            {
                var ctx = context.HttpContext;
                var path = ctx.Request.Path.Value;
                var result = await CheckResource(ctx, path);
                if (result.Code != 0)
                {
                    context.Result = result;
                }
                AddAccessLog(result, context.HttpContext);
            }
        }

        private async Task<APIReturn> CheckResource(HttpContext ctx, string path)
        {
            resource = resourceService.Detail(path);
            if (resource == null)
                return APIReturn.失败.SetMessage(LocalResource.NotFound);

            if (resource.Authorize)
            {
                var token = ctx.Request.Headers["token"];
                if (string.IsNullOrEmpty(token))
                    return APIReturn.用户_未登录;

                int.TryParse(await redisClient.GetDatabase().StringGetAsync(SignInKey + token), out userId);
                var roleId = roleService.GetRoles(userId).Select(f => f.Id).ToList();
                if (roleId.Count == 0)
                    return APIReturn.没有访问权限;
                else
                {
                    var access = roleService.ValidatorRole(resource.Id, roleId.ToArray());
                    if (!access)
                        return APIReturn.没有访问权限;
                }
            }

            return APIReturn.成功;
        }

        private void AddAccessLog(APIReturn apiReturn, HttpContext context)
        {
            var resourceName = resource == null ? context.Request.Path.Value : resource.Content;
            _ = accessLogService.Add(new M_Accesslog
            {
                Resource = resourceName,
                Code = apiReturn.Code,
                ReqContent = GetRequestBody(context),
                UserId = userId,
                Remark = resource?.Name,
                ResourceId = resource?.Id,
                ResContent = null,
                IP = GetClientIP(context),
                CreateTime = DateTime.Now,
            });
        }

        private string GetRequestBody(HttpContext context)
        {
            string body = string.Empty;
            if (context.Items?.ContainsKey("this_body") == true)
                try { body = context.Items["this_body"].ToString(); } catch { }
            return body;
        }

        public string GetClientIP(HttpContext context)
        {
            return context.Request.Headers["X-Real-IP"].FirstOrDefault() ?? context.Request.HttpContext.Connection.RemoteIpAddress?.ToString();
        }

    }
}