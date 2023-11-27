using Common.Enums;
using Common.Interfaces.Messaging;
using Common.Shared.Models.Logs;
using Nest;
using Portal.Domain.AggregatesModel.AlbumAggregate;
using Portal.Domain.Interfaces.Infrastructure;
using Portal.Domain.Models.AlbumModels;
using Portal.Domain.SeedWork;

namespace Portal.Infrastructure.Implements.Infrastructure
{
    public class ElasticsearchService : IElasticsearchService
    {
        private readonly string _albumIndex = "album-index";
        private readonly ElasticClient _elasticClient;
        private readonly IServiceLogPublisher _serviceLogPublisher;
        private readonly IUnitOfWork _unitOfWork;

        public ElasticsearchService(
            ElasticClient elasticClient,
            IServiceLogPublisher serviceLogPublisher,
            IUnitOfWork unitOfWork)
        {
            _elasticClient = elasticClient;
            _serviceLogPublisher = serviceLogPublisher;
            _unitOfWork = unitOfWork;
        }

        public async Task SyncAlbumsAsync()
        {
            var albumns = await _unitOfWork.Repository<Album>().GetAllAsync();
            var albumnDocuments = albumns.Select(x => new AlbumDocument
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                AlbumAlertMessageId = x.AlbumAlertMessageId,
                AlbumAlertMessageName = x.AlbumAlertMessage?.Name,
                CreatedDate = x.CreatedOnUtc,
                UpdatedDate = x.UpdatedOnUtc
            }).ToList();

            // Create if index not found
            var result = await _elasticClient.Indices.ExistsAsync(_albumIndex);
            if (!result.Exists)
            {
                await CreateIndexAsync();
            }

            await IndexManyDocumenstAsync(albumnDocuments);
        }

        public async Task<bool> CreateIndexAsync()
        {
            var createIndexResponse = await _elasticClient.Indices.CreateAsync(_albumIndex);

            if (!createIndexResponse.IsValid)
            {
                await _serviceLogPublisher.WriteLogAsync(new ServiceLogMessage
                {
                    LogLevel = ELogLevel.Information,
                    EventName = "[ELK] CreateIndexAsync",
                    ServiceName = "Portal",
                    Environment = "Local",
                    Description = "Method AlbumSyncElkJob.CreateIndexAsync returns (false)"
                });
                return false;
            }

            return createIndexResponse.IsValid;
        }

        public async Task<bool> DeleteIndexAsync()
        {
            var deleteIndexResponse = await _elasticClient.Indices.DeleteAsync(_albumIndex);
            if (!deleteIndexResponse.IsValid)
            {
                await _serviceLogPublisher.WriteLogAsync(new ServiceLogMessage
                {
                    LogLevel = ELogLevel.Information,
                    EventName = "[ELK] DeleteIndexAsync",
                    ServiceName = "Portal",
                    Environment = "Local",
                    Description = "Method AlbumSyncElkJob.DeleteIndexAsync returns (false)"
                });
                return false;
            }

            return deleteIndexResponse.IsValid;
        }

        public async Task ResetIndexes()
        {
            await DeleteIndexAsync();
            await CreateIndexAsync();
        }

        public async Task IndexDocumentAsync(AlbumDocument albumDocument)
        {
            var response = await _elasticClient.IndexAsync(new IndexRequest<AlbumDocument>(albumDocument, _albumIndex));
            if (!response.IsValid)
            {
                await _serviceLogPublisher.WriteLogAsync(new ServiceLogMessage
                {
                    LogLevel = ELogLevel.Information,
                    EventName = "[ELK] UpdateDocumentAsync",
                    ServiceName = "Portal",
                    Environment = "Local",
                    Description = response.DebugInformation
                });
            }
        }

        public async Task IndexManyDocumenstAsync(List<AlbumDocument> albumDocuments)
        {
            var response = await _elasticClient.IndexManyAsync(albumDocuments, _albumIndex);
            if (!response.IsValid)
            {
                await _serviceLogPublisher.WriteLogAsync(new ServiceLogMessage
                {
                    LogLevel = ELogLevel.Information,
                    EventName = "[ELK] BulkUpdateDocumenstAsync",
                    ServiceName = "Portal",
                    Environment = "Local",
                    Description = response.DebugInformation
                });
            }
        }

        public async Task<List<AlbumDocument>> GetDocumentsAsync()
        {
            var searchResponse = await _elasticClient.SearchAsync<AlbumDocument>(s => s
                .Index(_albumIndex)
            );

            if (searchResponse.IsValid)
            {
                return [.. searchResponse.Documents];
            }

            return [];
        }
    }
}