using Common.Interfaces;
using Common.Models;
using Common.ValueObjects;
using Portal.Domain.Enums;
using Portal.Domain.Interfaces.Business.Services;
using Portal.Domain.Models.AlbumModels;
using Portal.Domain.SeedWork;

namespace Portal.Infrastructure.Implements.Business.Services
{
    public class BusinessCacheService : IBusinessCacheService
    {
        private readonly IRedisService _redisService;
        private readonly IUnitOfWork _unitOfWork;

        public BusinessCacheService(IRedisService redisService, IUnitOfWork unitOfWork)
        {
            _redisService = redisService;
            _unitOfWork = unitOfWork;
        }

        public async Task ReloadCacheHomePageAsync(string locale)
        {
            _redisService.RemoveByPattern(Const.RedisCacheKey.ComicPagingPattern);

            // Override Home Cache
            // 1. Popular Comic
            var popularComics = await GetComicPagingAsync(new PagingCommonRequest
            {
                PageNumber = 1,
                PageSize = 12,
                SearchTerm = "",
                SortColumn = "views",
                SortDirection = "desc",
            }, new FilterAdvanced
            {
                FirstChar = "",
                Genre = "",
                Country = "",
                Year = "",
                Status = false,
                Language = "",
                Rating = "",
                Region = locale
            });
            await _redisService.SetAsync(string.Format(Const.RedisCacheKey.HomePopularComicsPaging, locale), popularComics, 60 * 24);

            // 2. Recently Comic
            var recentlyComics = await GetComicPagingAsync(new PagingCommonRequest
            {
                PageNumber = 1,
                PageSize = 12,
                SearchTerm = "",
                SortColumn = "updatedOnUtc",
                SortDirection = "desc",
            }, new FilterAdvanced
            {
                FirstChar = "",
                Genre = "",
                Country = "",
                Year = "",
                Status = false,
                Language = "",
                Rating = "",
                Region = locale
            });
            await _redisService.SetAsync(string.Format(Const.RedisCacheKey.HomeRecentlyComicsPaging, locale), recentlyComics, 60 * 24);

            // 3. Top Day Comic
            var topDayComics = await GetComicPagingAsync(new PagingCommonRequest
            {
                PageNumber = 1,
                PageSize = 5,
                SearchTerm = "",
                SortColumn = "viewTopType",
                SortDirection = "desc",
            }, new FilterAdvanced
            {
                FirstChar = "",
                Genre = "",
                Country = "",
                Year = "",
                Status = false,
                Language = "",
                Rating = "",
                TopType = "day",
                Region = locale
            });
            await _redisService.SetAsync(string.Format(Const.RedisCacheKey.HomeTopDayComicsPaging, locale), topDayComics, 60 * 24);

            // 4. Top Month Comic
            var topMonthComics = await GetComicPagingAsync(new PagingCommonRequest
            {
                PageNumber = 1,
                PageSize = 5,
                SearchTerm = "",
                SortColumn = "viewByTopType",
                SortDirection = "desc",
            }, new FilterAdvanced
            {
                FirstChar = "",
                Genre = "",
                Country = "",
                Year = "",
                Status = false,
                Language = "",
                Rating = "",
                TopType = "month",
                Region = locale
            });
            await _redisService.SetAsync(string.Format(Const.RedisCacheKey.HomeTopMonthComicsPaging, locale), topMonthComics, 60 * 24);

            // 5. Top Year Comic
            var topYearComics = await GetComicPagingAsync(new PagingCommonRequest
            {
                PageNumber = 1,
                PageSize = 5,
                SearchTerm = "",
                SortColumn = "viewByTopType",
                SortDirection = "desc",
            }, new FilterAdvanced
            {
                FirstChar = "",
                Genre = "",
                Country = "",
                Year = "",
                Status = false,
                Language = "",
                Rating = "",
                TopType = "year",
                Region = locale
            });
            await _redisService.SetAsync(string.Format(Const.RedisCacheKey.HomeTopYearComicsPaging, locale), topYearComics, 60 * 24);
        }

        public async Task ReloadCachePopularComicsAsync(string locale)
        {
            var popularComics = await GetComicPagingAsync(new PagingCommonRequest
            {
                PageNumber = 1,
                PageSize = 12,
                SearchTerm = "",
                SortColumn = "views",
                SortDirection = "desc",
            }, new FilterAdvanced
            {
                FirstChar = "",
                Genre = "",
                Country = "",
                Year = "",
                Status = false,
                Language = "",
                Rating = "",
                Region = locale
            });

            await _redisService.SetAsync(string.Format(Const.RedisCacheKey.HomePopularComicsPaging, locale), popularComics, 60 * 24);

            // Remove cache Comic Paging
            _redisService.RemoveByPattern(string.Format(Const.RedisCacheKey.PopularComicsPagingPattern, locale));
        }

        public async Task RelaodCacheRecentlyComicsAsync(string locale)
        {
            var recentlyComics = await GetComicPagingAsync(new PagingCommonRequest
            {
                PageNumber = 1,
                PageSize = 12,
                SearchTerm = "",
                SortColumn = "updatedOnUtc",
                SortDirection = "desc",
            }, new FilterAdvanced
            {
                FirstChar = "",
                Genre = "",
                Country = "",
                Year = "",
                Status = false,
                Language = "",
                Rating = "",
                Region = locale
            });
            await _redisService.SetAsync(string.Format(Const.RedisCacheKey.HomeRecentlyComicsPaging, locale), recentlyComics, 60 * 24);

            // Remove cache Comic Paging
            _redisService.RemoveByPattern(string.Format(Const.RedisCacheKey.RecentlyComicsPagingPattern, locale));
        }

        public async Task ReloadCacheTopComicsAsync(string locale)
        {
            // Top Day Comic
            var topDayComics = await GetComicPagingAsync(new PagingCommonRequest
            {
                PageNumber = 1,
                PageSize = 5,
                SearchTerm = "",
                SortColumn = "viewByTopType",
                SortDirection = "desc",
            }, new FilterAdvanced
            {
                FirstChar = "",
                Genre = "",
                Country = "",
                Year = "",
                Status = false,
                Language = "",
                Rating = "",
                TopType = "day",
                Region = locale
            });
            await _redisService.SetAsync(string.Format(Const.RedisCacheKey.HomeTopDayComicsPaging, locale), topDayComics, 60 * 24);

            // Top Month Comic
            var topMonthComics = await GetComicPagingAsync(new PagingCommonRequest
            {
                PageNumber = 1,
                PageSize = 5,
                SearchTerm = "",
                SortColumn = "viewByTopType",
                SortDirection = "desc",
            }, new FilterAdvanced
            {
                FirstChar = "",
                Genre = "",
                Country = "",
                Year = "",
                Status = false,
                Language = "",
                Rating = "",
                TopType = "month",
                Region = locale
            });
            await _redisService.SetAsync(string.Format(Const.RedisCacheKey.HomeTopMonthComicsPaging, locale), topMonthComics, 60 * 24);

            // Top Year Comic
            var topYearComics = await GetComicPagingAsync(new PagingCommonRequest
            {
                PageNumber = 1,
                PageSize = 5,
                SearchTerm = "",
                SortColumn = "viewByTopType",
                SortDirection = "desc",
            }, new FilterAdvanced
            {
                FirstChar = "",
                Genre = "",
                Country = "",
                Year = "",
                Status = false,
                Language = "",
                Rating = "",
                TopType = "year",
                Region = locale
            });
            await _redisService.SetAsync(string.Format(Const.RedisCacheKey.HomeTopYearComicsPaging, locale), topYearComics, 60 * 24);

            // Remove cache Comic Paging
            _redisService.RemoveByPattern(string.Format(Const.RedisCacheKey.TopComicsPagingPattern, locale));
        }

        private async Task<ServiceResponse<PagingCommonResponse<AlbumPagingResponse>>> GetComicPagingAsync(PagingCommonRequest request, FilterAdvanced filter)
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
    }
}
