using System.Security.Claims;
using Common;
using Common.Interfaces;
using Common.Models;
using Common.ValueObjects;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Portal.Domain.Interfaces.Business.Services;
using Portal.Domain.Models.CollectionModels;
using Portal.Domain.Models.LevelModels;

namespace Portal.API.Attributes.Business
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ContentComicRedisCacheAttribute : Attribute, IAsyncActionFilter
    {
        private static string? IpAddress(ActionExecutingContext context)
        {
            // get source ip address for the current request
            if (context.HttpContext.Request.Headers.TryGetValue("X-Forwarded-For", out Microsoft.Extensions.Primitives.StringValues value))
                return value;
            else
                return context.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // 1. Get Services to check cache (standalone to optimize time to load)
            var redisService = context.HttpContext.RequestServices.GetRequiredService<IRedisService>();
            var backgroundJobClient = context.HttpContext.RequestServices.GetRequiredService<IBackgroundJobClient>();
            var identityUserId = context.HttpContext.User.FindFirstValue("id");

            var previousCollectionIdString = context.HttpContext.Request.Query["previousCollectionId"].ToString();
            int? previousCollectionId = null;
            bool isSucess = int.TryParse(previousCollectionIdString, out int output);
            if (!string.IsNullOrEmpty(previousCollectionIdString) && !isSucess)
            {
                context.Result = new ContentResult
                {
                    Content = "error_previouscollectionid_is_not_valid",
                    ContentType = "application/json",
                    StatusCode = 400
                };
                return;
            }
            else if (!string.IsNullOrEmpty(previousCollectionIdString))
            {
                previousCollectionId = output;
            }

            var comicFriendlyName = Convert.ToString(context.HttpContext.GetRouteValue("comicFriendlyName"));
            var contentFriendlyName = Convert.ToString(context.HttpContext.GetRouteValue("contentFriendlyName"));

            // 2. Get from cache, if not go to API
            #region Using cache if exists
            var value = await redisService.GetAsync<ContentAppModel>(string.Format(Const.RedisCacheKey.ComicContent, comicFriendlyName, contentFriendlyName));
            if (value != null)
            {
                // Hangfire
                backgroundJobClient.Enqueue<ICollectionService>(x => x.AddViewFromUserToRedisAsync(new CollectionViewUserBuildModel
                {
                    CollectionId = value!.Id,
                    IdentityUserId = identityUserId,
                    AtViewedOnUtc = DateTime.UtcNow,
                    IpAddress = IpAddress(context),
                    SessionId = context.HttpContext.Session.Id
                }));

                // User next chap from previous chapter
                if (previousCollectionId.HasValue && !string.IsNullOrEmpty(identityUserId))
                {
                    backgroundJobClient.Enqueue<ILevelService>(x => x.AddExperienceFromUserToRedisAsync(new LevelBuildRedisRequestModel
                    {
                        IdentityUserId = identityUserId,
                        CollectionId = previousCollectionId,
                        CreatedOnUtc = DateTime.UtcNow,
                        IpAddress = IpAddress(context),
                        SessionId = context.HttpContext.Session.Id
                    }));
                }

                context.Result = new ContentResult
                {
                    Content = JsonSerializationHelper.Serialize(new ServiceResponse<ContentAppModel>(value)),
                    ContentType = "application/json",
                    StatusCode = 200
                };
                return;
            }
            #endregion

            await next(); // Move controller
        }
    }
}
