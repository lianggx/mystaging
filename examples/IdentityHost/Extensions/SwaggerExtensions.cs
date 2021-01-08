using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.IO;

namespace IdentityHost.Extensions
{
    public static class SwashbuckleSwaggerExtensions
    {
        public static readonly string[] docs = new[] { "首页", "个人中心", "管理员" };

        public static IServiceCollection AddCustomSwagger(this IServiceCollection services, IWebHostEnvironment env)
        {
            services.AddSwaggerGen(c =>
            {
                foreach (var doc in docs)
                    c.SwaggerDoc(doc, new OpenApiInfo
                    {
                        Version = doc,
                        Title = doc
                    });
                c.EnableAnnotations();
                c.IgnoreObsoleteProperties(); // Obsolete接口划删除线
                c.CustomSchemaIds(a => a.FullName);
                var basePath = Path.GetDirectoryName(AppContext.BaseDirectory);
                var xmlPath = Path.Combine(basePath, $"{env.ApplicationName}.xml");
                c.IncludeXmlComments(xmlPath); // 加载生成XML文件
            });

            return services;
        }

        public static IApplicationBuilder UseCustomizedSwagger(this IApplicationBuilder app)
        {
            app.UseSwaggerUI(c =>
            {
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None); // 默认折叠
                foreach (var doc in docs)
                    c.SwaggerEndpoint($"/swagger/{doc}/swagger.json", doc);
                //c.RoutePrefix = string.Empty;
            }).UseSwagger();
            return app;
        }
    }
}
