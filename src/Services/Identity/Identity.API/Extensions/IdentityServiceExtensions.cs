using Identity.Domain.AggregatesModel.UserAggregate;
using Identity.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Common.Interfaces;
using Common.Implements;
using Identity.Domain.Business.Interfaces.Services;
using Identity.Infrastructure.Implements.Business.Services;
using Identity.Domain.Interfaces.Infrastructure;
using Identity.Infrastructure.Implements.Infrastructure;
using MassTransit;
using System.Security.Authentication;
using Common.Interfaces.Messaging;
using Common.Implements.Messaging;

namespace Identity.API.Extensions
{
    public static class IdentityServiceExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services,
          IConfiguration config)
        {
            services.AddDbContext<AppIdentityDbContext>(opt => opt.UseLazyLoadingProxies().UseSqlServer(config.GetConnectionString("IdentityConnection")));
            services.AddSingleton<ISystemClock, SystemClock>();
            services.AddDataProtection();

            services.AddIdentityCore<User>(_ =>
            {
                // add identity options here
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddSignInManager<SignInManager<User>>()
            .AddRoleManager<RoleManager<IdentityRole>>()
            .AddDefaultTokenProviders();

            services.AddScoped<IApiService, ApiService>();

            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(config.GetSection("RabitMQSettings").GetValue<string>("Hostname"), 5671, config.GetSection("RabitMQSettings").GetValue<string>("VHost"), h =>
                    {
                        h.Username(config.GetSection("RabitMQSettings").GetValue<string>("Username"));
                        h.Password(config.GetSection("RabitMQSettings").GetValue<string>("Password"));
                        h.UseSsl(s =>
                        {
                            s.Protocol = SslProtocols.Tls12;
                        });
                    });
                });
            });

            // Identity registers publishers for MassTransit
            services.AddScoped<ISendMailPublisher, SendMailPublisher>();

            // configure DI for application services
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAccountService, AccountService>();
            return services;
        }
    }
}