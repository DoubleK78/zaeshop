using Common;
using Common.Enums;
using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Portal.API.Attributes;
using Portal.Domain.AggregatesModel.AlbumAggregate;
using Portal.Domain.AggregatesModel.CollectionAggregate;
using Portal.Domain.Enums;
using Portal.Domain.Interfaces.Business.Services;
using Portal.Domain.Models.AlbumModels;
using Portal.Domain.Models.CollectionModels;

namespace Portal.API.Controllers
{
    [ApiController]
    [Route("api/client/[controller]")]
    [AllowAnonymous]
    public class ComicAppController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<Album> _albumRepository;
        private readonly IGenericRepository<Collection> _collectionRepository;
        private readonly IAlbumService _albumService;

        public ComicAppController(IUnitOfWork unitOfWork, IAlbumService albumService)
        {
            _unitOfWork = unitOfWork;
            _albumRepository = unitOfWork.Repository<Album>();
            _collectionRepository = unitOfWork.Repository<Collection>();
            _albumService = albumService;
        }

        [HttpGet("paging")]
        [RedisCache(60 * 2)]
        public async Task<IActionResult> GetAsync([FromQuery] PagingCommonRequest request, [FromQuery] FilterAdvanced filter)
        {
            var response = await _albumService.GetPagingAsync(request, filter);
            if (!response.IsSuccess)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("{friendlyName}")]
        [RedisCache(60)]
        public async Task<IActionResult> GetByIdAsync(string friendlyName)
        {
            var parameters = new Dictionary<string, object?>
            {
                { "friendlyName",  friendlyName }
            };
            var comic = (await _unitOfWork.QueryAsync<ComicAppModel>("Collection_Comic_GetByFriendlyName", parameters, commandTimeout: 180)).FirstOrDefault();
            if (comic == null)
            {
                return BadRequest(new ServiceResponse<ComicAppModel>("Không tìm thấy truyện tranh"));
            }

            var collections = await _collectionRepository.GetQueryable().Where(o => o.AlbumId == comic.Id).ToListAsync();
            comic.Contents = collections.ConvertAll(z => new ContentAppModel
            {
                Id = z.Id,
                Title = z.Title,
                FriendlyName = z.FriendlyName,
                CreatedOnUtc = z.CreatedOnUtc,
                UpdatedOnUtc = z.UpdatedOnUtc,
                IsPublic = z.IsPublic,
                AlbumId = z.AlbumId,
                AlbumTitle = comic.Title,
                AlbumFriendlyName = comic.FriendlyName,
                Views = z.Views,
                LevelPublic = z.LevelPublic,
                AlbumLevelPublic = comic.LevelPublic,
                Region = comic.Region,
                StorageType = z.StorageType
            }).OrderByDescending(x => RegexHelper.GetNumberByText(x.Title)).ToList();

            var result = new ServiceResponse<ComicAppModel>(comic);

            return Ok(result);
        }

        [HttpGet]
        [Route("{comicFriendlyName}/metadata")]
        [RedisCache(60)]
        public async Task<IActionResult> GetMetadata([FromRoute] string comicFriendlyName)
        {
            var albumMetadata = (await _unitOfWork.QueryAsync<AlbumMetadataModel>("Comic_Metadata", new Dictionary<string, object?>
            {
                { "comicFriendlyName",  comicFriendlyName }
            })).FirstOrDefault();

            if (albumMetadata == null || string.IsNullOrEmpty(albumMetadata.Title))
            {
                return Ok(new ComicMetadata());
            }

            var response = new ComicMetadata
            {
                Title = albumMetadata.Title,
                LastestChapter = albumMetadata.LastestChapter,
                ComicImageUrl = albumMetadata.ComicImageUrl,
                Region = albumMetadata.Region
            };

            return Ok(response);
        }

        [HttpGet]
        [Route("sitemap")]
        [RedisCache(60)]
        public async Task<IActionResult> GetSitemap()
        {
            var comicSitemap = await _albumRepository.GetQueryable()
                .Where(o => !string.IsNullOrEmpty(o.FriendlyName))
                .OrderByDescending(o => o.Views)
                .Select(o => new ComicSitemap
                {
                    FriendlyName = o.FriendlyName,
                    Region = o.Region,
                    ContentFriendlyNames = o.Collections.Where(c => !string.IsNullOrEmpty(c.FriendlyName)).Select(x => x.FriendlyName!).ToList()
                }).ToListAsync();
            return Ok(comicSitemap);
        }

        [HttpGet]
        [Route("comics-search")]
        [RedisCache(60 * 24)]
        public async Task<IActionResult> GetAllComicsSearch([FromQuery] ERegion region)
        {
            var comicsSearch = await _albumRepository.GetQueryable()
                .Where(o => o.Region == region)
                .OrderByDescending(o => o.Views)
                .Select(o => new ComicSearchModel
                {
                    Title = o.Title,
                    FriendlyName = o.FriendlyName,
                    Region = o.Region
                }).ToListAsync();

            return Ok(new ServiceResponse<List<ComicSearchModel>?>(comicsSearch));
        }
    }
}
