using Portal.Domain.Interfaces.Business.Services;
using Portal.Infrastructure.Implements.Business.Services;

namespace HangFireServer.Extensions
{
    public static class BusinessServiceExtension
    {
        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            // Inject Services
            services.AddScoped<IContentTypeService, ContentTypeService>();
            services.AddScoped<IAlbumAlertMessageService, AlbumAlertMessageService>();
            services.AddScoped<IAlbumService, AlbumService>();
            services.AddScoped<ICollectionService, CollectionService>();
            services.AddScoped<IContentItemService, ContentItemService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IFollowingService, FollowingService>();
            services.AddScoped<ILevelService, LevelService>();
            services.AddScoped<IActivityLogService, ActivityLogService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
}
