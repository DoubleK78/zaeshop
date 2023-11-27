using Common;
using Common.Models;
using Nest;
using Portal.Domain.AggregatesModel.AlbumAggregate;
using Portal.Domain.Interfaces.Business.Services;
using Portal.Domain.Models.AlbumModels;
using Portal.Domain.SeedWork;

namespace Portal.Infrastructure.Implements.Business.Services
{
    public class AlbumService : IAlbumService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<Album> _repository;
        private readonly IGenericRepository<AlbumAlertMessage> _albumAlertMessageRepository;
        private readonly IGenericRepository<ContentType> _contentTypeRepository;
        private readonly ElasticClient _elasticClient;
        private readonly string _albumIndex = "album-index";

        public AlbumService(
            IUnitOfWork unitOfWork,
            ElasticClient elasticClient)
        {
            _unitOfWork = unitOfWork;
            _repository = unitOfWork.Repository<Album>();
            _albumAlertMessageRepository = unitOfWork.Repository<AlbumAlertMessage>();
            _contentTypeRepository = unitOfWork.Repository<ContentType>();
            _elasticClient = elasticClient;
        }

        public async Task<ServiceResponse<AlbumResponseModel>> CreateAsync(AlbumRequestModel requestModel)
        {
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
                AlbumAlertMessageId = requestModel.AlbumAlertMessageId
            };

            if (requestModel.ContentTypeIds?.Count > 0)
            {
                entity.AlbumContentTypes = requestModel.ContentTypeIds.ConvertAll(id => new AlbumContentType
                {
                    ContentTypeId = id
                });
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
            // Get existing entity
            var existingAlbum = await _repository.GetByIdAsync(id);
            if (existingAlbum == null)
            {
                return new ServiceResponse<AlbumResponseModel>("error_album_not_found");
            }

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

            // Update properties
            existingAlbum.Title = requestModel.Title;
            existingAlbum.Description = requestModel.Description;
            existingAlbum.AlbumAlertMessageId = requestModel.AlbumAlertMessageId;

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
                    else
                    {
                        // remove exists
                        existingAlbum.AlbumContentTypes.Remove(existingAlbumContentType);
                    }
                }
            }
            else
            {
                foreach (var existingAlbumContentType in existingAlbum.AlbumContentTypes)
                {
                    existingAlbum.AlbumContentTypes.Remove(existingAlbumContentType);
                }
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
                CreatedDate = x.CreatedOnUtc
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

        public async Task<ServiceResponse<PagingCommonResponse<AlbumPagingResponse>>> GetPagingByELKAsync(PagingCommonRequest request)
        {
            // Generate a code get by _elasticsearchClient
            var searchResponse = await _elasticClient.SearchAsync<AlbumPagingResponse>(s => s
                .Index(_albumIndex)
                .Query(q => q
                    .Bool(b => b
                        .Should(sh => sh
                            .Prefix(p => p
                                .Field(f => f.Title)
                                .Value(request.SearchTerm ?? string.Empty)
                            ),
                            sh => sh
                            .Prefix(p => p
                                .Field(f => f.Description)
                                .Value(request.SearchTerm ?? string.Empty)
                            )
                        )
                    )
                )
                .From((request.PageNumber - 1) * request.PageSize)
                .Size(request.PageSize)
            );

            var result = new PagingCommonResponse<AlbumPagingResponse>
            {
                RowNum = searchResponse.Total,
                Data = searchResponse.Documents.ToList()
            };

            return new ServiceResponse<PagingCommonResponse<AlbumPagingResponse>>(result);
        }
    }
}
