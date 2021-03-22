using IdentityHost.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IdentityHost
{
    public partial class APIResult : ContentResult
    {
        [JsonPropertyName("code")] public int Code { get; set; }
        [JsonPropertyName("message")] public string Message { get; set; }
        [JsonPropertyName("data")] public Hashtable Data { get; set; } = new Hashtable();

        public APIResult() { }

        public APIResult(int code, string message, params object[] data)
        {
            Code = code;
            Message = message;
            AppendData(data);
        }

        public APIResult SetCode(int code) { Code = code; return this; }

        public APIResult SetMessage(string message) { Message = message; return this; }

        public APIResult SetData(params object[] value) { Data.Clear(); return AppendData(value); }

        public APIResult AppendData(params object[] value)
        {
            if (value == null || value.Length < 2 || value[0] == null) return this;
            for (int a = 0; a < value.Length; a += 2)
            {
                if (value[a] == null) continue;
                Data[value[a]] = a + 1 < value.Length ? value[a + 1] : null;
            }
            return this;
        }

        private void WriteResult()
        {
            var data = new Hashtable {
                { "code",Code},
                {"message",Message },
                {"data",Data }
            };

            ContentType = "application/json;charset=utf-8;";
            //Content = JsonSerializer.Serialize(data, JsonSerializerExtension.JsonOptions);
            Content = JsonSerializer.Serialize(data);
        }

        public override void ExecuteResult(ActionContext context)
        {
            WriteResult();
            base.ExecuteResult(context);
        }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            WriteResult();
            await base.ExecuteResultAsync(context);
        }

        public const int OK = 0;

        /// <summary>
        /// 成功 0
        /// </summary>
        public static APIResult 成功 { get { return new APIResult(0, "成功"); } }
        /// <summary>
        ///  失败 99
        /// </summary>
        public static APIResult 失败 { get { return new APIResult(99, "失败"); } }
        /// <summary>
        /// 记录不存在 98
        /// </summary>
        public static APIResult 记录不存在 { get { return new APIResult(98, "记录不存在"); } }
        /// <summary>
        /// 参数格式不正确 97
        /// </summary>
        public static APIResult 参数格式不正确 { get { return new APIResult(97, "参数格式不正确"); } }
        /// <summary>
        ///  没有访问权限 96
        /// </summary>
        public static APIResult 没有访问权限 { get { return new APIResult(96, "没有访问权限"); } }
        /// <summary>
        ///  系统内置_内部异常 5001000
        /// </summary>
        public static APIResult 系统内置_内部异常 => new APIResult(5001000, "抱歉，访问出现错误了");
        /// <summary>
        ///  用户_未登录 1001005
        /// </summary>
        public static APIResult 用户_未登录 => new APIResult(1001005, "未登录");
    }
}