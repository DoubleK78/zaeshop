using Common.Enums;
using Common.Interfaces.Messaging;
using Common.Shared.Models.Emails;
using Common.Shared.Models.Logs;
using Common.ValueObjects;
using Microsoft.Extensions.Hosting;
using Portal.Domain.AggregatesModel.AlbumAggregate;
using Portal.Domain.AggregatesModel.TaskAggregate;
using Portal.Domain.AggregatesModel.UserAggregate;
using Portal.Domain.Enums;
using Portal.Domain.Interfaces.Business.Services;
using Portal.Domain.SeedWork;
using Portal.Infrastructure.Helpers;
using System;

namespace Portal.Infrastructure.Implements.Business.Services
{
    public class EmailService : IEmailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<User> _userRepository;
        private readonly ISendMailPublisher _sendMailPublisher;
        private readonly IServiceLogPublisher _serviceLogPublisher;
        private readonly IHostEnvironment _hostingEnvironment;

        public EmailService(
           IUnitOfWork unitOfWork,
           ISendMailPublisher sendMailPublisher,
           IServiceLogPublisher serviceLogPublisher,
           IHostEnvironment hostingEnvironment)
        {
            _unitOfWork = unitOfWork;
            _userRepository = unitOfWork.Repository<User>();
            _sendMailPublisher = sendMailPublisher;
            _serviceLogPublisher = serviceLogPublisher;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task SendEmailToFollowersTaskAsync()
        {
            bool isDeployed = bool.Parse(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT_DEPLOYED") ?? "false");
            var prefixEnvironment = isDeployed ? "[Docker] " : string.Empty;

            var scheduleJob = await _unitOfWork.Repository<HangfireScheduleJob>().GetByNameAsync(Const.HangfireJobName.SendEmailSPremiumFollowers);
            if (scheduleJob != null && scheduleJob.IsEnabled && !scheduleJob.IsRunning)
            {
                try
                {
                    scheduleJob.IsRunning = true;
                    scheduleJob.StartOnUtc = DateTime.UtcNow;
                    await _unitOfWork.SaveChangesAsync();

                    await SendEmailToFollowersAsync();

                    scheduleJob.EndOnUtc = DateTime.UtcNow;
                    scheduleJob.IsRunning = false;
                    await _unitOfWork.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    await _serviceLogPublisher.WriteLogAsync(new ServiceLogMessage
                    {
                        LogLevel = ELogLevel.Error,
                        EventName = ex.Message,
                        StackTrace = ex.StackTrace,
                        ServiceName = "Hangfire",
                        Environment = prefixEnvironment + _hostingEnvironment.EnvironmentName,
                        Description = $"[Exception]: {ex.Message}",
                        StatusCode = "Internal Server Error"
                    });

                    scheduleJob.EndOnUtc = DateTime.UtcNow;
                    scheduleJob.IsRunning = false;
                    await _unitOfWork.SaveChangesAsync();
                }
            }
        }

        private async Task SendEmailToFollowersAsync()
        {
            var followersSPremium = await _userRepository.GetQueryable()
                .Include(x => x.Followings).Where(x => x.RoleType == ERoleType.UserSuperPremium)
                .ToListAsync();

            foreach (var follower in followersSPremium)
            {
                var region = follower.Region;
                var toEmail = string.Join(", ", follower.Email);

                var bodyList = follower.Followings
                    .Select(comic => comic.Album)
                    .Select(album =>
                    {
                        var newChap = album.Collections
                            .Where(x => (DateTime.UtcNow - x.CreatedOnUtc).TotalHours <= 24)
                            .OrderByDescending(x => x.CreatedOnUtc)
                            .FirstOrDefault();

                        if (newChap == null)
                            return null;

                        var path = region switch
                        {
                            ERegion.en => "en/comics",
                            ERegion.vi => "truyen-tranh",
                            _ => ""
                        };

                        return $"{album.Title} : " +
                               $"{newChap?.Title}." +
                               $" Link: https://fastscans.net/{path}/{album.FriendlyName}/{newChap?.FriendlyName}\n";
                    })
                    .Where(content => content != null)
                    .ToList();

                if (bodyList.Count > 0)
                {
                    var subject = region switch
                    {
                        ERegion.en => "FAST SCANS FOLLOWING NOTIFICATION",
                        ERegion.vi => "FAST SCANS THÔNG BÁO THEO DÕI",
                        _ => ""
                    };

                    string body = string.Join("\n", bodyList);

                    var message = new SendEmailMessage
                    {
                        Subject = subject,
                        Body = body,
                        ToEmails = toEmail!.Split(',').ToList(),
                        CcEmails = null
                    };

                    await _sendMailPublisher.SendMailAsync(message);
                }
            }
        }
    }
}
