using Common;
using Common.Enums;
using Common.Interfaces;
using Common.Interfaces.Messaging;
using Common.Shared.Models.Logs;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Portal.API.Attributes;
using Portal.Domain.AggregatesModel.UserAggregate;
using Portal.Domain.Interfaces.Business.Services;
using Portal.Domain.Interfaces.External;
using Portal.Domain.Models.ImageUploadModels;

namespace Portal.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IApiService _apiService;
        private readonly IAmazonS3Service _amazonS3Service;
        private readonly ISendMailPublisher _sendMailPublisher;
        private readonly IServiceLogPublisher _serviceLogPublisher;
        private readonly IRedisService _redisService;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly IFirebaseCloudMessageService _firebaseCloudMessageService;

        public TestController(
            IUnitOfWork unitOfWork,
            IBackgroundJobClient backgroundJobClient,
            IApiService apiService,
            IAmazonS3Service amazonS3Service,
            ISendMailPublisher sendMailPublisher,
            IServiceLogPublisher serviceLogPublisher,
            IRedisService redisService,
            IUserService userService,
            IEmailService emailService,
            IFirebaseCloudMessageService firebaseCloudMessageService)
        {
            _unitOfWork = unitOfWork;
            _backgroundJobClient = backgroundJobClient;
            _apiService = apiService;
            _amazonS3Service = amazonS3Service;
            _sendMailPublisher = sendMailPublisher;
            _serviceLogPublisher = serviceLogPublisher;
            _redisService = redisService;
            _userService = userService;
            _emailService = emailService;
            _firebaseCloudMessageService = firebaseCloudMessageService;
        }

        [HttpGet]
        [Route("users")]
        [Authorize(ERoles.Administrator)]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _unitOfWork.Repository<User>().GetAllAsync();
            return Ok(users);
        }

        [HttpGet]
        [Route("users-mapping")]
        [Authorize(ERoles.Administrator)]
        public async Task<IActionResult> GetUserMapping()
        {
            var users = await _unitOfWork.Repository<User>().GetQueryable()
                .Filter(o => o.Id == 1)
                .Project(x => new { x.FullName, x.IdentityUserId })
                .ToListAsync();
            return Ok(users);
        }

        [HttpGet("portal-hangfire")]
        public IActionResult TestHangFire(string message)
        {
            _backgroundJobClient.Enqueue(() => Console.WriteLine(message));
            return Ok();
        }

        [HttpGet("identity-grpc-get-user")]
        public async Task<IActionResult> CallApiPortalAsync()
        {
            var result = await _apiService.GetAsync<object>(CommonHelper.GetServiceUrl(EServiceHost.Identity), "/v1/users");
            return Ok(result);
        }

        [HttpPost("bulk-upload")]
        // Limit 1 MB
        [RequestSizeLimit(1024 * 1024)]
        public async Task<IActionResult> BulkUploadAsync([FromForm] List<IFormFile> files)
        {
            // Validate and get data
            var listImages = new List<ImageUploadRequestModel>();
            foreach (var file in files)
            {
                var fileName = file.FileName;
                var fileBytes = GetFileBytes(file);

                listImages.Add(new ImageUploadRequestModel
                {
                    FileName = fileName,
                    ImageData = fileBytes
                });
            }

            var response = await _amazonS3Service.BulkUploadImagesAsync(listImages, "Test");
            return Ok(response);
        }

        private static byte[] GetFileBytes(IFormFile file)
        {
            using var ms = new MemoryStream();
            file.CopyTo(ms);
            return ms.ToArray();
        }

        [HttpPost("send-email")]
        public async Task<IActionResult> SendMailAsync(
            string subject,
            string body,
            string toEmails,
            string? ccEmails
        )
        {
            var message = new Common.Shared.Models.Emails.SendEmailMessage
            {
                Subject = subject,
                Body = body,
                ToEmails = toEmails.Split(',').ToList(),
                CcEmails = ccEmails?.Split(',').ToList()
            };
            await _sendMailPublisher.SendMailAsync(message);
            return Ok();
        }

        [HttpPost("service-log")]
        public async Task<IActionResult> CreateServiceLog(string eventName, string description, ELogLevel? logLevel)
        {
            await _serviceLogPublisher.WriteLogAsync(new ServiceLogMessage
            {
                LogLevel = logLevel ?? ELogLevel.Information,
                EventName = eventName,
                ServiceName = "Portal",
                Environment = "Test",
                Description = description
            });
            return Ok();
        }

        [HttpPost]
        [Route("clear-cache-key")]
        [Authorize(ERoles.Administrator)]
        public async Task<IActionResult> ClearCacheByKeyAsync(string key)
        {
            await _redisService.RemoveAsync(key);
            return Ok();
        }

        [HttpPost]
        [Route("reset-role")]
        [Authorize(ERoles.Administrator)]
        public async Task<IActionResult> ResetRoleUser()
        {
            await _userService.ResetRoleAsync();
            return Ok();
        }

        [HttpPost]
        [Route("noti-following")]
        [Authorize(ERoles.Administrator)]
        public async Task<IActionResult> SendMailToFollowers()
        {
            await _emailService.SendEmailToFollowersTaskAsync();
            return Ok();
        }

        [HttpGet]
        [Route("cache")]
        [Authorize(ERoles.Administrator)]
        public async Task<IActionResult> GetCacheValueByKey([FromQuery] string key)
        {
            var response = await _redisService.GetStringAsync(key);
            return Ok(response);
        }

        [HttpGet]
        [Route("remove-cache-pattern")]
        [Authorize(ERoles.Administrator)]
        public async Task<IActionResult> RemoveByPatternAsync([FromQuery] string pattern)
        {
            await _redisService.RemoveByPatternAsync(pattern);
            return Ok(pattern);
        }

        [HttpPost]
        [Route("reload-popular")]
        [Authorize(ERoles.Administrator)]
        public IActionResult ReloadPopular()
        {
            _backgroundJobClient.Enqueue<IBusinessCacheService>(x => x.ReloadCachePopularComicsAsync("vi"));
            return Ok();
        }

        [HttpPost]
        [Route("reload-recently")]
        [Authorize(ERoles.Administrator)]
        public IActionResult ReloadRecently()
        {
            _backgroundJobClient.Enqueue<IBusinessCacheService>(x => x.RelaodCacheRecentlyComicsAsync("vi"));
            return Ok();
        }

        [HttpPost]
        [Route("reload-top")]
        [Authorize(ERoles.Administrator)]
        public IActionResult ReloadTop()
        {
            _backgroundJobClient.Enqueue<IBusinessCacheService>(x => x.ReloadCacheTopComicsAsync("vi"));
            return Ok();
        }

        [HttpPost]
        [Route("push-notification")]
        [Authorize(ERoles.Administrator)]
        public IActionResult PushNotification(string registrationToken, string title, string description, string? clickAction = null)
        {
            _backgroundJobClient.Enqueue(() => _firebaseCloudMessageService.SendAsync(registrationToken, title, description, clickAction));
            return Ok();
        }

        [HttpPost]
        [Route("push-notification-all")]
        [Authorize(ERoles.Administrator)]
        public IActionResult PushNotificationAll(string registrationTokens, string title, string description, string? clickAction = null)
        {
            List<string> tokens = registrationTokens.Split(',').ToList();
            _backgroundJobClient.Enqueue(() => _firebaseCloudMessageService.SendAllAsync(tokens, title, description, clickAction));
            return Ok();
        }

        [HttpPost]
        [Route("remind-subscription")]
        [Authorize(ERoles.Administrator)]
        public IActionResult RemindSubscription()
        {
            _backgroundJobClient.Enqueue<IUserService>(x => x.RemindSubscriptionAsync());
            return Ok();
        }
    }
}
