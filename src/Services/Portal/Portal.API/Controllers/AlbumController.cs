using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Portal.API.Attributes;
using Portal.Domain.Interfaces.Business.Services;
using Portal.Domain.Models.AlbumModels;

namespace Portal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlbumController : ControllerBase
    {
        private readonly IAlbumService _albumService;

        public AlbumController(IAlbumService albumService)
        {
            _albumService = albumService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(AlbumRequestModel model)
        {
            var response = await _albumService.CreateAsync(model);

            if (!response.IsSuccess)
                return BadRequest(response);

            return Ok(response);
        }

        [Authorize]
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

        [Authorize]
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

        [Authorize]
        [HttpPut("{id}/extra-info")]
        public async Task<IActionResult> UpdateExtraInfoByIdAsync(int id, AlbumExtraInfoModel model)
        {
            var response = await _albumService.UpdateExtraInfoByIdAsync(id, model);

            if (!response.IsSuccess)
                return BadRequest(response);

            return Ok(response);
        }
    }
}
