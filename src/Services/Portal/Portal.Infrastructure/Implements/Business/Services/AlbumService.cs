using Common;
using Common.Models;
using Portal.Domain.AggregatesModel.AlbumAggregate;
using Portal.Domain.Interfaces.Business.Services;
using Portal.Domain.Interfaces.External;
using Portal.Domain.Models.AlbumModels;
using Portal.Domain.Models.ImageUploadModels;
using Portal.Domain.SeedWork;

namespace Portal.Infrastructure.Implements.Business.Services
{
    public class AlbumService : IAlbumService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<Album> _repository;
        private readonly IGenericRepository<AlbumAlertMessage> _albumAlertMessageRepository;
        private readonly IGenericRepository<ContentType> _contentTypeRepository;
        private readonly IAmazonS3Service _amazonS3Service;

        public AlbumService(
            IUnitOfWork unitOfWork,
            IAmazonS3Service amazonS3Service)
        {
            _unitOfWork = unitOfWork;
            _repository = unitOfWork.Repository<Album>();
            _albumAlertMessageRepository = unitOfWork.Repository<AlbumAlertMessage>();
            _contentTypeRepository = unitOfWork.Repository<ContentType>();
            _amazonS3Service = amazonS3Service;
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
                FriendlyName = CommonHelper.GenerateFriendlyName(requestModel.Title)
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
            if (!string.IsNullOrEmpty(requestModel.FileName) && requestModel.FileData != null)
            {
                var result = await _amazonS3Service.UploadImageAsync(new ImageUploadRequestModel
                {
                    FileName = requestModel.FileName,
                    ImageData = requestModel.FileData
                }, entity.FriendlyName);

                entity.ThumbnailUrl = result.AbsoluteUrl;
                entity.CdnThumbnailUrl = $"https://s3.codegota.me/{result.RelativeUrl}";
            }

            _repository.Add(entity);
            await _unitOfWork.SaveChangesAsync();

            // Map entity to response model
            var responseModel = new AlbumResponseModel
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                AlbumAlertMessageName = entity.AlbumAlertMessage?.Name,
                ContentTypeNames = contentTypeNames.JoinSeparator(),
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
            if (requestModel.IsUpdateThumbnail && !string.IsNullOrEmpty(requestModel.FileName) && requestModel.FileData != null)
            {
                var result = await _amazonS3Service.UploadImageAsync(new ImageUploadRequestModel
                {
                    FileName = requestModel.FileName,
                    ImageData = requestModel.FileData
                }, existingAlbum.FriendlyName);

                existingAlbum.ThumbnailUrl = result.AbsoluteUrl;
                existingAlbum.CdnThumbnailUrl = $"https://s3.codegota.me/{result.RelativeUrl}";
            }

            // Update
            _repository.Update(existingAlbum);
            await _unitOfWork.SaveChangesAsync();

            // Map to response
            var responseModel = new AlbumResponseModel
            {
                Id = existingAlbum.Id,
                Title = existingAlbum.Title,
                Description = existingAlbum.Description,
                AlbumAlertMessageName = existingAlbum.AlbumAlertMessage?.Name,
                ContentTypeNames = contentTypeNames.JoinSeparator(),
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
                CdnThumbnailUrl = x.CdnThumbnailUrl
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

            return new ServiceResponse<bool>(true);
        }

        // Other private methods
        private async Task<bool> DoesTitleExistAsync(string title)
        {
            return await _repository.GetQueryable().AnyAsync(x => x.Title == title);
        }

        public async Task<ServiceResponse<PagingCommonResponse<AlbumPagingResponse>>> GetPagingAsync(PagingCommonRequest request)
        {
            var parameters = new Dictionary<string, object?>
            {
                { "PageNumber", request.PageNumber },
                { "PageSize", request.PageSize },
                { "SearchTerm", request.SearchTerm },
                { "SortColumn", request.SortColumn },
                { "SortDirection", request.SortDirection }
            };
            var result = await _unitOfWork.QueryAsync<AlbumPagingResponse>("Album_All_Paging", parameters);

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
                CdnThumbnailUrl = album.CdnThumbnailUrl
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
    }
}
