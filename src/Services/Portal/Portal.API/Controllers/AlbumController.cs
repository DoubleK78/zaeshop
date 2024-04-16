using Common.Enums;
using Common.Models;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Portal.API.Attributes;
using Portal.Domain.Interfaces.Business.Services;
using Portal.Domain.Models.AlbumModels;
using Portal.Domain.Models.CollectionModels;

namespace Portal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlbumController : ControllerBase
    {
        private readonly IAlbumService _albumService;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public AlbumController(IAlbumService albumService, IBackgroundJobClient backgroundJobClient)
        {
            _albumService = albumService;
            _backgroundJobClient = backgroundJobClient;
        }

        [Authorize(ERoles.Administrator)]
        [HttpPost]
        public async Task<IActionResult> Create(AlbumRequestModel model)
        {
            var response = await _albumService.CreateAsync(model);

            if (!response.IsSuccess)
                return BadRequest(response);

            return Ok(response);
        }

        [Authorize(ERoles.Administrator)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, AlbumRequestModel model)
        {
            var response = await _albumService.UpdateAsync(id, model);

            if (!response.IsSuccess)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("all")]
        [RedisCache(5)]
        public async Task<IActionResult> GetAll()
        {
            var response = await _albumService.GetAllAsync();

            if (!response.IsSuccess)
                return BadRequest(response);

            return Ok(response);
        }

        [Authorize(ERoles.Administrator)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _albumService.DeleteAsync(id);

            if (!response.IsSuccess)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetPagingAsync([FromQuery] PagingCommonRequest request, [FromQuery] FilterAdvanced filter)
        {
            var response = await _albumService.GetPagingAsync(request, filter);

            if (!response.IsSuccess)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var response = await _albumService.GetByIdAsync(id);

            if (!response.IsSuccess)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("{id}/extra-info")]
        public async Task<IActionResult> GetExtraInfoByIdAsync(int id)
        {
            var response = await _albumService.GetExtraInfoByIdAsync(id);

            if (!response.IsSuccess)
                return BadRequest(response);

            return Ok(response);
        }

        [Authorize(ERoles.Administrator)]
        [HttpPut("{id}/extra-info")]
        public async Task<IActionResult> UpdateExtraInfoByIdAsync(int id, AlbumExtraInfoModel model)
        {
            var response = await _albumService.UpdateExtraInfoByIdAsync(id, model);

            if (!response.IsSuccess)
                return BadRequest(response);

            return Ok(response);
        }

        [Authorize(ERoles.Administrator)]
        [HttpPost]
        [Route("{id}/collections/bulk-create")]
        public IActionResult BulkCreateCollections(int id, [FromBody] List<BulkCreateCollectionRequest> collections)
        {
            _backgroundJobClient.Enqueue<ICollectionService>(x => x.BulkCreateAsync(id, collections));
            return Ok();
        }

        //[Authorize(ERoles.Administrator)]
        [HttpPost]
        [Route("create-schedule")]
        public async Task<IActionResult> CreateSchedule([FromBody] AlbumScheduleModel requestModel)
        {
            var response = await _albumService.CreateScheduleAsync(requestModel);

            if (!response.IsSuccess)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet]
        [Route("schedule")]
        [RedisCache(60 * 24)]
        public async Task<IActionResult> GetSchedule([FromQuery] AlbumScheduleRequestModel requestModel)
        {
            var response = await _albumService.GetScheduleAsync(requestModel);

            if (!response.IsSuccess)
                return BadRequest(response);

            return Ok(response);
        }
    }
}
