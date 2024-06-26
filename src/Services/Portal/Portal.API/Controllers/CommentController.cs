using Common.Enums;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Portal.API.Attributes;
using Portal.Domain.Interfaces.Business.Services;
using Portal.Domain.Models.CommentModels;
using Portal.Domain.Models.LevelModels;

namespace Portal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : BaseApiController
    {
        private readonly ICommentService _commentService;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public CommentController(
            ICommentService commentService,
            IBackgroundJobClient backgroundJobClient)
        {
            _commentService = commentService;
            _backgroundJobClient = backgroundJobClient;
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
            if (!response.IsSuccess || response.Data == null)
            {
                return BadRequest(response);
            }

            #region Hangfire Enqueue Background
            await _backgroundJobClient.EnqueueWithCircuitBreakerAsync<ILevelService>(x => x.AddExperienceFromUserToRedisAsync(new LevelBuildRedisRequestModel
            {
                IdentityUserId = identityUserId,
                CommentId = response.Data.Id,
                AlbumId = response.Data.AlbumId,
                CollectionId = response.Data.CollectionId,
                CreatedOnUtc = DateTime.UtcNow,
                IpAddress = IpAddress(),
                SessionId = HttpContext.Session.Id
            }));
            #endregion

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

        [Authorize(ERoles.Administrator)]
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

        [Authorize(ERoles.Administrator)]
        [HttpDelete]
        [Route("reply/{id}")]
        public async Task<IActionResult> DeleteReply([FromRoute] int id)
        {
            var identityUserId = GetIdentityUserIdByToken();
            if (string.IsNullOrEmpty(identityUserId))
            {
                return BadRequest("error_user_not_found");
            }

            var response = await _commentService.DeleteReplyAsync(id, identityUserId);
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
