using Common.Enums;
using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Portal.API.Attributes;
using Portal.Domain.AggregatesModel.CollectionAggregate;
using Portal.Domain.AggregatesModel.UserAggregate;
using Portal.Domain.Enums;
using Portal.Domain.Interfaces.Business.Services;
using Portal.Domain.Models.ActivityLogs;
using Portal.Domain.Models.CommentModels;
using Portal.Domain.Models.UserModels;

namespace Portal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : BaseApiController
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<UserActivityLog> _userActivityLogRepository;
        private readonly IActivityLogService _activityLogService;
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;

        public UserController(IUnitOfWork unitOfWork, IActivityLogService activityLogService, IUserService userService)
        {
            _userRepository = unitOfWork.Repository<User>();
            _userActivityLogRepository = unitOfWork.Repository<UserActivityLog>();
            _activityLogService = activityLogService;
            _userService = userService;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        [Authorize]
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
        [Authorize]
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
        public async Task<IActionResult> GetActivityLogByIdentityUserId([FromRoute] string identityUserId, [FromQuery] PagingCommonRequest request, [FromQuery] EActivityType activityType)
        {
            var user = await _userRepository.GetByIdentityUserIdAsync(identityUserId);
            if (user == null)
            {
                return BadRequest("error_user_not_found");
            }

            // Paging shortcut linq to get paging user activity
            var totalRecords = await _userActivityLogRepository.GetQueryable()
                                        .Where(o => o.UserId == user.Id && o.ActivityType == activityType)
                                        .LongCountAsync();
            var activityLogs = await _userActivityLogRepository.GetQueryable()
                                        .Where(o => o.UserId == user.Id && o.ActivityType == activityType)
                                        .Sort(x => x.CreatedOnUtc, false)
                                        .Page(request.PageNumber, request.PageSize)
                                        .ToListAsync();

            var resposne = activityLogs.ConvertAll(x => new ActivityLogResponseModel
            {
                Id = x.Id,
                ActivityType = x.ActivityType,
                Description = x.Description,
                IpV4Address = x.IpV4Address,
                IpV6Address = x.IpV6Address,
                LogTimes = x.LogTimes,
                UserId = x.UserId,
                CreatedOnUtc = x.CreatedOnUtc
            });
            return Ok(new ServiceResponse<PagingCommonResponse<ActivityLogResponseModel>>(new PagingCommonResponse<ActivityLogResponseModel>
            {
                RowNum = totalRecords,
                Data = resposne
            }));
        }

        [HttpGet("ranking")]
        [RedisCache(30)]
        public async Task<IActionResult> GetPagingAsync([FromQuery] PagingCommonRequest request, [FromQuery] ERegion region)
        {
            var response = await _userService.GetPagingAsync(request, region);

            if (!response.IsSuccess)
                return BadRequest(response);

            return Ok(response);
        }


        [HttpGet("activity-logs")]
        [Authorize(ERoles.Administrator)]
        public async Task<IActionResult> GetActivityLogs([FromQuery] PagingCommonRequest request)
        {
            // Paging shortcut linq to get paging user activity
            var totalRecords = await _userActivityLogRepository.GetQueryable()
                                        .Where(o => o.ActivityType == EActivityType.Subscription)
                                        .LongCountAsync();
            var activityLogs = await _userActivityLogRepository.GetQueryable()
                                        .Where(o => o.ActivityType == EActivityType.Subscription)
                                        .Sort(x => x.Id, false)
                                        .Page(request.PageNumber, request.PageSize)
                                        .Select(x => new ActivityLogsPagingReponseModel
                                        {
                                            Id = x.Id,
                                            Description = x.Description,
                                            UserId = x.UserId,
                                            IdentityUserId = x.User.IdentityUserId,
                                            CreatedOnUtc = x.CreatedOnUtc,
                                            Email = x.User.Email
                                        })
                                        .ToListAsync();

            return Ok(new ServiceResponse<PagingCommonResponse<ActivityLogsPagingReponseModel>>(new PagingCommonResponse<ActivityLogsPagingReponseModel>
            {
                RowNum = totalRecords,
                Data = activityLogs
            }));
        }

        [HttpGet("comments/check")]
        [Authorize(ERoles.Administrator)]
        public async Task<IActionResult> GetPagingManagementAsync([FromQuery] CommentManagementRequestModel request)
        {
            long totalRecords;
            List<CommentManagementResponseModel>? response;

            if (request.IsReply)
            {
                // Paging shortcut linq to get paging user activity
                totalRecords = await _unitOfWork.Repository<ReplyComment>().GetQueryable()
                                            .Where(o => o.IsDeleted == request.IsDeleted && o.CreatedOnUtc >= request.StartDate && o.CreatedOnUtc < request.EndDate)
                                            .LongCountAsync();

                response = await _unitOfWork.Repository<ReplyComment>().GetQueryable()
                                            .Where(o => o.IsDeleted == request.IsDeleted && o.CreatedOnUtc >= request.StartDate && o.CreatedOnUtc < request.EndDate)
                                            .Sort(x => x.Id, false)
                                            .Page(request.PageNumber, request.PageSize)
                                            .Select(x => new CommentManagementResponseModel
                                            {
                                                Id = x.Id,
                                                UserId = x.UserId,
                                                CreatedOnUtc = x.CreatedOnUtc,
                                                Email = x.User.Email,
                                                Text = x.Text,
                                                AlbumFriendlyName = x.Comment!.Album!.FriendlyName,
                                                IsReply = true
                                            })
                                            .ToListAsync();

                return Ok(new ServiceResponse<PagingCommonResponse<CommentManagementResponseModel>>(new PagingCommonResponse<CommentManagementResponseModel>
                {
                    RowNum = totalRecords,
                    Data = response
                }));
            }

            // Paging shortcut linq to get paging user activity
            totalRecords = await _unitOfWork.Repository<Comment>().GetQueryable()
                                        .Where(o => o.IsDeleted == request.IsDeleted && o.CreatedOnUtc >= request.StartDate && o.CreatedOnUtc < request.EndDate)
                                        .LongCountAsync();

            response = await _unitOfWork.Repository<Comment>().GetQueryable()
                                        .Where(o => o.IsDeleted == request.IsDeleted && o.CreatedOnUtc >= request.StartDate && o.CreatedOnUtc < request.EndDate)
                                        .Sort(x => x.Id, false)
                                        .Page(request.PageNumber, request.PageSize)
                                        .Select(x => new CommentManagementResponseModel
                                        {
                                            Id = x.Id,
                                            UserId = x.UserId,
                                            CreatedOnUtc = x.CreatedOnUtc,
                                            Email = x.User.Email,
                                            Text = x.Text,
                                            IsReply = false,
                                            AlbumFriendlyName = x.Album!.FriendlyName
                                        })
                                        .ToListAsync();

            return Ok(new ServiceResponse<PagingCommonResponse<CommentManagementResponseModel>>(new PagingCommonResponse<CommentManagementResponseModel>
            {
                RowNum = totalRecords,
                Data = response
            }));
        }
    }
}
