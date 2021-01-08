using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IdentityHost.Extensions
{
    public class RequestMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestMiddleware> _logger;
        public RequestMiddleware(RequestDelegate next, ILogger<RequestMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Request.EnableBuffering();
            if (context.Request.ContentType?.Contains("multipart/form-data; boundary=") == true)
            {
                var fileName = context.Request.Form?.Files[0]?.FileName;
                context.Items.Add("this_body", JsonSerializer.Serialize(new { file = fileName ?? "file" }));
            }
            else
            {
                var str = await GetContextRequestBody(context.Request.BodyReader);
                if (context.Items?.ContainsKey("this_body") != true && !string.IsNullOrEmpty(str))
                    context.Items.Add("this_body", str);
            }
            try
            {
                context.Request.Body.Position = 0;
                await _next.Invoke(context);
            }
            catch (InvalidOperationException ioe)
            {
                _logger.LogError("中间件下一步命令报错");
                throw ioe;
            }
        }

        private async Task<string> GetContextRequestBody(PipeReader reader)
        {
            ReadResult readResult;
            try
            {
                readResult = await reader.ReadAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("readasync error");
                throw ex;
            }

            var buffer = readResult.Buffer;
            if (buffer.Length <= 0)
            {
                reader.AdvanceTo(buffer.Start);
                return "{}";
            }
            var resturnStr = GetBufferString(buffer);
            try
            {
                reader.AdvanceTo(buffer.Start);
            }
            catch (Exception ex)
            {
                ex.Data["segment"] = readResult.Buffer.IsSingleSegment;
                ex.Data["start"] = buffer.Start.GetInteger();
                ex.Data["end"] = buffer.End.GetInteger();
                _logger.LogError("advanceTo error, {0}", JsonSerializer.Serialize(ex.Data));
                throw ex;
            }
            return resturnStr;
        }
        private static string GetBufferString(in ReadOnlySequence<byte> readOnlySequence)
        {
            ReadOnlySpan<byte> span = readOnlySequence.IsSingleSegment ? readOnlySequence.FirstSpan : readOnlySequence.ToArray().AsSpan();
            return Encoding.UTF8.GetString(span);
        }
    }
}
