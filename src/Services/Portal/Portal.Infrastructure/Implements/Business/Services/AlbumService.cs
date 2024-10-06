using Common;
using Common.Enums;
using Common.Interfaces;
using Common.Interfaces.Messaging;
using Common.Models;
using Common.Shared.Models.Logs;
using Common.ValueObjects;
using Microsoft.Extensions.Hosting;
using Portal.Domain.AggregatesModel.AlbumAggregate;
using Portal.Domain.AggregatesModel.TaskAggregate;
using Portal.Domain.Enums;
using Portal.Domain.Interfaces.Business.Services;
using Portal.Domain.Models.AlbumModels;
using Portal.Domain.SeedWork;
using Portal.Infrastructure.Helpers;

namespace Portal.Infrastructure.Implements.Business.Services
{
    public class AlbumService : IAlbumService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<Album> _repository;
        private readonly IGenericRepository<AlbumAlertMessage> _albumAlertMessageRepository;
        private readonly IGenericRepository<ContentType> _contentTypeRepository;
        private readonly IGenericRepository<ScheduleAlbum> _scheduleRepository;
        private readonly IServiceLogPublisher _serviceLogPublisher;
        private readonly IHostEnvironment _hostingEnvironment;
        private readonly IRedisService _redisService;
        private readonly IBusinessCacheService _businessCacheService;

        public AlbumService(
            IUnitOfWork unitOfWork,
            IServiceLogPublisher serviceLogPublisher,
            IHostEnvironment hostingEnvironment,
            IRedisService redisService,
            IBusinessCacheService businessCacheService)
        {
            _unitOfWork = unitOfWork;
            _repository = unitOfWork.Repository<Album>();
            _albumAlertMessageRepository = unitOfWork.Repository<AlbumAlertMessage>();
            _contentTypeRepository = unitOfWork.Repository<ContentType>();
            _scheduleRepository = unitOfWork.Repository<ScheduleAlbum>();
            _serviceLogPublisher = serviceLogPublisher;
            _hostingEnvironment = hostingEnvironment;
            _redisService = redisService;
            _businessCacheService = businessCacheService;
        }

        public async Task<ServiceResponse<AlbumResponseModel>> CreateAsync(AlbumRequestModel requestModel)
        {
            requestModel.Title = requestModel.Title.Trim();
            requestModel.Description = requestModel.Description?.Trim();

            // Validate
            if (await DoesTitleExistAsync(requestModel.Title))
            {
                return new ServiceResponse<AlbumResponseModel>("error_album_already_exists");
            }

            if (requestModel.AlbumAlertMessageId != null)
            {
                var albumAlertMessage = await _albumAlertMessageRepository.GetByIdAsync(requestModel.AlbumAlertMessageId.Value);
                if (albumAlertMessage == null)
                {
                    return new ServiceResponse<AlbumResponseModel>("error_album_alert_message_not_found");
                }
            }

            List<string>? contentTypeNames = null;
            if (requestModel.ContentTypeIds?.Count > 0)
            {
                // Check Db same as request ids
                contentTypeNames = await _contentTypeRepository.GetQueryable()
                                            .Where(x => requestModel.ContentTypeIds.Contains(x.Id))
                                            .Select(y => y.Name)
                                            .ToListAsync();
                if (contentTypeNames.Count != requestModel.ContentTypeIds.Count)
                {
                    return new ServiceResponse<AlbumResponseModel>("error_album_content_type_not_found");
                }
            }

            // Map request model to entity
            var entity = new Album
            {
                Title = requestModel.Title,
                Description = requestModel.Description,
                AlbumAlertMessageId = requestModel.AlbumAlertMessageId,
                FriendlyName = CommonHelper.GenerateFriendlyName(requestModel.Title),
                Region = requestModel.Region == "en" ? ERegion.en : ERegion.vi,
                LevelPublic = ELevelPublic.Partner
            };

            if (requestModel.IsPublic.HasValue)
            {
                entity.IsPublic = requestModel.IsPublic.Value;
            }

            if (requestModel.ContentTypeIds?.Count > 0)
            {
                entity.AlbumContentTypes = requestModel.ContentTypeIds.ConvertAll(id => new AlbumContentType
                {
                    ContentTypeId = id
                });
            }

            // We can upload thumbnail
            if (!string.IsNullOrEmpty(requestModel.FileNameThumbnail))
            {
                var relativePath = $"{entity.FriendlyName}/thumbnail";
                entity.ThumbnailUrl = $"https://s1.fastscans.net/{relativePath}/{requestModel.FileNameThumbnail}";
                entity.CdnThumbnailUrl = $"https://s1.fastscans.net/{relativePath}/{requestModel.FileNameThumbnail}";
            }

            // We can upload background
            if (!string.IsNullOrEmpty(requestModel.FileNameBackground))
            {
                var relativePath = $"{entity.FriendlyName}/background";
                entity.OriginalUrl = $"https://s1.fastscans.net/{relativePath}/{requestModel.FileNameBackground}";
                entity.CdnOriginalUrl = $"https://s1.fastscans.net/{relativePath}/{requestModel.FileNameBackground}";
            }

            _repository.Add(entity);
            await _unitOfWork.SaveChangesAsync();

            // Remove cache Comic Paging
            await _businessCacheService.RelaodCacheRecentlyComicsAsync(entity.Region.ToString());

            // Map entity to response model
            var responseModel = new AlbumResponseModel
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                AlbumAlertMessageName = entity.AlbumAlertMessage?.Name,
                ContentTypeNames = contentTypeNames.JoinSeparator(),
                Region = entity.Region,
                CreatedDate = entity.CreatedOnUtc
            };

            return new ServiceResponse<AlbumResponseModel>(responseModel);
        }

        public async Task<ServiceResponse<AlbumResponseModel>> UpdateAsync(int id, AlbumRequestModel requestModel)
        {
            requestModel.Title = requestModel.Title.Trim();
            requestModel.Description = requestModel.Description?.Trim();

            // Get existing entity
            var existingAlbum = await _repository.GetByIdAsync(id);
            if (existingAlbum == null)
            {
                return new ServiceResponse<AlbumResponseModel>("error_album_not_found");
            }

            // Validate
            if (existingAlbum.Title != requestModel.Title && await DoesTitleExistAsync(requestModel.Title))
            {
                return new ServiceResponse<AlbumResponseModel>("error_album_already_exists");
            }

            if (requestModel.AlbumAlertMessageId != null)
            {
                var albumAlertMessage = await _albumAlertMessageRepository.GetByIdAsync(requestModel.AlbumAlertMessageId.Value);
                if (albumAlertMessage == null)
                {
                    return new ServiceResponse<AlbumResponseModel>("error_album_alert_message_not_found");
                }
            }

            List<string>? contentTypeNames = null;
            if (requestModel.ContentTypeIds?.Count > 0)
            {
                // Check Db same as request ids
                contentTypeNames = await _contentTypeRepository.GetQueryable()
                                            .Where(x => requestModel.ContentTypeIds.Contains(x.Id))
                                            .Select(y => y.Name)
                                            .ToListAsync();
                if (contentTypeNames.Count != requestModel.ContentTypeIds.Count)
                {
                    return new ServiceResponse<AlbumResponseModel>("error_album_content_type_not_found");
                }
            }

            // Update properties
            existingAlbum.Title = requestModel.Title;
            existingAlbum.Description = requestModel.Description;
            existingAlbum.AlbumAlertMessageId = requestModel.AlbumAlertMessageId;
            existingAlbum.FriendlyName = CommonHelper.GenerateFriendlyName(requestModel.Title);
            existingAlbum.OriginalUrl = requestModel.OriginalUrl;
            existingAlbum.Region = requestModel.Region == "en" ? ERegion.en : ERegion.vi;

            if (requestModel.IsPublic.HasValue)
            {
                existingAlbum.IsPublic = requestModel.IsPublic.Value;
            }

            // Create or Update ContentType
            if (requestModel.ContentTypeIds?.Count > 0)
            {
                foreach (var contentTypeId in requestModel.ContentTypeIds)
                {
                    var existingAlbumContentType = existingAlbum.AlbumContentTypes.FirstOrDefault(x => x.ContentTypeId == contentTypeId);
                    if (existingAlbumContentType == null)
                    {
                        existingAlbum.AlbumContentTypes.Add(new AlbumContentType
                        {
                            ContentTypeId = contentTypeId
                        });
                    }
                }

                foreach (var existingNotExistContentType in existingAlbum.AlbumContentTypes.Where(x => !requestModel.ContentTypeIds.Contains(x.ContentTypeId)).ToList())
                {
                    existingAlbum.AlbumContentTypes.Remove(existingNotExistContentType);
                }
            }
            else
            {
                foreach (var existingAlbumContentType in existingAlbum.AlbumContentTypes.ToList())
                {
                    existingAlbum.AlbumContentTypes.Remove(existingAlbumContentType);
                }
            }

            // We can upload thumbnail
            if (requestModel.IsUpdateThumbnail && !string.IsNullOrEmpty(requestModel.FileNameThumbnail))
            {
                var relativePath = $"{existingAlbum.FriendlyName}/thumbnail";
                existingAlbum.ThumbnailUrl = $"https://s1.fastscans.net/{relativePath}/{requestModel.FileNameThumbnail}";
                existingAlbum.CdnThumbnailUrl = $"https://s1.fastscans.net/{relativePath}/{requestModel.FileNameThumbnail}";
            }
            else if (requestModel.IsUpdateThumbnail)
            {
                existingAlbum.ThumbnailUrl = null;
                existingAlbum.CdnThumbnailUrl = null;

                // Feature we should remove image no longer use
            }

            // We can upload background
            if (requestModel.IsUpdateBackground && !string.IsNullOrEmpty(requestModel.FileNameBackground))
            {
                var relativePath = $"{existingAlbum.FriendlyName}/background";
                existingAlbum.OriginalUrl = $"https://s1.fastscans.net/{relativePath}/{requestModel.FileNameBackground}";
                existingAlbum.CdnOriginalUrl = $"https://s1.fastscans.net/{relativePath}/{requestModel.FileNameBackground}";
            }
            else if (requestModel.IsUpdateBackground)
            {
                existingAlbum.OriginalUrl = null;
                existingAlbum.CdnThumbnailUrl = null;

                // Feature we should remove image no longer use
            }

            // Update
            _repository.Update(existingAlbum);
            await _unitOfWork.SaveChangesAsync();

            // Remove cache Comic Paging
            await _businessCacheService.RelaodCacheRecentlyComicsAsync(existingAlbum.Region.ToString());

            // Map to response
            var responseModel = new AlbumResponseModel
            {
                Id = existingAlbum.Id,
                Title = existingAlbum.Title,
                Description = existingAlbum.Description,
                AlbumAlertMessageName = existingAlbum.AlbumAlertMessage?.Name,
                ContentTypeNames = contentTypeNames.JoinSeparator(),
                Region = existingAlbum.Region,
                CreatedDate = existingAlbum.CreatedOnUtc
            };

            return new ServiceResponse<AlbumResponseModel>(responseModel);
        }

        public async Task<ServiceResponse<List<AlbumResponseModel>>> GetAllAsync()
        {
            var albums = await _repository.GetAllAsync();

            var response = albums.Select(x => new AlbumResponseModel
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                AlbumAlertMessageName = x.AlbumAlertMessage?.Name,
                ContentTypeNames = string.Join(", ", x.AlbumContentTypes.Select(y => y.ContentType.Name)),
                CreatedDate = x.CreatedOnUtc,
                CdnThumbnailUrl = x.CdnThumbnailUrl,
                CdnOriginalUrl = x.CdnOriginalUrl
            }).ToList();

            return new ServiceResponse<List<AlbumResponseModel>>(response);
        }

        public async Task<ServiceResponse<bool>> DeleteAsync(int id)
        {
            // Get existing entity
            var existingAlbum = await _repository.GetByIdAsync(id);
            if (existingAlbum == null)
                return new ServiceResponse<bool>("error_album_not_found");

            _repository.Delete(existingAlbum);
            await _unitOfWork.SaveChangesAsync();

            // Remove cache Comic Paging
            await _businessCacheService.ReloadCacheHomePageAsync(existingAlbum.Region.ToString());

            return new ServiceResponse<bool>(true);
        }

        // Other private methods
        private async Task<bool> DoesTitleExistAsync(string title)
        {
            return await _repository.GetQueryable().AnyAsync(x => x.Title == title);
        }

        public async Task<ServiceResponse<PagingCommonResponse<AlbumPagingResponse>>> GetPagingAsync(PagingCommonRequest request, FilterAdvanced filter)
        {
            ERegion regionEnum = new ERegion();
            if (filter.Region != null)
                regionEnum = (ERegion)Enum.Parse(typeof(ERegion), filter.Region);

            var parameters = new Dictionary<string, object?>
            {
                { "PageNumber", request.PageNumber },
                { "PageSize", request.PageSize },
                { "SearchTerm", request.SearchTerm },
                { "SortColumn", request.SortColumn },
                { "SortDirection", request.SortDirection },
                { "FirstChar", filter.FirstChar },
                { "Language", filter.Language },
                { "Country", filter.Country },
                { "Genre", filter.Genre },
                { "Status", filter.Status },
                { "Year", filter.Year },
                { "TopType", filter.TopType },
                { "Region", regionEnum }
            };
            var result = await _unitOfWork.QueryAsync<AlbumPagingResponse>("Album_All_Paging", parameters, commandTimeout: 180);

            var record = result.Find(o => o.IsTotalRecord);
            if (record == null)
            {
                return new ServiceResponse<PagingCommonResponse<AlbumPagingResponse>>(new PagingCommonResponse<AlbumPagingResponse>
                {
                    RowNum = 0,
                    Data = new List<AlbumPagingResponse>()
                });
            }

            result.Remove(record);
            return new ServiceResponse<PagingCommonResponse<AlbumPagingResponse>>(new PagingCommonResponse<AlbumPagingResponse>
            {
                RowNum = record.RowNum,
                Data = result
            });
        }

        public async Task<ServiceResponse<AlbumResponseModel>> GetByIdAsync(int id)
        {
            var album = await _repository.GetByIdAsync(id);
            if (album == null)
            {
                return new ServiceResponse<AlbumResponseModel>("error_album_not_found");
            }

            var albumResponse = new AlbumResponseModel
            {
                Id = album.Id,
                Title = album.Title,
                Description = album.Description,
                AlbumAlertMessageId = album.AlbumAlertMessageId,
                AlbumAlertMessageName = album.AlbumAlertMessage?.Name,
                ContentTypeIds = album.AlbumContentTypes?.Select(x => x.ContentTypeId).ToList(),
                // Set other properties as needed
                CreatedDate = album.CreatedOnUtc,
                UpdatedDate = album.UpdatedOnUtc,
                IsPublic = album.IsPublic,
                CdnThumbnailUrl = album.CdnThumbnailUrl,
                CdnOriginalUrl = album.CdnOriginalUrl,
                FriendlyName = album.FriendlyName,
                Region = album.Region
            };

            return new ServiceResponse<AlbumResponseModel>(albumResponse);
        }

        public async Task<ServiceResponse<AlbumExtraInfoModel>> GetExtraInfoByIdAsync(int id)
        {
            var album = await _repository.GetByIdAsync(id);
            if (album == null)
            {
                return new ServiceResponse<AlbumExtraInfoModel>("error_album_not_found");
            }

            var albumResponse = new AlbumExtraInfoModel
            {
                Id = album.Id,
                AlternativeName = album.AlternativeName,
                Type = album.Type,
                AlbumStatus = album.AlbumStatus,
                ReleaseYear = album.ReleaseYear,
                AuthorNames = album.AuthorNames,
                ArtistNames = album.ArtitstNames,
                Tags = album.Tags,
            };

            return new ServiceResponse<AlbumExtraInfoModel>(albumResponse);
        }

        public async Task<ServiceResponse<AlbumExtraInfoModel>> UpdateExtraInfoByIdAsync(int id, AlbumExtraInfoModel requestModel)
        {
            var album = await _repository.GetByIdAsync(id);
            if (album == null)
            {
                return new ServiceResponse<AlbumExtraInfoModel>("error_album_not_found");
            }

            // Update fields
            album.AlternativeName = requestModel.AlternativeName;
            album.Type = requestModel.Type;
            album.AlbumStatus = requestModel.AlbumStatus;
            album.ReleaseYear = requestModel.ReleaseYear;
            album.AuthorNames = requestModel.AuthorNames;
            album.ArtitstNames = requestModel.ArtistNames;
            album.Tags = requestModel.Tags;

            // Save changes
            await _unitOfWork.SaveChangesAsync();

            requestModel.Id = album.Id;
            return new ServiceResponse<AlbumExtraInfoModel>(requestModel);
        }

        public async Task ResetLevelPublicTaskAsync()
        {
            bool isDeployed = bool.Parse(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT_DEPLOYED") ?? "false");
            var prefixEnvironment = isDeployed ? "[Docker] " : string.Empty;

            var scheduleJob = await _unitOfWork.Repository<HangfireScheduleJob>().GetByNameAsync(Const.HangfireJobName.ResetLevelPublic);
            if (scheduleJob != null && scheduleJob.IsEnabled && !scheduleJob.IsRunning)
            {
                try
                {
                    scheduleJob.IsRunning = true;
                    scheduleJob.StartOnUtc = DateTime.UtcNow;
                    await _unitOfWork.SaveChangesAsync();

                    await ResetLevelPublicAsync();

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

        // Reset Level Public
        private async Task<ServiceResponse<bool>> ResetLevelPublicAsync()
        {
            var albums = await _repository.GetQueryable().Where(x => x.LevelPublic != ELevelPublic.AllUser).ToListAsync();

            if (albums == null || albums.Count == 0)
                return new ServiceResponse<bool>("error_reset_level_public");

            var albumFriendlyNames = new List<string?>();
            foreach (var album in albums)
            {
                TimeSpan difference = DateTime.UtcNow - album.CreatedOnUtc;

                if (album.LevelPublic == ELevelPublic.Partner && difference.TotalMinutes >= 15)
                {
                    album.LevelPublic = ELevelPublic.SPremiumUser;
                    albumFriendlyNames.Add(album.FriendlyName);
                }

                if (album.LevelPublic == ELevelPublic.SPremiumUser && difference.TotalHours >= 4 && difference.TotalHours < 15)
                {
                    album.LevelPublic = ELevelPublic.PremiumUser;
                    albumFriendlyNames.Add(album.FriendlyName);
                }

                if ((album.LevelPublic == ELevelPublic.SPremiumUser || album.LevelPublic == ELevelPublic.PremiumUser) &&
                     difference.TotalHours >= 15)
                {
                    album.LevelPublic = ELevelPublic.AllUser;
                    albumFriendlyNames.Add(album.FriendlyName);
                }
            }

            await _unitOfWork.BulkSaveChangesAsync();

            // Remove cache Comic Paging
            if (albumFriendlyNames.Count > 0)
            {
                foreach (var friendlyName in albumFriendlyNames)
                {
                    await _redisService.RemoveAsync(string.Format(Const.RedisCacheKey.ComicDetail, friendlyName));
                }

                // Remove cache Comic Paging
                await _businessCacheService.RelaodCacheRecentlyComicsAsync("vi");
                await _businessCacheService.RelaodCacheRecentlyComicsAsync("en");
            }

            return new ServiceResponse<bool>(true);
        }

        public async Task<ServiceResponse<List<AlbumScheduleResponseModel>>> GetScheduleAsync(AlbumScheduleRequestModel requestModel)
        {
            var albumsSchedule = await _scheduleRepository.GetQueryable()
                .Where(x => x.Region == requestModel.Region && x.DateRelease == requestModel.DateRelease)
                .OrderBy(x => x.TimeRelease)
                .ToListAsync();

            var albumsScheduleResponse = albumsSchedule.Select(x => new AlbumScheduleResponseModel
            {
                Id = x.Id,
                Title = x.Title,
                TimeRelease = x.TimeRelease,
                Type = x.Type,
                BackgroundUrl = x.BackgroundUrl,
                Url = x.Url,
                DateRelease = x.DateRelease,
                Region = x.Region,
                Status = x.Status
            }).ToList();

            return new ServiceResponse<List<AlbumScheduleResponseModel>>(albumsScheduleResponse);
        }

        public async Task<ServiceResponse<bool>> CreateScheduleAsync(AlbumScheduleModel requestModel)
        {
            var existAlbumSchedule = await _scheduleRepository.GetQueryable().AnyAsync(x => x.Title == requestModel.Title);

            if (existAlbumSchedule || requestModel == null)
                return new ServiceResponse<bool>("error_schedule_existed");

            var newSchedule = new ScheduleAlbum()
            {
                Title = requestModel.Title,
                TimeRelease = requestModel.TimeRelease,
                Type = requestModel.Type,
                BackgroundUrl = requestModel.BackgroundUrl,
                Url = requestModel.Url,
                DateRelease = requestModel.DateRelease,
                Region = requestModel.Region,
                Status = requestModel.Status
            };

            _scheduleRepository.Add(newSchedule);
            await _unitOfWork.SaveChangesAsync();

            return new ServiceResponse<bool>(true);
        }
    }
}
