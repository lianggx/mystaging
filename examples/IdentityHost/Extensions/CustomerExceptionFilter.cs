using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;

namespace IdentityHost.Extensions
{
    public class CustomerExceptionFilter : ExceptionFilterAttribute
    {
        private readonly ILogger _logger = null;
        private readonly IWebHostEnvironment _env = null;


        public CustomerExceptionFilter(ILogger<CustomerExceptionFilter> logger,IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is OperationCanceledException)
            {
                _logger.LogInformation("Request was cancelled");
                context.ExceptionHandled = true;
                return;
            }
            if (context.Exception is APIReturnException ar)
            {
                context.Result = new APIResult(ar.HResult, ar.Message);
                return;
            }
            string exmessage = string.Empty;
            void act(Exception ex)
            {
                exmessage += string.Format("{0} {1} {2} {3}",
                    ex,
                   ex.StackTrace,
                    ex.InnerException,
                    ex.Message,
                    ex.Data == null ? null : JsonSerializer.Serialize(ex.Data)
                    );
                if (ex.InnerException != null)
                {
                    act(ex.InnerException);
                }
            }

            act(context.Exception);
            _logger.LogError(exmessage);

            if (_env.IsDevelopment() || _env.IsStaging())
            {
                context.Result = APIResult.失败.SetMessage(context.Exception.Message);
            }
            else
            {
                context.Result = APIResult.系统内置_内部异常;
            }
            context.ExceptionHandled = true;
        }

    }

    public partial class APIReturnException : Exception
    {
        public APIReturnException(int code, string message) : base(message)
        {
            HResult = code;
        }
        public APIReturnException(APIResult ar) : base(ar.Message)
        {
            HResult = ar.Code;

        }
        public static implicit operator APIReturnException(APIResult value)
        {
            return new APIReturnException(value);
        }
        public static implicit operator APIResult(APIReturnException value)
        {
            return new APIResult(value.HResult, value.Message);
        }
    }
}