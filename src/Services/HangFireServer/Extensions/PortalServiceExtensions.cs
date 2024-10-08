﻿using System.Reflection;
using System.Security.Authentication;
using Amazon;
using Amazon.S3;
using Common.Implements;
using Common.Implements.Messaging;
using Common.Interfaces;
using Common.Interfaces.Messaging;
using Common.Models.Redis;
using EmailHelper.Models;
using EmailHelper.Services;
using HangFireServer.Messaging.Publishers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Portal.Domain.Interfaces.External;
using Portal.Domain.Interfaces.Infrastructure;
using Portal.Domain.Interfaces.Messaging;
using Portal.Domain.SeedWork;
using Portal.Infrastructure;
using Portal.Infrastructure.Helpers;
using Portal.Infrastructure.Implements.External;
using Portal.Infrastructure.Implements.Services;
using Portal.Infrastructure.SeedWork;
using Serilog;
using StackExchange.Redis;

namespace HangFireServer.Extensions;
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

        if (!string.IsNullOrEmpty(config.GetConnectionString("RedisBackgroundConnection")))
        {
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(config.GetConnectionString("RedisBackgroundConnection") ?? string.Empty));
            services.AddScoped<IRedisBackgroundService, RedisBackgroundService>();
        }
        else
        {
            services.AddScoped<IRedisBackgroundService, RedisBackgroundBlankService>();
        }

        services.AddMassTransit(x =>
        {
            var entryAssembly = Assembly.GetExecutingAssembly();
            x.AddConsumers(entryAssembly);

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

                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<ISimpleTokenService>(x => new SimpleTokenService(config.GetSection("AppSettings").GetValue<string>("Secret")!));

        // Portal registers publishers for MassTransit
        services.AddScoped<ISendMailPublisher, SendMailPublisher>();
        services.AddScoped<IServiceLogPublisher, ServiceLogPublisher>();
        services.AddScoped<ISyncResetExpiredRolePublisher, SyncResetExpiredRolePublisher>();

        // Inject Services
        services.AddScoped<IApiService, ApiService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAmazonS3Service, AmazonS3Service>();
        services.AddScoped<IFirebaseCloudMessageService, FirebaseCloudMessageService>();

        // Hangfire use service differnce than Portal
        #region Email Service
        var appSettingsConfig = config.GetSection("SmtpSettings");
        var options = new EmailOptions
        {
            Environment = appSettingsConfig.GetValue<string>("Environment"),
            SmtpServer = appSettingsConfig.GetValue<string>("Host"),
            SmtpPort = appSettingsConfig.GetValue<int>("Port"),
            SmtpUser = appSettingsConfig.GetValue<string>("Username"),
            SmtpPassword = appSettingsConfig.GetValue<string>("Password"),
            MailFrom = appSettingsConfig.GetValue<string>("SenderEmail"),
            SenderName = appSettingsConfig.GetValue<string>("SenderName"),
        };
        services.AddScoped<IEmailService>(x =>
            new EmailService(options)
        );
        #endregion

        #region System Log
        var mongoDbConnectionString = config.GetSection("MongoDbSettings").GetValue<string>("ConnectionString")!;
        var mongoDbCollectionName = config.GetSection("MongoDbSettings").GetValue<string>("CollectionName")!;

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.MongoDB(mongoDbConnectionString, collectionName: mongoDbCollectionName)
            .CreateLogger();
        #endregion

        return services;
    }
}
