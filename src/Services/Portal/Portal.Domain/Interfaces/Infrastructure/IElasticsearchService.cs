using Portal.Domain.Models.AlbumModels;

namespace Portal.Domain.Interfaces.Infrastructure
{
    public interface IElasticsearchService
    {
        Task SyncAlbumsAsync();
        Task<bool> CreateIndexAsync();
        Task<bool> DeleteIndexAsync();
        Task ResetIndexes();
        Task IndexDocumentAsync(AlbumDocument albumDocument);
        Task IndexManyDocumenstAsync(List<AlbumDocument> albumDocuments);
        Task<List<AlbumDocument>> GetDocumentsAsync();
    }
}
