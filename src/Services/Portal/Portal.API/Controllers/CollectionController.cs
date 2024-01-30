using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Portal.API.Attributes;
using Portal.Domain.Interfaces.Business.Services;
using Portal.Domain.Models.CollectionModels;
using Portal.Domain.Models.ContentItemModels;

namespace Portal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CollectionController : ControllerBase
    {
        private readonly ICollectionService _collectionService;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IContentItemService _contentItemService;

        public CollectionController(
            ICollectionService collectionService,
            IBackgroundJobClient backgroundJobClient,
            IContentItemService contentItemService)
        {
            _collectionService = collectionService;
            _backgroundJobClient = backgroundJobClient;
            _contentItemService = contentItemService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CollectionRequestModel model)
        {
            var result = await _collectionService.CreateAsync(model);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> Update(int id, CollectionRequestModel model)
        {
            var result = await _collectionService.UpdateAsync(id, model);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("all")]
        [RedisCache(5)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _collectionService.GetAllAsync();
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _collectionService.DeleteAsync(id);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("{id}/content-items")]
        public async Task<IActionResult> GetContentItems(int id)
        {
            var result = await _collectionService.GetContentItemsAsync(id);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("{id}/content-items")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
        [EnableRouteResponseCompression]
        [MultipartFormData]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> CreateContentItemsAsync([FromRoute] int id)
        {
            await _contentItemService.CreateContentItemsAsync(id, Request.Body, Request.ContentType ?? string.Empty);
            return Ok();
        }

        [HttpPut]
        [Route("{id}/content-items")]
        [EnableRouteResponseCompression]
        public IActionResult UpdateContentItems([FromRoute] int id, [FromBody] ContentItemUpdateRequestModel updateRequestModel)
        {
            _backgroundJobClient.Enqueue<IContentItemService>(x => x.UpdateContentItemsAsync(id, updateRequestModel));
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetPagingAsync([FromQuery] CollectionPagingRequest request)
        {
            var response = await _collectionService.GetPagingAsync(request);

            if (!response.IsSuccess)
                return BadRequest(response);

            return Ok(response);
        }
    }
}
