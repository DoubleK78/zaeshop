using Common;
using Common.Interfaces;
using Common.Models;
using Common.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Portal.Domain.Models.CollectionModels;

namespace Portal.API.Attributes.Business
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ContentComicRedisCacheAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // 1. Get Services to check cache (standalone to optimize time to load)
            var redisService = context.HttpContext.RequestServices.GetRequiredService<IRedisService>();

            var comicFriendlyName = Convert.ToString(context.HttpContext.GetRouteValue("comicFriendlyName"));
            var contentFriendlyName = Convert.ToString(context.HttpContext.GetRouteValue("contentFriendlyName"));

            // 2. Get from cache, if not go to API
            #region Using cache if exists
            var value = await redisService.GetAsync<ContentAppModel>(string.Format(Const.RedisCacheKey.ComicContent, comicFriendlyName, contentFriendlyName));
            if (value != null)
            {
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