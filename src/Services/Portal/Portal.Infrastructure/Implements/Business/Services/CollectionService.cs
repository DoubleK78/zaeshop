using Common;
using Common.Enums;
using Common.Interfaces;
using Common.Interfaces.Messaging;
using Common.Models;
using Common.Shared.Models.Logs;
using Common.ValueObjects;
using Microsoft.Extensions.Hosting;
using Portal.Domain.AggregatesModel.AlbumAggregate;
using Portal.Domain.AggregatesModel.CollectionAggregate;
using Portal.Domain.AggregatesModel.TaskAggregate;
using Portal.Domain.AggregatesModel.UserAggregate;
using Portal.Domain.Enums;
using Portal.Domain.Interfaces.Business.Services;
using Portal.Domain.Models.CollectionModels;
using Portal.Domain.Models.ContentItemModels;
using Portal.Domain.SeedWork;
using Portal.Infrastructure.Helpers;

namespace Portal.Infrastructure.Implements.Business.Services
{
    public class CollectionService : ICollectionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<Collection> _repository;
        private readonly IGenericRepository<Album> _albumRepository;
        private readonly IGenericRepository<ContentItem> _contentItemRepository;
        private readonly IRedisService _redisService;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IServiceLogPublisher _serviceLogPublisher;
        private readonly IHostEnvironment _hostingEnvironment;
        private readonly IGenericRepository<CollectionView> _collectionViewRepository;

        public CollectionService(
            IUnitOfWork unitOfWork,
            IRedisService redisService,
            IServiceLogPublisher serviceLogPublisher,
            IHostEnvironment hostingEnvironment)
        {
            _unitOfWork = unitOfWork;
            _repository = unitOfWork.Repository<Collection>();
            _albumRepository = unitOfWork.Repository<Album>();
            _contentItemRepository = unitOfWork.Repository<ContentItem>();
            _redisService = redisService;
            _userRepository = unitOfWork.Repository<User>();
            _serviceLogPublisher = serviceLogPublisher;
            _hostingEnvironment = hostingEnvironment;
            _collectionViewRepository = unitOfWork.Repository<CollectionView>();
        }

        public async Task<ServiceResponse<CollectionResponseModel>> CreateAsync(CollectionRequestModel requestModel)
        {
            requestModel.Title = requestModel.Title.Trim();
            requestModel.Description = requestModel.Description?.Trim();
            requestModel.ExtendName = requestModel.ExtendName?.Trim();

            var existingAlbum = await _albumRepository.GetByIdAsync(requestModel.AlbumId);
            if (existingAlbum == null)
            {
                return new ServiceResponse<CollectionResponseModel>("error_album_not_found");
            }

            // Validate if the associated album exists
            var isExistsTitle = await _repository.GetQueryable().AnyAsync(x => x.Title == requestModel.Title && x.AlbumId == requestModel.AlbumId);
            if (isExistsTitle)
            {
                return new ServiceResponse<CollectionResponseModel>("error_collection_title_exists");
            }

            // Create a new collection entity
            var entity = new Collection
            {
                Title = requestModel.Title,
                AlbumId = requestModel.AlbumId,
                Volume = requestModel.Volume,
                ExtendName = requestModel.ExtendName,
                Description = requestModel.Description,
                FriendlyName = CommonHelper.GenerateFriendlyName(requestModel.Title),
                LevelPublic = requestModel.IsPriority ? ELevelPublic.SPremiumUser : ELevelPublic.AllUser,
                StorageType = requestModel.StorageType
            };

            // Update Comic Recently uploaded
            existingAlbum.UpdatedOnUtc = DateTime.UtcNow;

            // Add the entity to the repository and save changes
            _repository.Add(entity);
            await _unitOfWork.SaveChangesAsync();

            // Remove cache Comic Detail
            _redisService.Remove(string.Format(Const.RedisCacheKey.ComicDetail, existingAlbum.FriendlyName));

            // Remove cache Comic Paging
            _redisService.RemoveByPattern(Const.RedisCacheKey.ComicPagingPattern);

            // Map the entity to the response model
            var responseModel = new CollectionResponseModel
            {
                Id = entity.Id,
                Title = entity.Title,
                AlbumId = entity.AlbumId,
                AlbumTitle = existingAlbum.Title,
                Volume = entity.Volume,
                ExtendName = entity.ExtendName,
                Description = entity.Description
                // Add other properties as needed
            };

            return new ServiceResponse<CollectionResponseModel>(responseModel);
        }

        public async Task<ServiceResponse<CollectionResponseModel>> UpdateAsync(int id, CollectionRequestModel requestModel)
        {
            requestModel.Title = requestModel.Title.Trim();
            requestModel.Description = requestModel.Description?.Trim();
            requestModel.ExtendName = requestModel.ExtendName?.Trim();

            // Retrieve the existing collection entity by ID
            var existingEntity = await _repository.GetByIdAsync(id);
            if (existingEntity == null)
            {
                return new ServiceResponse<CollectionResponseModel>("error_collection_not_found");
            }

            // Validate if the associated album exists
            var existingAlbum = await _albumRepository.GetByIdAsync(requestModel.AlbumId);
            if (existingAlbum == null)
            {
                return new ServiceResponse<CollectionResponseModel>("error_album_not_found");
            }

            var isExistsTitle = await _repository.GetQueryable().AnyAsync(x => x.Title == requestModel.Title && x.Id != id && x.AlbumId == requestModel.AlbumId);
            if (isExistsTitle)
            {
                return new ServiceResponse<CollectionResponseModel>("error_collection_title_exists");
            }

            // Update the existing collection entity properties
            existingEntity.Title = requestModel.Title;
            existingEntity.AlbumId = requestModel.AlbumId;
            existingEntity.Volume = requestModel.Volume;
            existingEntity.ExtendName = requestModel.ExtendName;
            existingEntity.Description = requestModel.Description;
            existingEntity.FriendlyName = CommonHelper.GenerateFriendlyName(requestModel.Title);
            existingEntity.LevelPublic = requestModel.IsPriority ? ELevelPublic.SPremiumUser : ELevelPublic.AllUser;
            existingEntity.StorageType = requestModel.StorageType;

            // Update the entity in the repository and save changes
            _repository.Update(existingEntity);
            await _unitOfWork.SaveChangesAsync();

            // Remove cache Comic Detail
            _redisService.Remove(string.Format(Const.RedisCacheKey.ComicDetail, existingAlbum.FriendlyName));

            // Remove cache Comic Paging
            _redisService.RemoveByPattern(Const.RedisCacheKey.ComicPagingPattern);

            // Map the updated entity to the response model
            var responseModel = new CollectionResponseModel
            {
                Id = existingEntity.Id,
                Title = existingEntity.Title,
                AlbumId = existingEntity.AlbumId,
                AlbumTitle = existingAlbum.Title,
                Volume = existingEntity.Volume,
                ExtendName = existingEntity.ExtendName,
                Description = existingEntity.Description
                // Add other properties as needed
            };

            return new ServiceResponse<CollectionResponseModel>(responseModel);
        }

        public async Task<ServiceResponse<List<CollectionResponseModel>>> GetAllAsync()
        {
            // Retrieve all collections from the repository
            var collections = await _repository.GetAllAsync();

            // Map the entities to the response model list
            var response = collections.Select(x => new CollectionResponseModel
            {
                Id = x.Id,
                Title = x.Title,
                AlbumId = x.AlbumId,
                AlbumTitle = x.Album.Title,
                Volume = x.Volume,
                ExtendName = x.ExtendName,
                Description = x.Description,
                // Add other properties as needed
                ContentItems = x.ContentItems?.Select(y => y.RelativeUrl).ToList()
            }).ToList();

            return new ServiceResponse<List<CollectionResponseModel>>(response);
        }

        public async Task<ServiceResponse<bool>> DeleteAsync(int id)
        {
            // Retrieve the existing collection entity by ID
            var existingEntity = await _repository.GetByIdAsync(id);
            if (existingEntity == null)
            {
                return new ServiceResponse<bool>("error_collection_not_found");
            }

            // Delete the entity from the repository and save changes
            _repository.Delete(existingEntity);
            await _unitOfWork.SaveChangesAsync();

            // Remove cache Comic Paging
            _redisService.RemoveByPattern(Const.RedisCacheKey.ComicPagingPattern);

            return new ServiceResponse<bool>(true);
        }

        public async Task<ServiceResponse<List<GetContentItemModel>>> GetContentItemsAsync(int id)
        {
            var contentItems = await _contentItemRepository.GetQueryable().Where(x => x.CollectionId == id)
                                    .Select(x => new GetContentItemModel
                                    {
                                        Id = x.Id,
                                        Name = x.Name,
                                        OrderBy = x.OrderBy,
                                        RelativeUrl = x.RelativeUrl,
                                        CreatedOnUtc = x.CreatedOnUtc,
                                        Type = x.Type
                                    }).ToListAsync();

            return new ServiceResponse<List<GetContentItemModel>>(contentItems);
        }

        // Additional private methods or helper functions can be added here
        public async Task<ServiceResponse<PagingCommonResponse<CollectionPagingResponse>>> GetPagingAsync(CollectionPagingRequest request)
        {
            var parameters = new Dictionary<string, object?>
            {
                { "PageNumber", request.PageNumber },
                { "PageSize", request.PageSize },
                { "SearchTerm", request.SearchTerm },
                { "SortColumn", request.SortColumn },
                { "SortDirection", request.SortDirection },
                { "AlbumId", request.AlbumId }
            };
            var result = await _unitOfWork.QueryAsync<CollectionPagingResponse>("Collection_All_Paging", parameters);

            var record = result.Find(o => o.IsTotalRecord);
            if (record == null)
            {
                return new ServiceResponse<PagingCommonResponse<CollectionPagingResponse>>(new PagingCommonResponse<CollectionPagingResponse>
                {
                    RowNum = 0,
                    Data = new List<CollectionPagingResponse>()
                });
            }

            result.Remove(record);
            return new ServiceResponse<PagingCommonResponse<CollectionPagingResponse>>(new PagingCommonResponse<CollectionPagingResponse>
            {
                RowNum = record.RowNum,
                Data = result
            });
        }

        public async Task AddViewFromUserToRedisAsync(CollectionViewUserBuildModel model)
        {
            bool isDeployed = bool.Parse(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT_DEPLOYED") ?? "false");
            var prefixEnvironment = isDeployed ? "[Docker] " : string.Empty;

            try
            {
                User? user = null;
                if (!string.IsNullOrEmpty(model.IdentityUserId))
                {
                    user = await _userRepository.GetByIdentityUserIdAsync(model.IdentityUserId);
                    if (user == null)
                    {
                        // Log Error when model have user id not exists database
                        await _serviceLogPublisher.WriteLogAsync(new ServiceLogMessage
                        {
                            LogLevel = ELogLevel.Information,
                            EventName = Const.ServiceLogEventName.ErrorAddView,
                            ServiceName = "Hangfire",
                            Environment = prefixEnvironment + _hostingEnvironment.EnvironmentName,
                            Description = $"User with IdentityUserId {model.IdentityUserId} not found",
                            IpAddress = model.IpAddress,
                            Request = JsonSerializationHelper.Serialize(model)
                        });
                        return;
                    }
                }

                var collectionExists = await _repository.GetQueryable().AnyAsync(x => x.Id == model.CollectionId);
                if (!collectionExists)
                {
                    // Log Error when model have collection id not exists database
                    await _serviceLogPublisher.WriteLogAsync(new ServiceLogMessage
                    {
                        LogLevel = ELogLevel.Information,
                        EventName = Const.ServiceLogEventName.ErrorAddView,
                        ServiceName = "Hangfire",
                        Environment = prefixEnvironment + _hostingEnvironment.EnvironmentName,
                        Description = $"Collection with id {model.CollectionId} not found",
                        IpAddress = model.IpAddress,
                        Request = JsonSerializationHelper.Serialize(model)
                    });
                    return;
                }

                // We use key ViewCount_0 -> ViewCount_50
                var key = string.Format(Const.RedisCacheKey.ViewCount, DateTime.UtcNow.Minute - (DateTime.UtcNow.Minute % 10));
                var value = await _redisService.GetAsync<List<CollectionViewModel>>(key);
                if (value == null)
                {
                    value = new List<CollectionViewModel>
                    {
                        new CollectionViewModel
                        {
                            CollectionId = model.CollectionId,
                            UserId = user?.Id,
                            SessionId = model.SessionId,
                            IpAddress = model.IpAddress,
                            CreatedOnUtc = DateTime.UtcNow
                        }
                    };

                    await _redisService.SetAsync(key, value, 30);
                }
                else
                {
                    var collectionViewByUser = value.Find(o => o.CollectionId == model.CollectionId && (
                        (o.UserId.HasValue && o.UserId == user?.Id) || o.IpAddress == model.IpAddress || o.SessionId == model.SessionId
                    ));

                    if (collectionViewByUser == null)
                    {
                        value.Add(new CollectionViewModel
                        {
                            CollectionId = model.CollectionId,
                            UserId = user?.Id,
                            SessionId = model.SessionId,
                            IpAddress = model.IpAddress,
                            CreatedOnUtc = DateTime.UtcNow
                        });

                        await _redisService.SetAsync(key, value, 30);
                    }
                }
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

        public async Task CalculateViewsFromRedisTaskAsync()
        {
            bool isDeployed = bool.Parse(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT_DEPLOYED") ?? "false");
            var prefixEnvironment = isDeployed ? "[Docker] " : string.Empty;

            var scheduleJob = await _unitOfWork.Repository<HangfireScheduleJob>().GetByNameAsync(Const.HangfireJobName.CalculateViewsFromRedis);
            if (scheduleJob != null && scheduleJob.IsEnabled && !scheduleJob.IsRunning)
            {
                try
                {
                    var parameters = new Dictionary<string, object?>
                    {
                        { "Id",  scheduleJob.Id }
                    };
                    await _unitOfWork.ExecuteAsync("Hangfire_StartJob", parameters);

                    await CaclculateViewsFromRedisAsync();

                    await _unitOfWork.ExecuteAsync("Hangfire_EndJob", parameters);
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

                    var parameters = new Dictionary<string, object?>
                    {
                        { "Id",  scheduleJob.Id }
                    };
                    await _unitOfWork.ExecuteAsync("Hangfire_EndJob", parameters);
                }
            }
        }

        public async Task CaclculateViewsFromRedisAsync()
        {
            bool isDeployed = bool.Parse(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT_DEPLOYED") ?? "false");
            var prefixEnvironment = isDeployed ? "[Docker] " : string.Empty;

            // Get redis data from 10 minutes ago
            var minutes = DateTime.UtcNow.Minute - (DateTime.UtcNow.Minute % 10) - 10;
            var key = string.Format(Const.RedisCacheKey.ViewCount, minutes < 0 ? 50: minutes);

            // Get safely Cache value from key
            List<CollectionViewModel>? value;
            try
            {
                value = await _redisService.GetAsync<List<CollectionViewModel>>(key);
            }
            catch
            {
                value = new List<CollectionViewModel>();
            }

            if (value != null && value.Count != 0)
            {
                var collectionIds = value.Select(x => x.CollectionId).Distinct().ToList();
                var collectionViewsInDb = await _collectionViewRepository.GetQueryable().Where(x =>
                    collectionIds.Contains(x.CollectionId) && (x.Date == value[0].CreatedOnUtc.Date)
                ).ToListAsync();

                var addCollectionViews = new List<CollectionView>();
                var updateCollectionViews = new List<CollectionView>();

                foreach (var item in value)
                {
                    var collectionView = collectionViewsInDb.Find(x => x.CollectionId == item.CollectionId && (
                        x.UserId == item.UserId || x.IpAddress == item.IpAddress || x.SessionId == item.SessionId
                    ));
                    var newCollectionView = addCollectionViews.Find(x => x.CollectionId == item.CollectionId && (
                        x.UserId == item.UserId || x.IpAddress == item.IpAddress || x.SessionId == item.SessionId
                    ));

                    // Case 1: No records today, Create new record
                    if (collectionView == null && newCollectionView == null)
                    {
                        addCollectionViews.Add(new CollectionView
                        {
                            CollectionId = item.CollectionId,
                            UserId = item.UserId,
                            View = 1,
                            IpAddress = item.IpAddress,
                            SessionId = item.SessionId,
                            Date = item.CreatedOnUtc.Date
                        });
                    }
                    // Case 2: No records today, created record before so we update record that ready to save database
                    else if (collectionView == null && newCollectionView != null)
                    {
                        newCollectionView.View++;

                        if (item.UserId != null && newCollectionView.UserId == null)
                        {
                            newCollectionView.UserId = item.UserId;
                        }

                        #region Update lastest IP and stored Previous IPs
                        if (!string.IsNullOrEmpty(item.IpAddress) && item.IpAddress != newCollectionView.IpAddress)
                        {
                            newCollectionView.IpAddress = item.IpAddress;
                        }
                        #endregion
                    }
                    // Case 3: Exists today, we update record
                    else if (collectionView != null && newCollectionView == null)
                    {
                        collectionView.View++;

                        if (item.UserId != null && collectionView.UserId == null)
                        {
                            collectionView.UserId = item.UserId;
                        }

                        #region Update lastest IP and stored Previous IPs
                        if (!string.IsNullOrEmpty(item.IpAddress) && item.IpAddress != collectionView.IpAddress)
                        {
                            collectionView.IpAddress = item.IpAddress;
                            updateCollectionViews.Add(collectionView);
                        }
                        #endregion
                    }
                }

                if (addCollectionViews.Count > 0)
                {
                    await _unitOfWork.BulkInsertAsync(addCollectionViews);
                }

                if (updateCollectionViews.Count > 0)
                {
                    updateCollectionViews.ForEach(x => x.UpdatedOnUtc = DateTime.UtcNow);
                    await _unitOfWork.BulkUpdateAsync(updateCollectionViews);
                }

                // Re-calculate views to collection and album
                var parameters = new Dictionary<string, object?>
                {
                    { "collectionIds",  string.Join(',', collectionIds)}
                };
                await _unitOfWork.ExecuteAsync("Collection_Album_RecalculateViews", parameters);

                // Reset cache when calculated successfully
                _redisService.Remove(key);

                // Remove cache comic details
                if (collectionIds.Count > 0)
                {
                    var albumFriendlyNames = await _repository.GetQueryable()
                                                    .Where(x => collectionIds.Contains(x.Id))
                                                    .Select(y => y.Album.FriendlyName)
                                                    .Distinct()
                                                    .ToListAsync();

                    foreach (var friendlyName in albumFriendlyNames)
                    {
                        _redisService.Remove(string.Format(Const.RedisCacheKey.ComicDetail, friendlyName));
                    }
                }

                // Reset Top Rank Comics
                _redisService.RemoveByPattern(Const.RedisCacheKey.ComicRankingPagingPattern);

                // Log to service log to stored
                await _serviceLogPublisher.WriteLogAsync(new ServiceLogMessage
                {
                    LogLevel = ELogLevel.Information,
                    EventName = Const.ServiceLogEventName.StoredViewsCache,
                    ServiceName = "Hangfire",
                    Environment = prefixEnvironment + _hostingEnvironment.EnvironmentName,
                    Description = $"Stored total views from redis cache. Key {key}, At {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}",
                    Request = JsonSerializationHelper.Serialize(value)
                });
            }
        }

        public async Task<ServiceResponse<CollectionResponseModel>> GetByIdAsync(int id)
        {
            var collection = await _repository.GetByIdAsync(id);
            if (collection == null)
            {
                return new ServiceResponse<CollectionResponseModel>("error_collection_not_found");
            }

            // Map the entities to the response model list
            var response = new CollectionResponseModel
            {
                Id = collection.Id,
                Title = collection.Title,
                AlbumId = collection.AlbumId,
                AlbumTitle = collection.Album.Title,
                Volume = collection.Volume,
                ExtendName = collection.ExtendName,
                Description = collection.Description,
                AlbumFriendlyName = collection.Album.FriendlyName,
                FriendlyName = collection.FriendlyName
            };

            return new ServiceResponse<CollectionResponseModel>(response);
        }

        public async Task<ServiceResponse<string>> BulkCreateAsync(int albumId, List<BulkCreateCollectionRequest> collections)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            if (transaction == null) return new ServiceResponse<string>("error_transaction");
            try
            {
                var album = await _albumRepository.GetByIdAsync(albumId);
                if (album == null)
                {
                    return new ServiceResponse<string>("error_album_not_found");
                }

                var existsCollections = await _repository.GetQueryable().Where(o => o.AlbumId == albumId).ToListAsync();

                var addCollections = new List<BulkCreateCollectionDb>();
                foreach (var item in collections)
                {
                    bool isExists = existsCollections.Any(x => x.FriendlyName == item.Name);
                    if (!isExists)
                    {
                        // convert friendly name to title
                        string[] words = item.Name.Split('-');
                        string title = string.Join(" ", words.Select(w => char.ToUpper(w[0]) + w[1..]));
                        var contentItems = item.ContentItems.ConvertAll(x =>
                        {
                            // Prefix relative to stored folder places
                            string prefixRelative = $"{album.FriendlyName}/{item.Name}";
                            return new ContentItem
                            {
                                Name = x.Name,
                                RelativeUrl = prefixRelative + "/" + x.Name,
                                OrderBy = RegexHelper.GetNumberByText(x.Name)
                            };
                        }).OrderBy(x => x.OrderBy).ToList();

                        var newCollection = new Collection
                        {
                            AlbumId = albumId,
                            Title = title,
                            FriendlyName = item.Name,
                            LevelPublic = item.IsPriority ? ELevelPublic.SPremiumUser : ELevelPublic.AllUser,
                            StorageType = item.StorageType
                        };
                        addCollections.Add(new BulkCreateCollectionDb
                        {
                            Collection = newCollection,
                            ContentItems = contentItems
                        });
                    }
                }

                if (addCollections.Count > 0)
                {
                    // Update Comic Recently uploaded
                    album.UpdatedOnUtc = DateTime.UtcNow;

                    await _unitOfWork.BulkInsertAsync(addCollections.ConvertAll(x => x.Collection));

                    foreach (var item in addCollections)
                    {
                        item.ContentItems.ForEach(x => x.CollectionId = item.Collection.Id);
                    }

                    await _unitOfWork.BulkInsertAsync(addCollections.SelectMany(x => x.ContentItems).ToList());

                    // Remove cache Comic Detail
                    _redisService.Remove(string.Format(Const.RedisCacheKey.ComicDetail, album.FriendlyName));

                    // Remove cache Comic Paging
                    _redisService.RemoveByPattern(Const.RedisCacheKey.ComicPagingPattern);

                    await _unitOfWork.CommitTransactionBulkOperationsAsync(transaction);
                }
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
            return new ServiceResponse<string>("success");
        }

        public async Task ResetLevelPublicTaskAsync()
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
            var collections = await _repository.GetQueryable()
                                    .Include(x => x.Album)
                                    .Where(x => x.LevelPublic != ELevelPublic.AllUser)
                                    .ToListAsync();

            if (collections == null || collections.Count == 0)
                return new ServiceResponse<bool>("error_reset_level_public");

            var albumFriendlyNames = new List<string?>();
            var updateCollections = new List<Collection>();

            foreach (var collection in collections)
            {
                TimeSpan difference = DateTime.UtcNow - collection.CreatedOnUtc;

                if (collection.LevelPublic == ELevelPublic.Partner && difference.TotalMinutes >= 15)
                {
                    collection.LevelPublic = ELevelPublic.SPremiumUser;
                    albumFriendlyNames.Add(collection.Album.FriendlyName);
                    updateCollections.Add(collection);
                }

                if (collection.LevelPublic == ELevelPublic.SPremiumUser && difference.TotalHours >= 4 && difference.TotalHours < 12)
                {
                    collection.LevelPublic = ELevelPublic.PremiumUser;
                    albumFriendlyNames.Add(collection.Album.FriendlyName);
                    updateCollections.Add(collection);
                }

                if (collection.LevelPublic == ELevelPublic.PremiumUser && difference.TotalHours >= 12)
                {
                    collection.LevelPublic = ELevelPublic.AllUser;
                    albumFriendlyNames.Add(collection.Album.FriendlyName);
                    updateCollections.Add(collection);
                }
            }

            await _unitOfWork.BulkSaveChangesAsync();

            // Remove cache comic details
            if (albumFriendlyNames.Count > 0)
            {
                foreach (var friendlyName in albumFriendlyNames.Distinct())
                {
                    _redisService.Remove(string.Format(Const.RedisCacheKey.ComicDetail, friendlyName));
                }
            }

            if (updateCollections.Count > 0)
            {
                foreach (var collection in updateCollections)
                {
                    _redisService.Remove(string.Format(Const.RedisCacheKey.ComicContent, collection.Album.FriendlyName, collection.FriendlyName));
                }
            }

            return new ServiceResponse<bool>(true);
        }
    }
}
