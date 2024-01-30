using Common;
using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Portal.API.Attributes;
using Portal.Domain.AggregatesModel.AlbumAggregate;
using Portal.Domain.AggregatesModel.CollectionAggregate;
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

        public ComicAppController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _albumRepository = unitOfWork.Repository<Album>();
            _collectionRepository = unitOfWork.Repository<Collection>();
        }

        [HttpGet]
        [RedisCache(5)]
        public async Task<IActionResult> GetAsync()
        {
            var comics = await _albumRepository.GetAllAsync();
            var result = new ServiceResponse<List<ComicAppModel>>(comics.ConvertAll(x => new ComicAppModel
            {
                Id = x.Id,
                Title = x.Title,
                FriendlyName = x.FriendlyName,
                Description = x.Description,
                AlbumAlertMessageName = x.AlbumAlertMessage?.Name,
                ContentTypeNames = x.AlbumContentTypes?.Where(y => !string.IsNullOrEmpty(y.ContentType?.Name)).Select(y => y.ContentType.Name).OrderBy(o => o).JoinSeparator(),
                IsPublic = x.IsPublic,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedOnUtc = x.UpdatedOnUtc,
                CdnThumbnailUrl = x.CdnThumbnailUrl,
                Views = x.Views,
                Contents = x.Collections.OrderByDescending(y => y.Title).Take(5).Select(z => new ContentAppModel
                {
                    Id = z.Id,
                    Title = z.Title,
                    FriendlyName = z.FriendlyName,
                    CreatedOnUtc = z.CreatedOnUtc,
                    UpdatedOnUtc = z.UpdatedOnUtc,
                    IsPublic = z.IsPublic,
                    AlbumId = z.AlbumId,
                    AlbumTitle = x.Title,
                    AlbumFriendlyName = x.FriendlyName,
                    Description = z.Description,
                    ExtendName = z.ExtendName,
                    Volume = z.Volume,
                    Views = z.Views,
                }).ToList()
            }));

            return Ok(result);
        }

        [HttpGet("{friendlyName}")]
        [RedisCache(5)]
        public async Task<IActionResult> GetByIdAsync(string friendlyName)
        {
            var parameters = new Dictionary<string, object?>
            {
                { "friendlyName",  friendlyName }
            };
            var comic = (await _unitOfWork.QueryAsync<ComicAppModel>("Collection_Comic_GetByFriendlyName", parameters)).FirstOrDefault();
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
            }).OrderByDescending(x => RegexHelper.GetNumberByText(x.Title)).ToList();

            var result = new ServiceResponse<ComicAppModel>(comic);

            return Ok(result);
        }
    }
}
