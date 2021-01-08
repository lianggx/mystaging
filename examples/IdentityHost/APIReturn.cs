using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IdentityHost
{
    public partial class APIReturn : ContentResult
    {
        [JsonPropertyName("code")] public int Code { get; protected set; }

        [JsonPropertyName("message")] public string Message { get; protected set; }

        [JsonPropertyName("data")] public Hashtable Data { get; protected set; } = new Hashtable();

        [JsonPropertyName("success")] public bool Success { get { return this.Code == 0; } }

        public APIReturn() { }

        public APIReturn(int code) { this.SetCode(code); }

        public APIReturn(string message) { this.SetMessage(message); }

        public APIReturn(int code, string message, params object[] data) { this.SetCode(code).SetMessage(message).AppendData(data); }

        public APIReturn SetCode(int value) { this.Code = value; return this; }

        public APIReturn SetCode(Enum value) { this.Code = Convert.ToInt32(value); this.Message = value.ToString(); return this; }

        public APIReturn SetMessage(string value) { this.Message = value; return this; }

        public APIReturn SetData(params object[] value)
        {
            this.Data.Clear();
            return this.AppendData(value);
        }

        public APIReturn AppendData(params object[] value)
        {
            if (value == null || value.Length < 2 || value[0] == null) return this;
            for (int a = 0; a < value.Length; a += 2)
            {
                if (value[a] == null) continue;
                this.Data[value[a]] = a + 1 < value.Length ? value[a + 1] : null;
            }
            return this;
        }

        #region form 表单 target=iframe 提交回调处理
        private void Jsonp(ActionContext context)
        {
            //var settings = new JsonSerializerOptions();
            //settings.
            //settings.ContractResolver = new LowercaseContractResolver();
            //settings.Converters.Add(new StringEnumConverter());
            //settings.Converters.Add(new BooleanConverter());
            //settings.Converters.Add(new DateTimeConverter());

            this.ContentType = "text/json;charset=utf-8;";
            this.Content = JsonSerializer.Serialize(this);
        }

        public override void ExecuteResult(ActionContext context)
        {
            Jsonp(context);
            base.ExecuteResult(context);
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            Jsonp(context);
            return base.ExecuteResultAsync(context);
        }
        #endregion

        public const int OK = 0;

        /// <summary>
        /// 成功 0
        /// </summary>
        public static APIReturn 成功 { get { return new APIReturn(0, "成功"); } }
        /// <summary>
        ///  失败 99
        /// </summary>
        public static APIReturn 失败 { get { return new APIReturn(99, "失败"); } }
        /// <summary>
        /// 记录不存在 98
        /// </summary>
        public static APIReturn 记录不存在 { get { return new APIReturn(98, "记录不存在"); } }
        /// <summary>
        /// 参数格式不正确 97
        /// </summary>
        public static APIReturn 参数格式不正确 { get { return new APIReturn(97, "参数格式不正确"); } }
        /// <summary>
        ///  没有访问权限 96
        /// </summary>
        public static APIReturn 没有访问权限 { get { return new APIReturn(96, "没有访问权限"); } }
        /// <summary>
        ///  系统内置_内部异常 5001000
        /// </summary>
        public static APIReturn 系统内置_内部异常 => new APIReturn(5001000, "抱歉，访问出现错误了");
        /// <summary>
        ///  用户_未登录 1001005
        /// </summary>
        public static APIReturn 用户_未登录 => new APIReturn(1001005, "未登录");
    }
}