using System.Security.Authentication;
using Amazon;
using Amazon.S3;
using Common.Implements;
using Common.Implements.Messaging;
using Common.Interfaces;
using Common.Interfaces.Messaging;
using Common.Models.Redis;
using HangFireServer.Messaging.Publishers;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Portal.API.Hubs;
using Portal.Domain.Interfaces.External;
using Portal.Domain.Interfaces.Infrastructure;
using Portal.Domain.Interfaces.Messaging;
using Portal.Infrastructure;
using Portal.Infrastructure.Implements.External;
using Portal.Infrastructure.Implements.Services;
using Portal.Infrastructure.SeedWork;

namespace Portal.API.Extensions;
public static class PortalServiceExtensions
{
    public static IServiceCollection AddPortalServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<ApplicationDbContext>(opt => opt.UseLazyLoadingProxies().UseSqlServer(config.GetConnectionString("PortalConnection")));
        services.Configure<AppSettings>(config.GetSection("AppSettings"));
        services.Configure<FirebaseSettings>(config.GetSection("FirebaseSettings"));

        services.AddScoped<IAmazonS3>(x => new AmazonS3Client(config["AWS:AccessKey"], config["AWS:SecretKey"], RegionEndpoint.USEast1));

        services.AddStackExchangeRedisCache(options =>
         {
             options.Configuration = config.GetConnectionString("RedisConnection");
             options.InstanceName = "Portal";
         });
        services.AddDistributedMemoryCache();

        services.AddScoped<IRedisService>(x => new RedisService(x.GetRequiredService<IDistributedCache>(), new RedisOptions
        {
            ConnectionString = config.GetConnectionString("RedisConnection") ?? string.Empty,
            Host = config.GetSection("RedisSettings").GetValue<string>("Host") ?? string.Empty,
            Port = config.GetSection("RedisSettings").GetValue<string>("Port") ?? string.Empty,
            InstanceName = "Portal"
        }));

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

        // Portal registers publishers for MassTransit
        services.AddScoped<ISendMailPublisher, SendMailPublisher>();
        services.AddScoped<IServiceLogPublisher, ServiceLogPublisher>();
        services.AddScoped<ISyncResetExpiredRolePublisher, SyncResetExpiredRolePublisher>();

        // Inject Services
        services.AddScoped<IApiService, ApiService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAmazonS3Service, AmazonS3Service>();
        services.AddSingleton<IFirebaseCloudMessageService, FirebaseCloudMessageService>();
        return services;
    }
}
