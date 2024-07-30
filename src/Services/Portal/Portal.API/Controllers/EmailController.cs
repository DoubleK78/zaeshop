using Common.Interfaces.Messaging;
using Common.Shared.Models.Emails;
using Microsoft.AspNetCore.Mvc;
using Portal.API.Attributes;
using Portal.Domain.Interfaces.Business.Services;

namespace Portal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmailController : BaseApiController
    {
        private readonly ISendMailPublisher _sendMailPublisher;

        public EmailController(ISendMailPublisher sendMailPublisher)
        {
            _sendMailPublisher = sendMailPublisher;
        }

        [HttpPost]
        public async Task<IActionResult> SendMailAsync([FromBody] SendEmailMessage request)
        {
            var toEmail = string.Join(", ", request.ToEmails);
            var ccEmail = string.Join(", ", request.CcEmails ?? new List<string>());
            var message = new SendEmailMessage
            {
                Subject = request.Subject,
                Body = request.Body,
                ToEmails = toEmail!.Split(',').ToList(),
                CcEmails = ccEmail?.Split(',').ToList()
            };
            await _sendMailPublisher.SendMailAsync(message);
            return Ok();
        }
    }
}
