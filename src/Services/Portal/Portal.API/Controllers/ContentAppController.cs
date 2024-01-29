using System.Security.Claims;
using Common.Interfaces;
using Common.Models;
using Common.ValueObjects;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Portal.API.Attributes;
using Portal.Domain.AggregatesModel.CollectionAggregate;
using Portal.Domain.Interfaces.Business.Services;
using Portal.Domain.Models.CollectionModels;

namespace Portal.API.Controllers
{
    [ApiController]
    [Route("api/client/[controller]")]
    [AllowAnonymous]
    public class ContentAppController : BaseApiController
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IRedisService _redisService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<ContentItem> _contentItemRepository;

        public ContentAppController(IUnitOfWork unitOfWork, IBackgroundJobClient backgroundJobClient, IRedisService redisService)
        {
            _backgroundJobClient = backgroundJobClient;
            _redisService = redisService;
            _unitOfWork = unitOfWork;
            _contentItemRepository = unitOfWork.Repository<ContentItem>();
        }

        [HttpGet("comics/{comicFriendlyName}/contents/{contentFriendlyName}")]
        public async Task<IActionResult> GetByIdAsync(string comicFriendlyName, string contentFriendlyName)
        {
            //#region Using cache if exists
            //var value = await _redisService.GetAsync<ContentAppModel>(string.Format(Const.RedisCacheKey.ComicContent, comicFriendlyName, contentFriendlyName));
            //if (value != null)
            //{
            //    // Hangfire
            //    _backgroundJobClient.Enqueue<ICollectionService>(x => x.AddViewFromUserToRedisAsync(new CollectionViewUserBuildModel
            //    {
            //        CollectionId = value!.Id,
            //        IdentityUserId = User.FindFirstValue("id"),
            //        AtViewedOnUtc = DateTime.UtcNow,
            //        IpAddress = IpAddress(),
            //        SessionId = HttpContext.Session.Id
            //    }));

            //    return Ok(new ServiceResponse<ContentAppModel>(value));
            //}
            //#endregion
            var parameters = new Dictionary<string, object?>
            {
                { "comicFriendlyName", comicFriendlyName },
                { "contentFriendlyName",  contentFriendlyName }
            };
            var collection = (await _unitOfWork.QueryAsync<ContentAppModel>("Collection_Content_GetByFriendlyName", parameters)).FirstOrDefault();
            if (collection == null)
            {
                return BadRequest(new ServiceResponse<ContentAppModel>("Không tìm thấy chap truyện tranh"));
            }

            var contentItems = await _contentItemRepository.GetQueryable()
                            .Where(o => o.CollectionId == collection.Id).OrderBy(o => o.OrderBy)
                            .Select(x => x.DisplayUrl).ToListAsync();
            collection.ContentItems = contentItems;

            var result = new ServiceResponse<ContentAppModel>(collection);
            await _redisService.SetAsync(string.Format(Const.RedisCacheKey.ComicContent, comicFriendlyName, contentFriendlyName), result.Data, 5);

            #region Hangfire Enqueue Background
            _backgroundJobClient.Enqueue<ICollectionService>(x => x.AddViewFromUserToRedisAsync(new CollectionViewUserBuildModel
            {
                CollectionId = result.Data!.Id,
                IdentityUserId = User.FindFirstValue("id"),
                AtViewedOnUtc = DateTime.UtcNow,
                IpAddress = IpAddress(),
                SessionId = HttpContext.Session.Id
            }));
            #endregion

            return Ok(result);
        }
    }
}
