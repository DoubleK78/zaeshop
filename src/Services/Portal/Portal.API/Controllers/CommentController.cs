using Microsoft.AspNetCore.Mvc;
using Portal.API.Attributes;
using Portal.Domain.Interfaces.Business.Services;
using Portal.Domain.Models.CommentModels;

namespace Portal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : BaseApiController
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CommentRequestModel model)
        {
            var identityUserId = GetIdentityUserIdByToken();
            if (string.IsNullOrEmpty(identityUserId))
            {
                return BadRequest("error_user_not_found");
            }

            var response = await _commentService.CreateAsync(model, identityUserId);
            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [Authorize]
        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] CommentRequestModel model)
        {
            var identityUserId = GetIdentityUserIdByToken();
            if (string.IsNullOrEmpty(identityUserId))
            {
                return BadRequest("error_user_not_found");
            }

            var response = await _commentService.UpdateAsync(id, model, identityUserId);
            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [Authorize]
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var identityUserId = GetIdentityUserIdByToken();
            if (string.IsNullOrEmpty(identityUserId))
            {
                return BadRequest("error_user_not_found");
            }

            var response = await _commentService.DeleteAsync(id, identityUserId);
            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetPagingAsync([FromQuery] CommentPagingRequestModel request)
        {
            var response = await _commentService.GetPagingAsync(request);

            if (!response.IsSuccess)
                return BadRequest(response);

            return Ok(response);
        }
    }
}
