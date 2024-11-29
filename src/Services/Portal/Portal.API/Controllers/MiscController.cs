using System.Security.Claims;
using Common.Interfaces;
using Hangfire;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using Portal.Domain.Interfaces.Business.Services;
using Portal.Domain.Models.CollectionModels;
using Portal.Domain.Models.LevelModels;
using Portal.Domain.Models.MiscModels;

namespace Portal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MiscController : BaseApiController
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ISimpleTokenService _simpleTokenService;

    public MiscController(
        IBackgroundJobClient backgroundJobClient,
        ISimpleTokenService simpleTokenService)
    {
        _backgroundJobClient = backgroundJobClient;
        _simpleTokenService = simpleTokenService;
    }

    [HttpPut]
    [Route("accumulate")]
    [RequestTimeout(5000)]
    public async Task<IActionResult> AccumulateAsync([FromBody] AccumulateModel model)
    {
        var identityUserId = GetIdentityUserIdByToken();
        var result = _simpleTokenService.VerifyToken<AccumulatePayload>(model.Token, out var payload);

        if (!result || payload == null)
            return BadRequest("invalid_token");

        var isBot = payload.IsBot;
        var now = DateTime.UtcNow;
        var ipAddress = GetIpAddress()?.Split(',').FirstOrDefault();

        // Clean Hangfire Job Times
        if (IsCleanJobsTimeNow()) isBot = true;

        if (!isBot)
        {
            #region Hangfire Enqueue Background
            await _backgroundJobClient.EnqueueWithCircuitBreakerAsync<ICollectionService>(x => x.AddViewFromUserToRedisAsync(new CollectionViewUserBuildModel
            {
                CollectionId = payload.CollectionId,
                IdentityUserId = User.FindFirstValue("id"),
                AtViewedOnUtc = DateTime.UtcNow,
                IpAddress = ipAddress,
                SessionId = HttpContext.Session.Id
            }));

            // User next chap from previous chapter
            if (payload.PreviousCollectionId.HasValue && !string.IsNullOrEmpty(identityUserId))
            {
                await _backgroundJobClient.EnqueueWithCircuitBreakerAsync<ILevelService>(x => x.AddExperienceFromUserToRedisAsync(new LevelBuildRedisRequestModel
                {
                    IdentityUserId = identityUserId,
                    CollectionId = payload.PreviousCollectionId.Value,
                    CreatedOnUtc = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    SessionId = HttpContext.Session.Id
                }));
            }
            else if (DateTime.UtcNow.Subtract(payload.GetCreatedOnUtc()).Hours < 4 && !string.IsNullOrEmpty(identityUserId))
            {
                await _backgroundJobClient.EnqueueWithCircuitBreakerAsync<ILevelService>(x => x.AddExperienceFromUserToRedisAsync(new LevelBuildRedisRequestModel
                {
                    IdentityUserId = identityUserId,
                    CollectionId = payload.CollectionId,
                    CreatedOnUtc = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    SessionId = HttpContext.Session.Id,
                    IsViewedNewChapter = true
                }));
            }
            #endregion
        }

        return Ok("checked");
    }

    private string? GetIpAddress()
    {
        // get source ip address for the current request
        if (Request.Headers.TryGetValue("CF-Connection-IP", out Microsoft.Extensions.Primitives.StringValues cfValue))
            return cfValue;
        if (Request.Headers.TryGetValue("X-Forwarded-For", out Microsoft.Extensions.Primitives.StringValues value))
            return value;
        else
            return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
    }

    private static bool IsCleanJobsTime(int targetHours)
    {
        // Get the current time in UTC
        DateTime now = DateTime.UtcNow;
        int hours = now.Hour;
        int minutes = now.Minute;
        int currentTime = hours * 60 + minutes; // Convert current time to minutes since midnight

        // Define the start and end of the range in minutes since midnight (UTC+7)
        int startTime = targetHours * 60 + 0;  // 21:00 UTC or 7:00 UTC
        int endTime = targetHours * 60 + 17;  // 21:17 UTC or 7:17 UTC

        // Check if the current time falls within the range
        return currentTime >= startTime && currentTime <= endTime;
    }

    private static bool IsHighWorkLoadTrafficTime()
    {
        DateTime now = DateTime.UtcNow;
        int hours = now.Hour;
        int minutes = now.Minute;
        int currentTime = hours * 60 + minutes; // Convert current time to minutes since midnight

        // Define the start and end of the range in minutes since midnight (UTC+7)
        int startTime = 15 * 60 + 30;  // 22:30
        int endTime = 16 * 60 + 15;  // 23:15

        // Check if the current time falls within the range
        return currentTime >= startTime && currentTime <= endTime;
    }

    private static bool IsCleanJobsTimeNow()
    {
        return IsCleanJobsTime(21) || IsCleanJobsTime(7) || IsHighWorkLoadTrafficTime();
    }
}
