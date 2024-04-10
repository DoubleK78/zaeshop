using System.Security.Claims;
using Common;
using Common.Enums;
using Common.Models;
using Identity.API.Attributes;
using Identity.Domain.AggregatesModel.UserAggregate;
using Identity.Domain.Business.Interfaces.Services;
using Identity.Domain.Models.ErrorCodes;
using Identity.Domain.Models.ErrorResponses;
using Identity.Domain.Models.Users;
using Identity.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        #region fields
        private readonly IUserService _userService;
        private readonly AppIdentityDbContext _context;
        private readonly UserManager<User> _userManager;
        #endregion

        #region ctor
        public UserController(
            IUserService userService,
            AppIdentityDbContext context,
            UserManager<User> userManager)
        {
            _userService = userService;
            _context = context;
            _userManager = userManager;
        }
        #endregion

        [HttpPost]
        [Authorize(ERoles.Administrator)]
        public async Task<IActionResult> CreateAsync([FromBody] UserRegisterRequestModel model)
        {
            var errorResult = new ErrorResult();
            var user = await _userService.CreateAsync(model, errorResult);

            if (user == null || !string.IsNullOrEmpty(errorResult.Description))
            {
                return BadRequest(errorResult);
            }

            return Ok(user);
        }

        [HttpPut]
        [Route("{id}")]
        [Authorize(ERoles.Administrator)]
        public async Task<IActionResult> UpdateAsync(string id, [FromBody] UserUpdateRequestModel model)
        {
            var errorResult = new ErrorResult();
            var user = await _userService.UpdateAsync(id, model, errorResult);

            if (user == null || !string.IsNullOrEmpty(errorResult.Description))
            {
                return BadRequest(errorResult);
            }

            return Ok(user);
        }

        [HttpDelete]
        [Route("{id}")]
        [Authorize(ERoles.Administrator)]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            var errorResult = new ErrorResult();

            // find user by id
            var user = await _userService.GetByIdAsync(id);

            // validate user
            if (user == null)
            {
                errorResult.Description = nameof(ErrorCodes.UserNotExists);
                return BadRequest(errorResult);
            }

            // delete user by user manager
            _context.Users.Remove(user);
            _context.SaveChanges();

            return Ok(user);
        }

        [HttpGet]
        public async Task<IActionResult> GetPagingAsync([FromQuery] PagingCommonRequest request)
        {
            var usersPagingResponse = await _userService.GetPagingAsync(request);
            return Ok(usersPagingResponse);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllAsync()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(string id)
        {
            var user = await _userService.GetByIdAsync(id);
            return Ok(user);
        }

        [HttpGet("{id}/role-subscription")]
        [Authorize(ERoles.Administrator)]
        public async Task<IActionResult> GetRoleSubcriptionAsync(string id)
        {
            var userRoleSubscription = await _userService.GetUserRoleSubcriptionAsync(id);
            if (!userRoleSubscription.IsSuccess)
            {
                return BadRequest(userRoleSubscription.ErrorMessage);
            }

            return Ok(userRoleSubscription);
        }

        [HttpPut("{id}/role-subscription")]
        [Authorize(ERoles.Administrator)]
        public async Task<IActionResult> UpdateRoleSubcriptionAsync(string id, [FromBody] UserRoleSubcriptionRequestModel requestModel)
        {
            requestModel.UserId = id;
            var response = await _userService.UpdateUserRoleFromSubscriptionAsync(requestModel);
            if (!string.IsNullOrEmpty(response))
            {
                return BadRequest(response);
            }
            return Ok("sucesss");
        }

        [HttpGet("type-sub")]
        public async Task<IActionResult> GetCurrentRolesAsync()
        {
            var userId = User.FindFirstValue("id");
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Ok(ERoleType.User);
                }

                var roles = await _userManager.GetRolesAsync(user);
                return Ok(new
                {
                    IsBanned = user.IsBanned,
                    RoleType = CommonHelper.GetRoleType(roles.ToList())
                });
            }

            return Ok(ERoleType.User);
        }

        [HttpGet("new-subscription")]
        public async Task<IActionResult> GetNewSubscriptionAsync()
        {
            var userId = User.FindFirstValue("id");
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return BadRequest("error_user_not_found");
                }

                var roles = await _userManager.GetRolesAsync(user);
                return Ok(new ServiceResponse<UserRoleNewSubscription>(new UserRoleNewSubscription
                {
                    Role = roles.ToList(),
                    ExpriedRoleDate = user.ExpriedRoleDate
                }));
            }

            return Ok();
        }

        [HttpGet("prepare")]
        [Authorize]
        public async Task<IActionResult> CheckCurrentUserFingerPrint()
        {
            var userId = User.FindFirstValue("id");
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return BadRequest("error_user_not_found");
                }
            }

            return Ok();
        }

        [HttpPut("prepare")]
        [Authorize]
        public async Task<IActionResult> CreateOrUpdateCurrentUserFingerPrint()
        {
            var userId = User.FindFirstValue("id");
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return BadRequest("error_user_not_found");
                }
            }

            return Ok();
        }
    }
}