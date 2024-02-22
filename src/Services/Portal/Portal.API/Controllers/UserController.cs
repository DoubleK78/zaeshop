using Common.Enums;
using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Portal.API.Attributes;
using Portal.Domain.AggregatesModel.UserAggregate;
using Portal.Domain.Enums;
using Portal.Domain.Interfaces.Business.Services;
using Portal.Domain.Models.ActivityLogs;
using Portal.Domain.Models.UserModels;

namespace Portal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : BaseApiController
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<UserActivityLog> _userActivityLogRepository;
        private readonly IActivityLogService _activityLogService;

        public UserController(IUnitOfWork unitOfWork, IActivityLogService activityLogService)
        {
            _userRepository = unitOfWork.Repository<User>();
            _userActivityLogRepository = unitOfWork.Repository<UserActivityLog>();
            _activityLogService = activityLogService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var identityUserId = GetIdentityUserIdByToken();
            if (string.IsNullOrEmpty(identityUserId))
            {
                return BadRequest("error_user_not_found");
            }

            var user = await _userRepository.GetByIdentityUserIdAsync(identityUserId);
            if (user == null)
            {
                return BadRequest("error_user_not_found");
            }

            // Response
            var response = new UserProfileResponse
            {
                Id = user.Id,
                Email = user.Email,
                Avatar = user.Avatar,
                FullName = user.FullName,
                UserName = user.UserName,
                LevelId = user.LevelId,
                CurrentExp = user.CurrentExp,
                NextLevelExp = user.NextLevelExp,
                RoleType = user.RoleType,
                ExpriedRoleDate = user.ExpriedRoleDate
            };

            return Ok(new ServiceResponse<UserProfileResponse>(response));
        }

        [HttpPost("activity-log")]
        public async Task<IActionResult> CreateLog(ActivityLogRequestModel model)
        {
            var identityUserId = GetIdentityUserIdByToken();
            if (string.IsNullOrEmpty(identityUserId))
                return BadRequest("error_user_not_found");

            var user = await _userRepository.GetByIdentityUserIdAsync(identityUserId);

            if (user == null)
                return BadRequest("error_user_not_found");

            model.UserId = user.Id;
            var response = await _activityLogService.CreateAsync(model);

            if (!response.IsSuccess)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("{identityUserId}/activity-log")]
        [Authorize(ERoles.Administrator)]
        public async Task<IActionResult> GetActivityLogByIdentityUserId([FromRoute] string identityUserId, [FromQuery] PagingCommonRequest request, [FromQuery] EActivityType type)
        {
            var user = await _userRepository.GetByIdentityUserIdAsync(identityUserId);
            if (user == null)
            {
                return BadRequest("error_user_not_found");
            }

            // Paging shortcut linq to get paging user activity
            var totalRecords = await _userActivityLogRepository.GetQueryable()
                                        .Where(o => o.UserId == user.Id && o.ActivityType == type)
                                        .LongCountAsync();
            var activityLogs = await _userActivityLogRepository.GetQueryable()
                                        .Where(o => o.UserId == user.Id && o.ActivityType == type)
                                        .Page(request.PageNumber, request.PageSize)
                                        .Sort(x => x.CreatedOnUtc, false)
                                        .ToListAsync();

            var resposne = activityLogs.ConvertAll(x => new ActivityLogResponseModel
            {
                Id = x.Id,
                ActivityType = x.ActivityType,
                Description = x.Description,
                IpV4Address = x.IpV4Address,
                IpV6Address = x.IpV6Address,
                LogTimes = x.LogTimes,
                UserId = x.UserId
            });
            return Ok(new ServiceResponse<PagingCommonResponse<ActivityLogResponseModel>>(new PagingCommonResponse<ActivityLogResponseModel>
            {
                RowNum = totalRecords,
                Data = resposne
            }));
        }
    }
}
