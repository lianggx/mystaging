using IdentityHost.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyStaging.Metadata;
using StackExchange.Redis;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace IdentityHost.Extensions
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddSingletonSerializerOptions(this IServiceCollection services)
        {
            services.AddSingleton((s) =>
            {
                var encoderSettings = new TextEncoderSettings();
                encoderSettings.AllowRanges(UnicodeRanges.CjkUnifiedIdeographs, UnicodeRanges.BasicLatin);
                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(encoderSettings)
                    //Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                return options;
            });
            return services;
        }

        public static IServiceCollection AddIManagerService(this IServiceCollection services)
        {
            services.AddScoped<IManagerService, AccessLogService>()
                         .AddScoped<IManagerService, ResourceService>()
                         .AddScoped<IManagerService, RoleService>()
                         .AddScoped<IManagerService, UserService>();

            return services;
        }

        public static IServiceCollection AddCustomCors(this IServiceCollection services)
        {
            services.AddCors(options => options.AddPolicy(
                "all",
                builder => builder
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin()
            ));

            return services;
        }

        public static IServiceCollection AddMyStagingDbContenxt(this IServiceCollection services, IConfiguration configuration)
        {
            var options = new StagingOptions("MySql", configuration["ConnectionStrings:MySql"]);
            services.AddScoped(fct => new IdentityHostDbContext(options));
            return services;
        }

        public static IServiceCollection AddStackExchangeRedis(this IServiceCollection services, IConfiguration configuration)
        {
            var multiplexer = ConnectionMultiplexer.Connect(configuration["ConnectionStrings:Redis"]);
            services.AddSingleton(multiplexer);
            return services;
        }
    }
}
