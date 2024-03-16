using Common.Enums;
using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Portal.API.Attributes;
using Portal.Domain.AggregatesModel.UserAggregate;
using Portal.Domain.Models.UserModels;

namespace Portal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserDeviceController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<UserDevice> _userDeviceRepository;

        public UserDeviceController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _userRepository = unitOfWork.Repository<User>();
            _userDeviceRepository = unitOfWork.Repository<UserDevice>();
        }

        [HttpGet]
        public async Task<IActionResult> GetPaging([FromQuery] PagingCommonRequest request)
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

            // Paging shortcut linq to get paging user activity
            var totalRecords = await _userDeviceRepository.GetQueryable()
                                        .Where(o => o.UserId == user.Id)
                                        .LongCountAsync();
            var activityLogs = await _userDeviceRepository.GetQueryable()
                                        .Where(o => o.UserId == user.Id)
                                        .Page(request.PageNumber, request.PageSize)
                                        .Sort(x => x.UpdatedOnUtc, false)
                                        .ToListAsync();

            var resposne = activityLogs.ConvertAll(x => new UserDeviceResponseModel
            {
                Id = x.Id,
                RegistrationToken = x.RegistrationToken,
                DeviceType = x.DeviceType,
                ScreenResolution = x.ScreenResolution,
                IsEnabled = x.IsEnabled,
                BrowserVersion = x.BrowserVersion,
                UserId = x.UserId,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedOnUtc = x.UpdatedOnUtc
            });
            return Ok(new ServiceResponse<PagingCommonResponse<UserDeviceResponseModel>>(new PagingCommonResponse<UserDeviceResponseModel>
            {
                RowNum = totalRecords,
                Data = resposne
            }));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserDeviceRequestModel model)
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

            var isExistsToken = await _userDeviceRepository.GetQueryable().AnyAsync(x => x.RegistrationToken == model.RegistrationToken);
            if (isExistsToken)
            {
                return BadRequest("error_device_already_exists");
            }

            var deviceType = EDeviceType.Unknown;
            if (model.DeviceTypeName?.ToLower() == "ios")
            {
                deviceType = EDeviceType.iOS;
            }
            else if (model.DeviceTypeName?.ToLower() == "adnroid")
            {
                deviceType = EDeviceType.Android;
            }

            var userDevice = new UserDevice
            {
                RegistrationToken = model.RegistrationToken,
                DeviceType = deviceType,
                ScreenResolution = model.ScreenResolution,
                IsEnabled = true,
                BrowserVersion = model.BrowserVersion,
                UserId = user.Id
            };

            _userDeviceRepository.Add(userDevice);
            await _unitOfWork.SaveChangesAsync();

            return Ok();
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> ToggleNotification([FromRoute] int id)
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

            var userDevice = await _userDeviceRepository.GetByIdAsync(id);
            if (userDevice == null || userDevice.UserId != user.Id)
            {
                return BadRequest("error_device_not_found");
            }

            userDevice.IsEnabled = !userDevice.IsEnabled;
            _userDeviceRepository.Update(userDevice);
            await _unitOfWork.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
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

            var userDevice = await _userDeviceRepository.GetByIdAsync(id);
            if (userDevice == null || userDevice.UserId != user.Id)
            {
                return BadRequest("error_device_not_found");
            }

            _userDeviceRepository.Delete(userDevice);
            await _unitOfWork.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        [Route("sync")]
        public async Task<IActionResult> Sync([FromBody] UserDeviceRequestModel model)
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

            var deviceType = EDeviceType.Unknown;
            if (model.DeviceTypeName?.ToLower() == "ios")
            {
                deviceType = EDeviceType.iOS;
            }
            else if (model.DeviceTypeName?.ToLower() == "adnroid")
            {
                deviceType = EDeviceType.Android;
            }

            var userDevice = await _userDeviceRepository.GetQueryable().FirstOrDefaultAsync(x => x.RegistrationToken == model.RegistrationToken);
            if (userDevice == null)
            {
                userDevice = new UserDevice
                {
                    RegistrationToken = model.RegistrationToken,
                    DeviceType = deviceType,
                    ScreenResolution = model.ScreenResolution,
                    IsEnabled = true,
                    BrowserVersion = model.BrowserVersion,
                    UserId = user.Id,
                    UpdatedOnUtc = DateTime.UtcNow
                };

                _userDeviceRepository.Add(userDevice);
            }
            else
            {
                userDevice.DeviceType = deviceType;
                userDevice.ScreenResolution = model.ScreenResolution;
                userDevice.BrowserVersion = model.BrowserVersion;
                userDevice.UpdatedOnUtc = DateTime.UtcNow;
            }

            await _unitOfWork.SaveChangesAsync();

            return Ok();
        }
    }
}