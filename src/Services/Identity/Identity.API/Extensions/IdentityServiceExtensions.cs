using Identity.Domain.AggregatesModel.UserAggregate;
using Identity.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EmailHelper.Models;
using EmailHelper.Services;
using Common.Interfaces;
using Common.Implements;
using Identity.Domain.Business.Interfaces.Services;
using Identity.Infrastructure.Implements.Business.Services;
using Identity.Domain.Interfaces.Infrastructure;
using Identity.Infrastructure.Implements.Infrastructure;

namespace Identity.API.Extensions
{
    public static class IdentityServiceExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services,
          IConfiguration config)
        {
            services.AddDbContext<AppIdentityDbContext>(opt => opt.UseLazyLoadingProxies().UseSqlServer(config.GetConnectionString("IdentityConnection")));
            services.AddSingleton<ISystemClock, SystemClock>();

            services.AddIdentityCore<User>(_ =>
            {
                // add identity options here
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddSignInManager<SignInManager<User>>()
            .AddRoleManager<RoleManager<IdentityRole>>();

            var appSettingsConfig = config.GetSection("AppSettings");
            var options = new EmailOptions
            {
                Environment = appSettingsConfig.GetValue<string>("Environment"),
                SmtpServer = appSettingsConfig.GetValue<string>("SmtpServer"),
                SmtpPort = appSettingsConfig.GetValue<int>("SmtpPort"),
                SmtpUser = appSettingsConfig.GetValue<string>("SmtpUser"),
                SmtpPassword = appSettingsConfig.GetValue<string>("SmtpPass"),
                MailFrom = appSettingsConfig.GetValue<string>("EmailFrom"),
            };
            services.AddScoped<IEmailService>(x =>
                new EmailMockupService(x.GetRequiredService<ILogger<EmailMockupService>>(), options)
            );
            services.AddScoped<IApiService, ApiService>();

            // configure DI for application services
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAccountService, AccountService>();
            return services;
        }
    }
}