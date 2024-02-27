using System.Security.Claims;
using Common.Interfaces;
using Common.Models;
using Common.ValueObjects;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Portal.API.Attributes;
using Portal.API.Attributes.Business;
using Portal.Domain.AggregatesModel.CollectionAggregate;
using Portal.Domain.Interfaces.Business.Services;
using Portal.Domain.Models.CollectionModels;
using Portal.Domain.Models.LevelModels;

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
        // [ContentComicRedisCache]
        public async Task<IActionResult> GetByIdAsync([FromRoute] string comicFriendlyName, [FromRoute] string contentFriendlyName, [FromQuery] int? previousCollectionId = null, [FromQuery] bool isBot = false)
        {
            var identityUserId = GetIdentityUserIdByToken();
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
                            .Select(x => x.RelativeUrl).ToListAsync();
            collection.ContentItems = contentItems;

            var result = new ServiceResponse<ContentAppModel>(collection);
            // await _redisService.SetAsync(string.Format(Const.RedisCacheKey.ComicContent, comicFriendlyName, contentFriendlyName), result.Data, 60);

            if (!isBot)
            {
                #region Hangfire Enqueue Background
                _backgroundJobClient.Enqueue<ICollectionService>(x => x.AddViewFromUserToRedisAsync(new CollectionViewUserBuildModel
                {
                    CollectionId = result.Data!.Id,
                    IdentityUserId = User.FindFirstValue("id"),
                    AtViewedOnUtc = DateTime.UtcNow,
                    IpAddress = IpAddress(),
                    SessionId = HttpContext.Session.Id
                }));

                // User next chap from previous chapter
                if (previousCollectionId.HasValue && !string.IsNullOrEmpty(identityUserId))
                {
                    _backgroundJobClient.Enqueue<ILevelService>(x => x.AddExperienceFromUserToRedisAsync(new LevelBuildRedisRequestModel
                    {
                        IdentityUserId = identityUserId,
                        CollectionId = previousCollectionId.Value,
                        CreatedOnUtc = DateTime.UtcNow,
                        IpAddress = IpAddress(),
                        SessionId = HttpContext.Session.Id
                    }));
                }
                else if (DateTime.UtcNow.Subtract(collection.CreatedOnUtc).Hours < 4 && !string.IsNullOrEmpty(identityUserId))
                {
                    _backgroundJobClient.Enqueue<ILevelService>(x => x.AddExperienceFromUserToRedisAsync(new LevelBuildRedisRequestModel
                    {
                        IdentityUserId = identityUserId,
                        CollectionId = collection.Id,
                        CreatedOnUtc = DateTime.UtcNow,
                        IpAddress = IpAddress(),
                        SessionId = HttpContext.Session.Id,
                        IsViewedNewChapter = true
                    }));
                }
                #endregion
            }

            return Ok(result);
        }

        [HttpGet("comics/{comicFriendlyName}/contents/{contentFriendlyName}/metadata")]
        [RedisCache(60)]
        public async Task<IActionResult> GetMetadata([FromRoute] string comicFriendlyName, [FromRoute] string contentFriendlyName)
        {
            var collectionMetadata = await _unitOfWork.Repository<Collection>().GetQueryable()
                .Select(o => new CollectionMetaModel
                {
                    ContentTitle = o.Title,
                    ContentFriendlyName = o.FriendlyName,
                    ComicTitle = o.Album.Title,
                    ComicFriendlyName = o.Album.FriendlyName,
                    ComicImageUrl = o.Album.CdnThumbnailUrl,
                    Region = o.Album.Region
                })
                .FirstOrDefaultAsync(x => x.ComicFriendlyName == comicFriendlyName && x.ContentFriendlyName == contentFriendlyName);

            if (collectionMetadata == null)
            {
                return Ok(new ContentMetadata());
            }

            return Ok(new ContentMetadata
            {
                ComicTitle = collectionMetadata.ComicTitle,
                ContentTitle = collectionMetadata.ContentTitle,
                ComicImageUrl = collectionMetadata.ComicImageUrl,
                Region = collectionMetadata.Region
            });
        }
    }
}
