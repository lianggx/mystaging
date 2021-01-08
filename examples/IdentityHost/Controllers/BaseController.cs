using IdentityHost.Model;
using IdentityHost.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace IdentityHost.Controllers
{
    [EnableCors("all")]
    public class BaseController : Controller
    {
        protected const string SignInKey = "SignInKey_";
        protected readonly IEnumerable<IManagerService> managerServices;
        protected readonly ConnectionMultiplexer redisClient;

        public IConfiguration Cfg { get; }
        public ILogger Log { get; }

        public BaseController(IConfiguration cfg, ILogger logger, IEnumerable<IManagerService> managerServices, ConnectionMultiplexer multiplexer)
        {
            this.Cfg = cfg;
            this.Log = logger;
            this.managerServices = managerServices;
            redisClient = multiplexer;
        }

        [FromHeader(Name = "token")]
        public string Token { get; set; }

        private M_User loginUser = null;

        public M_User LoginUser
        {
            get
            {
                if (loginUser != null)
                    return loginUser;

                if (!string.IsNullOrEmpty(Token))
                {
                    int.TryParse(redisClient.GetDatabase().StringGet(SignInKey + Token), out int userId);
                    if (userId > 0)
                    {
                        loginUser = GetService<UserService>().Detail(userId);
                    }

                    return loginUser;
                }
                else
                {
                    throw new Exception("未登录");
                }
            }
        }

        public string IP => this.Request.Headers["X-Real-IP"].FirstOrDefault() ?? this.Request.HttpContext.Connection.RemoteIpAddress?.ToString();

        public T GetService<T>() => (T)managerServices.FirstOrDefault(f => f.ServiceName == typeof(T).Name);

        private M_Resource resource;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var path = context.HttpContext.Request.Path.Value;
            resource = GetService<ResourceService>().Detail(path);
            if (context.ModelState.IsValid == false)
            {
                foreach (var value in context.ModelState.Values)
                    if (value.Errors.Any())
                    {
                        context.Result = APIReturn.参数格式不正确.SetMessage($"参数格式不正确：{value.Errors.First().ErrorMessage}");
                        return;
                    }
            }
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result is APIReturn apiReturn)
            {
                var response = JsonSerializer.Serialize(apiReturn);
                _ = GetService<AccessLogService>().Add(
                    new M_Accesslog
                    {
                        Resource = resource?.Content,
                        Code = apiReturn.Code,
                        ReqContent = GetRequestBody(context.HttpContext),
                        UserId = loginUser?.Id,
                        Remark = resource?.Name,
                        ResourceId = resource?.Id,
                        ResContent = response,
                        IP = IP,
                        CreateTime = DateTime.Now
                    });
            }
        }

        private string GetRequestBody(HttpContext context)
        {
            string body = string.Empty;
            if (context.Items?.ContainsKey("this_body") == true)
                try { body = context.Items["this_body"].ToString(); } catch { }
            return body;
        }
    }
}