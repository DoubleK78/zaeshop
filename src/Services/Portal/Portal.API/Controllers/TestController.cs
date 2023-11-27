using Common;
using Common.Enums;
using Common.Interfaces;
using Common.Interfaces.Messaging;
using Common.Shared.Models.Logs;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Portal.API.Attributes;
using Portal.Domain.AggregatesModel.UserAggregate;
using Portal.Domain.Interfaces.External;
using Portal.Domain.Interfaces.Infrastructure;
using Portal.Domain.Interfaces.Messaging;
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
        private readonly IHelloWorldPublisher _helloWorldSender;
        private readonly ISendMailPublisher _sendMailPublisher;
        private readonly IServiceLogPublisher _serviceLogPublisher;
        private readonly IElasticsearchService _elasticsearchService;

        public TestController(
            IUnitOfWork unitOfWork,
            IBackgroundJobClient backgroundJobClient,
            IApiService apiService,
            IAmazonS3Service amazonS3Service,
            IHelloWorldPublisher helloWorldSender,
            ISendMailPublisher sendMailPublisher,
            IServiceLogPublisher serviceLogPublisher,
            IElasticsearchService elasticsearchService)
        {
            _unitOfWork = unitOfWork;
            _backgroundJobClient = backgroundJobClient;
            _apiService = apiService;
            _amazonS3Service = amazonS3Service;
            _helloWorldSender = helloWorldSender;
            _sendMailPublisher = sendMailPublisher;
            _serviceLogPublisher = serviceLogPublisher;
            _elasticsearchService = elasticsearchService;
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

        // API send message from helloworldsender
        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessageAsync(string message)
        {
            await _helloWorldSender.SendAsync(message);
            return Ok();
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

        [HttpPost("elasticsearch")]
        public async Task<IActionResult> TestElasticsearchSyncAlbumsAsync()
        {
            await _elasticsearchService.SyncAlbumsAsync();
            return Ok();
        }

        [HttpDelete("elasticsearch")]
        public async Task<IActionResult> DeleteElasticsearchAsync()
        {
            await _elasticsearchService.ResetIndexes();
            return Ok();
        }

        [HttpGet("elasticsearch")]
        public async Task<IActionResult> GetElasticsearchSyncAlbumsAsync()
        {
            var response = await _elasticsearchService.GetDocumentsAsync();
            return Ok(response);
        }
    }
}
