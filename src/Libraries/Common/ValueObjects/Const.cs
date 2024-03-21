namespace Common.ValueObjects
{
    public static class Const
    {
        public static class RoleName
        {
            public const string User = "User";
        }

        public static class RedisCacheKey
        {
            public const string ComicContent = "ComicContent_ComicFriendlyName_{0}_ContentFriendlyName_{1}";
            public const string ViewCount = "ViewCount_{0}";
            public const string LevelCount = "LevelCount_{0}";
            public const string ComicDetail = "/api/client/comicapp/{0}";
            public const string HomePopularComicsPaging = "/api/client/comicapp/paging|country-|firstchar-|genre-|language-|pagenumber-1|pagesize-12|rating-|region-{0}|searchterm-|sortcolumn-views|sortdirection-desc|status-false|year-";
            public const string HomeRecentlyComicsPaging = "/api/client/comicapp/paging|country-|firstchar-|genre-|language-|pagenumber-1|pagesize-12|rating-|region-{0}|searchterm-|sortcolumn-updatedonutc|sortdirection-desc|status-false|year-";
            public const string HomeTopDayComicsPaging = "/api/client/comicapp/paging|country-|firstchar-|genre-|language-|pagenumber-1|pagesize-5|rating-|region-{0}|searchterm-|sortcolumn-viewbytoptype|sortdirection-desc|status-false|toptype-day|year-";
            public const string HomeTopMonthComicsPaging = "/api/client/comicapp/paging|country-|firstchar-|genre-|language-|pagenumber-1|pagesize-5|rating-|region-{0}|searchterm-|sortcolumn-viewbytoptype|sortdirection-desc|status-false|toptype-month|year-";
            public const string HomeTopYearComicsPaging = "/api/client/comicapp/paging|country-|firstchar-|genre-|language-|pagenumber-1|pagesize-5|rating-|region-{0}|searchterm-|sortcolumn-viewbytoptype|sortdirection-desc|status-false|toptype-year|year-";
            public const string ComicCommentPageOneCache = "ComicComment_{0}";

            public const string ComicPagingPattern = "*/api/client/comicapp/paging*";
            public const string UserRankingPagingPattern = "*/api/user/ranking*";
            public const string PopularComicsPagingPattern = "*/api/client/comicapp/paging|country-|firstchar-|genre-|language-|pagenumber-*|pagesize-*|rating-|region-{0}|searchterm-|sortcolumn-views|sortdirection-desc|toptype-|year-*";
            public const string RecentlyComicsPagingPattern = "*/api/client/comicapp/paging|country-|firstchar-|genre-|language-|pagenumber-*|pagesize-*|rating-|region-{0}|searchterm-|sortcolumn-updatedonutc|sortdirection-desc|toptype-|year-*";
            public const string TopComicsPagingPattern = "*/api/client/comicapp/paging|country-|firstchar-|genre-|language-|pagenumber-*|pagesize-*|rating-|region-{0}|searchterm-|sortcolumn-viewbytoptype|sortdirection-desc|toptype-*|year-*";
        }

        public static class ServiceLogEventName
        {
            public const string ErrorAddView = "Error From Add View From User To Redis";
            public const string StoredViewsCache = "Stored Views Content Comic From Redis Cache";

            public const string ErrorAddExp = "Error From Add Experience From User To Redis";
            public const string StoredExpCache = "Stored Experience View Or Comment To Redis";

            public const string StoredLevelPublicChap = "Stored Reset Level Public Chap";
        }

        public static class HangfireJobName
        {
            public const string CalculateViewsFromRedis = "Calculate Views From Redis";
            public const string CalculateExperiencesFromRedis = "Calculate Experiences From Redis";
            public const string SendEmailSPremiumFollowers = "Send notification to SPremium followers";
            public const string ResetRoleUsers = "Reset role users";
            public const string ResetJobNotUpdateRunningStatus = "Reset Job Not Update Running Status";
            public const string ResetLevelPublic = "Reset level public";
            public const string ResetLevelPublicChap = "Reset level public chap";
            public const string RemindSubscription = "Remind subscription";
        }

        public static class PushNotification
        {
            public const string NewChapterComicVi = "Đã có chap mới truyện bạn đang theo dõi";
            public const string NewChapterComicEn = "New chapter of the comic you are following is available";

            public const string NewChapterComicDescriptionVi = "{0} {1} đã có mặt trên Fast Scans, Vui lòng nhấn thông báo để truy cập nhanh vào ứng dụng";
            public const string NewChapterComicDescriptionEn = "{0} {1} is now available on Fast Scans. Please tap the notification to quickly access the app.";

            public const string RemindSubscriptionVi = "Gói Premium của bạn sắp hết hạn";
            public const string RemindSubscriptionEn = "Your premium subscription is expiring soon";

            public const string RemindSubscriptionDescriptionVi = "Còn {0} ngày nữa sẽ hết hạn. Để tiếp tục sử dụng không ngắt quảng, Vui lòng gia hạn hoặc nâng cấp gói Premium.";
            public const string RemindSubscriptionDescriptionEn = "Only {0} days left until expiration. To continue uninterrupted usage, please renew or upgrade to the Premium subscription.";
        }
    }
}
