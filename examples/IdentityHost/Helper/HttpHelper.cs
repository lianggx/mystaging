using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IdentityHost.Helpers
{
    public class HttpHelper
    {
        private readonly HttpClient httpClient;
        private readonly IHttpClientFactory _clientFactory;

        /// <summary>
        ///  
        /// </summary>
        /// <returns></returns>
        public HttpHelper(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            httpClient = clientFactory.CreateClient("helper");
            httpClient.DefaultRequestHeaders.Clear();
        }

        public HttpClient Create(int expireSeconds, string name = "getHelper")
        {
            var client = _clientFactory.CreateClient(name);
            client.Timeout = TimeSpan.FromSeconds(expireSeconds);
            client.DefaultRequestHeaders.Clear();
            return client;
        }

        /// <summary>
        /// 
        /// </summary>
        public enum HttpMethodEnum
        {
            /// <summary>
            /// 
            /// </summary>
            POST,
            /// <summary>
            /// 
            /// </summary>
            GET
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="httpMethod"></param>
        /// <param name="data"></param>
        /// <param name="headers"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public async Task<string> HttpRequest(string url, HttpMethodEnum httpMethod, string data = null, Dictionary<string, string> headers = null, string contentType = "application/json")
        {
            string result = string.Empty;
            try
            {
                HttpResponseMessage response = null;
                if (httpMethod == HttpMethodEnum.POST)
                {
                    HttpContent content = new StringContent(data ?? "", Encoding.UTF8, contentType);
                    if (headers != null)
                    {
                        foreach (var h in headers)
                        {
                            content.Headers.Add(h.Key, h.Value);
                        }
                    }
                    response = await httpClient.PostAsync(url, content);
                }
                else if (httpMethod == HttpMethodEnum.GET)
                {
                    response = await httpClient.GetAsync(url);
                }

                if (response != null && response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    result = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception e)
            {
                var ex = e;
                if (e.InnerException != null)
                    if (e.InnerException.InnerException != null)
                        ex = e.InnerException.InnerException;
                    else
                        ex = e.InnerException;
                throw ex;
            }
            return result;
        }

        public async Task<string> HttpRequest(HttpClient client, string url, HttpMethodEnum httpMethod, string data = null, Dictionary<string, string> headers = null, string contentType = "application/json")
        {
            client ??= httpClient;
            string result = string.Empty;
            try
            {
                HttpResponseMessage response = null;
                if (httpMethod == HttpMethodEnum.POST)
                {
                    HttpContent content = new StringContent(data ?? "", Encoding.UTF8, contentType);
                    if (headers != null)
                    {
                        foreach (var h in headers)
                        {
                            content.Headers.Add(h.Key, h.Value);
                        }
                    }
                    response = await client.PostAsync(url, content);
                }
                else if (httpMethod == HttpMethodEnum.GET)
                {
                    response = await client.GetAsync(url);
                }

                if (response != null && response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    result = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception e)
            {
                var ex = e;
                if (e.InnerException != null)
                    if (e.InnerException.InnerException != null)
                        ex = e.InnerException.InnerException;
                    else
                        ex = e.InnerException;
                throw ex;
            }
            return result;
        }

        /// <summary>
        ///  使用 Get 方式调用远程服务
        /// </summary>
        /// <param name="url"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task<string> GetFrom(string url, int? timeout = null)
        {
            string return_str;
            try
            {
                HttpClient client = timeout.HasValue ? Create(timeout.Value) : null;
                return_str = await HttpRequest(client, url, HttpMethodEnum.GET, null);
            }
            catch
            {
                throw;
            }
            return return_str;
        }
    }
}