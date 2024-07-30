
namespace Portal.Domain.Interfaces.Business.Services
{
    public interface IEmailService
    {
        Task SendEmailToFollowersTaskAsync();
    }
}
