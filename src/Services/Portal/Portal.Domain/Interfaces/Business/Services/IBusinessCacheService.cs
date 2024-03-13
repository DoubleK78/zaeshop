namespace Portal.Domain.Interfaces.Business.Services
{
    public interface IBusinessCacheService
    {
        Task ReloadCacheHomePageAsync(string locale);
        Task ReloadCachePopularComicsAsync(string locale);
        Task RelaodCacheRecentlyComicsAsync(string locale);
        Task ReloadCacheTopComicsAsync(string locale);
    }
}
