using Common;
using Common.Enums;
using Common.Interfaces;
using Common.Interfaces.Messaging;
using Common.Shared.Models.Logs;
using Common.ValueObjects;
using Microsoft.Extensions.Hosting;
using Portal.Domain.AggregatesModel.AlbumAggregate;
using Portal.Domain.AggregatesModel.CollectionAggregate;
using Portal.Domain.AggregatesModel.TaskAggregate;
using Portal.Domain.AggregatesModel.UserAggregate;
using Portal.Domain.Enums;
using Portal.Domain.Interfaces.Business.Services;
using Portal.Domain.Models.LevelModels;
using Portal.Domain.SeedWork;
using Portal.Infrastructure.Helpers;

namespace Portal.Infrastructure.Implements.Business.Services
{
    public class LevelService : ILevelService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<Level> _levelRepository;
        private readonly IServiceLogPublisher _serviceLogPublisher;
        private readonly IHostEnvironment _hostingEnvironment;
        private readonly IGenericRepository<Collection> _collectionRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Comment> _commentRepository;
        private readonly IRedisService _redisService;


        public LevelService(
            IUnitOfWork unitOfWork,
            IServiceLogPublisher serviceLogPublisher,
            IHostEnvironment hostingEnvironment,
            IRedisService redisService)
        {
            _unitOfWork = unitOfWork;
            _levelRepository = unitOfWork.Repository<Level>();
            _serviceLogPublisher = serviceLogPublisher;
            _hostingEnvironment = hostingEnvironment;
            _collectionRepository = unitOfWork.Repository<Collection>();
            _userRepository = unitOfWork.Repository<User>();
            _commentRepository = unitOfWork.Repository<Comment>();
            _redisService = redisService;
        }

        private static int CalculateEarnExpFromViewOrComment(int? collectionId, int? albumId, int? commentId, ERoleType roleType)
        {
            // Bounus exp when user upgraded account
            int bonousExp = 0;
            if (roleType == ERoleType.UserPremium)
            {
                bonousExp = 10;
            }
            else if (roleType == ERoleType.UserSuperPremium)
            {
                bonousExp = 20;
            }

            // Case: User views a chapter then get 10 exp
            if (collectionId.HasValue && !albumId.HasValue && !commentId.HasValue)
            {
                return 10 + bonousExp;
            }
            // Case: User comments comic or chapter then get 15 exp
            else if ((collectionId.HasValue || albumId.HasValue) && commentId.HasValue)
            {
                return 15 + bonousExp;
            }

            return 0;
        }

        public async Task AddExperienceFromUserToRedisAsync(LevelBuildRedisRequestModel model)
        {
            bool isDeployed = bool.Parse(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT_DEPLOYED") ?? "false");
            var prefixEnvironment = isDeployed ? "[Docker] " : string.Empty;

            try
            {
                #region Validate model
                var user = await _userRepository.GetByIdentityUserIdAsync(model.IdentityUserId);
                if (user == null)
                {
                    // Log Error when model have user id not exists database
                    await _serviceLogPublisher.WriteLogAsync(new ServiceLogMessage
                    {
                        LogLevel = ELogLevel.Error,
                        EventName = Const.ServiceLogEventName.ErrorAddExp,
                        ServiceName = "Hangfire",
                        Environment = prefixEnvironment + _hostingEnvironment.EnvironmentName,
                        Description = $"User with IdentityUserId {model.IdentityUserId} not found",
                        IpAddress = model.IpAddress,
                        Request = JsonSerializationHelper.Serialize(model)
                    });
                    return;
                }

                Collection? collection = null;
                if (model.CollectionId.HasValue)
                {
                    collection = await _collectionRepository.GetByIdAsync(model.CollectionId.Value);
                    if (collection == null)
                    {
                        // Log Error when model have collection id not exists database
                        await _serviceLogPublisher.WriteLogAsync(new ServiceLogMessage
                        {
                            LogLevel = ELogLevel.Error,
                            EventName = Const.ServiceLogEventName.ErrorAddExp,
                            ServiceName = "Hangfire",
                            Environment = prefixEnvironment + _hostingEnvironment.EnvironmentName,
                            Description = $"Collection with id {model.CollectionId} not found",
                            IpAddress = model.IpAddress,
                            Request = JsonSerializationHelper.Serialize(model)
                        });
                        return;
                    }
                }

                Album? album = null;
                if (model.AlbumId.HasValue)
                {
                    album = await _unitOfWork.Repository<Album>().GetByIdAsync(model.AlbumId.Value);
                    if (album == null)
                    {
                        // Log Error when model have album id not exists database
                        await _serviceLogPublisher.WriteLogAsync(new ServiceLogMessage
                        {
                            LogLevel = ELogLevel.Error,
                            EventName = Const.ServiceLogEventName.ErrorAddExp,
                            ServiceName = "Hangfire",
                            Environment = prefixEnvironment + _hostingEnvironment.EnvironmentName,
                            Description = $"Album with id {model.AlbumId} not found",
                            IpAddress = model.IpAddress,
                            Request = JsonSerializationHelper.Serialize(model)
                        });
                        return;
                    }
                }

                Comment? comment = null;
                if (model.CommentId.HasValue)
                {
                    comment = await _commentRepository.GetByIdAsync(model.CommentId.Value);
                    if (comment == null)
                    {
                        // Log Error when model have comment id not exists database
                        await _serviceLogPublisher.WriteLogAsync(new ServiceLogMessage
                        {
                            LogLevel = ELogLevel.Error,
                            EventName = Const.ServiceLogEventName.ErrorAddExp,
                            ServiceName = "Hangfire",
                            Environment = prefixEnvironment + _hostingEnvironment.EnvironmentName,
                            Description = $"Comment with id {model.CommentId} not found",
                            IpAddress = model.IpAddress,
                            Request = JsonSerializationHelper.Serialize(model)
                        });
                        return;
                    }
                }
                #endregion

                #region Collection (User go next chapter)
                if (model.CollectionId.HasValue && collection != null)
                {
                    // We use key LevelCount_00 & LevelCount_30
                    var key = string.Format(Const.RedisCacheKey.LevelCount, DateTime.UtcNow.Minute - (DateTime.UtcNow.Minute % 30));
                    var value = await _redisService.GetAsync<List<LevelBuildRedisModel>>(key);
                    if (value == null)
                    {
                        value = new List<LevelBuildRedisModel>
                        {
                            new LevelBuildRedisModel
                            {
                                UserId = user.Id,
                                CollectionId = collection.Id,
                                CreatedOnUtc = DateTime.UtcNow,
                                IpAddress = model.IpAddress,
                                SessionId = model.SessionId,
                                IsViewedNewChapter = model.IsViewedNewChapter
                            }
                        };

                        // Stored cache to 50 minutes
                        await _redisService.SetAsync(key, value, 50);
                    }
                    else
                    {
                        // Example: Chapter 1 -> next -> chapter 2 -> Stored: [1]
                        // Chapter 2 -> next -> Chapter 3 -> -> Stored: [1, 2] (Condition: chapter 2 go next each chapter for above 15s)
                        // If exists then skip, other 30 minutes can be again stored
                        bool isValidNextChapter = false;

                        var levelBuildRedisModel = value.Find(o => o.CollectionId == model.CollectionId && !o.CommentId.HasValue && o.UserId == user?.Id);

                        var lastNextChapterEvent = value
                            .Where(o => !o.CommentId.HasValue && o.UserId == user?.Id)
                            .OrderByDescending(o => o.CreatedOnUtc)
                            .FirstOrDefault()?.CreatedOnUtc;

                        if (lastNextChapterEvent == null ||
                        (levelBuildRedisModel == null &&
                            model.CreatedOnUtc.Subtract(lastNextChapterEvent.Value).TotalSeconds > 15))
                        {
                            isValidNextChapter = true;
                        }

                        // Condition: User go next each chapter for 15s
                        if (isValidNextChapter)
                        {
                            value.Add(new LevelBuildRedisModel
                            {
                                UserId = user.Id,
                                CollectionId = collection.Id,
                                CreatedOnUtc = DateTime.UtcNow,
                                IpAddress = model.IpAddress,
                                SessionId = model.SessionId,
                                IsViewedNewChapter = model.IsViewedNewChapter
                            });

                            // Stored cache to 50 minutes
                            await _redisService.SetAsync(key, value, 50);
                        }
                    }
                }
                #endregion

                #region Comment (User comment on comic or collection)
                if (model.CommentId.HasValue && comment != null && (model.AlbumId.HasValue || model.CollectionId.HasValue))
                {
                    // Same logic #region Collection (User go next chapter)
                    // We use key LevelCount_00 & LevelCount_30
                    var key = string.Format(Const.RedisCacheKey.LevelCount, DateTime.UtcNow.Minute - (DateTime.UtcNow.Minute % 30));
                    var value = await _redisService.GetAsync<List<LevelBuildRedisModel>>(key);
                    if (value == null)
                    {
                        value = new List<LevelBuildRedisModel>
                        {
                            new LevelBuildRedisModel
                            {
                                UserId = user.Id,
                                AlbumId = album?.Id,
                                CollectionId = collection?.Id,
                                CommentId = comment.Id,
                                CreatedOnUtc = DateTime.UtcNow,
                                IpAddress = model.IpAddress,
                                SessionId = model.SessionId
                            }
                        };

                        // Stored cache to 50 minutes
                        await _redisService.SetAsync(key, value, 50);
                    }
                    else
                    {
                        // Same logic #region Collection (User go next chapter)
                        // Example: Comment Chapter 1 -> chapter 2 -> Stored: [1]
                        // Chapter 2  -> Chapter 3 -> -> Stored: [1, 2] (Condition: chapter 2 go next each chapter for above 15s)
                        // Album is also same logic
                        // If exists then skip, other 30 minutes can be again stored
                        bool isValidEarnFromComment = false;

                        var levelBuildRedisModel = value.Find(o => o.CommentId == model.CommentId && (
                            o.AlbumId == album?.Id || o.CollectionId == collection?.Id
                        ) && o.UserId == user?.Id);

                        var lastNextChapterEvent = value
                            .Where(o => o.CommentId.HasValue && (
                                o.AlbumId == album?.Id || o.CollectionId == collection?.Id
                            ) && o.UserId == user?.Id)
                            .OrderByDescending(o => o.CreatedOnUtc)
                            .FirstOrDefault()?.CreatedOnUtc;

                        if (lastNextChapterEvent == null ||
                        (levelBuildRedisModel == null &&
                            model.CreatedOnUtc.Subtract(lastNextChapterEvent.Value).TotalSeconds > 15))
                        {
                            isValidEarnFromComment = true;
                        }

                        // Condition: User go next each chapter for 15s
                        if (isValidEarnFromComment)
                        {
                            value.Add(new LevelBuildRedisModel
                            {
                                UserId = user.Id,
                                AlbumId = album?.Id,
                                CollectionId = collection?.Id,
                                CommentId = comment.Id,
                                CreatedOnUtc = DateTime.UtcNow,
                                IpAddress = model.IpAddress,
                                SessionId = model.SessionId
                            });

                            // Stored cache to 50 minutes
                            await _redisService.SetAsync(key, value, 50);
                        }
                    }
                }
                #endregion
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
                    IpAddress = model.IpAddress,
                    StatusCode = "Internal Server Error",
                    Request = JsonSerializationHelper.Serialize(model)
                });
            }
        }

        public async Task CalculateExperiencesFromRedisTaskAsync()
        {
            bool isDeployed = bool.Parse(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT_DEPLOYED") ?? "false");
            var prefixEnvironment = isDeployed ? "[Docker] " : string.Empty;

            var scheduleJob = await _unitOfWork.Repository<HangfireScheduleJob>().GetByNameAsync(Const.HangfireJobName.CalculateExperiencesFromRedis);
            if (scheduleJob != null && scheduleJob.IsEnabled && !scheduleJob.IsRunning)
            {
                try
                {
                    scheduleJob.IsRunning = true;
                    scheduleJob.StartOnUtc = DateTime.UtcNow;
                    await _unitOfWork.SaveChangesAsync();

                    await CalculateExperiencesFromRedis();

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

        private async Task CalculateExperiencesFromRedis()
        {
            bool isDeployed = bool.Parse(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT_DEPLOYED") ?? "false");
            var prefixEnvironment = isDeployed ? "[Docker] " : string.Empty;

            // Get redis data from 30 minutes ago
            var key = string.Format(Const.RedisCacheKey.ViewCount, DateTime.UtcNow.Minute - (DateTime.UtcNow.Minute % 30) - 30);
            var value = await _redisService.GetAsync<List<LevelBuildRedisModel>>(key);
            if (value != null && value.Count != 0)
            {
                var userIds = value.Select(x => x.UserId).Distinct();
                var users = await _userRepository.GetQueryable().Where(o => userIds.Contains(o.Id)).ToListAsync();
                var userLevels = await _unitOfWork.Repository<UserLevel>().GetQueryable()
                    .Where(x => userIds.Contains(x.UserId) && (x.Date == value[0].CreatedOnUtc.Date)).ToListAsync();

                var level = await _levelRepository.GetQueryable().OrderBy(o => o.TargetExp).ToListAsync();
                var baseLevel = level[0];

                foreach (var item in value)
                {
                    var user = users.Find(o => o.Id == item.UserId);
                    if (user == null) continue;

                    var userLevel = userLevels.Find(x => x.UserId == item.UserId);

                    if (userLevel == null)
                    {
                        var additionalInformations = new List<LevelAdditionalInformation>
                        {
                            new LevelAdditionalInformation
                            {
                                RoleType = user.RoleType,
                                AlbumId = item.AlbumId,
                                CollectionId = item.CollectionId,
                                CommentId = item.CommentId,
                                IpAddress = item.IpAddress,
                                SessionId = item.SessionId,
                                CreatedOnUtc = DateTime.UtcNow,
                                IsViewedNewChapter = item.IsViewedNewChapter
                            }
                        };

                        _unitOfWork.Repository<UserLevel>().Add(new UserLevel
                        {
                            LevelId = user.LevelId ?? baseLevel.Id,
                            UserId = user.Id,
                            CurrentExp = user.CurrentExp,
                            Exp = CalculateEarnExpFromViewOrComment(item.CollectionId, item.AlbumId, item.CommentId, user.RoleType),
                            IpAddress = item.IpAddress,
                            SessionId = item.SessionId,
                            Date = item.CreatedOnUtc.Date,
                            AdditionalInformation = JsonSerializationHelper.Serialize(additionalInformations)
                        });
                    }
                    else
                    {
                        userLevel.Exp += CalculateEarnExpFromViewOrComment(item.CollectionId, item.AlbumId, item.CommentId, user.RoleType);

                        #region Update lastest IP and stored Previous IPs
                        if (!string.IsNullOrEmpty(item.IpAddress))
                        {
                            // Case 1 (Optional): When Additional Information empty, then create new records.
                            if (string.IsNullOrEmpty(userLevel.AdditionalInformation))
                            {
                                var additionalInformations = new List<LevelAdditionalInformation>
                                {
                                    new LevelAdditionalInformation
                                    {
                                        RoleType = user.RoleType,
                                        AlbumId = item.AlbumId,
                                        CollectionId = item.CollectionId,
                                        CommentId = item.CommentId,
                                        IpAddress = item.IpAddress,
                                        SessionId = item.SessionId,
                                        CreatedOnUtc = DateTime.UtcNow,
                                        IsViewedNewChapter = item.IsViewedNewChapter
                                    }
                                };
                                userLevel.AdditionalInformation = JsonSerializationHelper.Serialize(additionalInformations);
                            }
                            else
                            {
                                // Case 2: Push a new record in exist Additional Information
                                try
                                {
                                    var additionalInformations = JsonSerializationHelper.Deserialize<List<LevelAdditionalInformation>>(userLevel.AdditionalInformation);
                                    if (additionalInformations != null)
                                    {
                                        additionalInformations.Add(new LevelAdditionalInformation
                                        {
                                            RoleType = user.RoleType,
                                            AlbumId = item.AlbumId,
                                            CollectionId = item.CollectionId,
                                            CommentId = item.CommentId,
                                            IpAddress = item.IpAddress,
                                            SessionId = item.SessionId,
                                            CreatedOnUtc = DateTime.UtcNow,
                                            IsViewedNewChapter = item.IsViewedNewChapter
                                        });
                                        userLevel.AdditionalInformation = JsonSerializationHelper.Serialize(additionalInformations);
                                    }
                                }
                                // Case (Bad Data): Then we use from case 1
                                catch
                                {
                                    var additionalInformations = new List<LevelAdditionalInformation>
                                    {
                                       new LevelAdditionalInformation
                                       {
                                           RoleType = user.RoleType,
                                           AlbumId = item.AlbumId,
                                           CollectionId = item.CollectionId,
                                           CommentId = item.CommentId,
                                           IpAddress = item.IpAddress,
                                           SessionId = item.SessionId,
                                           CreatedOnUtc = DateTime.UtcNow,
                                           IsViewedNewChapter = item.IsViewedNewChapter
                                       }
                                    };
                                    userLevel.AdditionalInformation = JsonSerializationHelper.Serialize(additionalInformations);
                                }
                            }

                            userLevel.IpAddress = item.IpAddress;
                            userLevel.SessionId = item.SessionId;
                        }

                        _unitOfWork.Repository<UserLevel>().Update(userLevel);
                        #endregion
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                // Re-calculate Exps from users
                var parameters = new Dictionary<string, object?>
                {
                    { "userIds",  string.Join(',', userIds)}
                };
                await _unitOfWork.ExecuteAsync("User_RecalculateExperience", parameters);

                // Log to service log to stored
                await _serviceLogPublisher.WriteLogAsync(new ServiceLogMessage
                {
                    LogLevel = ELogLevel.Information,
                    EventName = Const.ServiceLogEventName.StoredExpCache,
                    ServiceName = "Hangfire",
                    Environment = prefixEnvironment + _hostingEnvironment.EnvironmentName,
                    Description = $"Stored total views from redis cache. At {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}",
                    Request = JsonSerializationHelper.Serialize(value)
                });
            }
        }
    }
}
