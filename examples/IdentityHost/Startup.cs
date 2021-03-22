using IdentityHost.Converters;
using IdentityHost.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace IdentityHost
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Env { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMyStagingDbContenxt(Configuration)
                         .AddIManagerService()
                         .AddSingletonSerializerOptions()
                         .AddCustomCors()
                         .AddCustomSwagger(Env)
                         .AddStackExchangeRedis(Configuration);

            services.AddControllers(p =>
            {
                p.Filters.Add<CustomerAuthorizeFilter>();
                p.Filters.Add<CustomerExceptionFilter>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting()
                  .UseAuthorization()
                  .UseCustomizedSwagger()
                  .UseCors()
                  .UseMiddleware<RequestMiddleware>()
                 .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
        }
    }
}
