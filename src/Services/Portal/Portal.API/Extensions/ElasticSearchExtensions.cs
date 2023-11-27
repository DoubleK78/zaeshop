using Nest;

namespace Portal.API.Extensions
{
    public static class ElasticSearchExtensions
    {
        public static void AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
        {
            var elkCloudUrl = configuration.GetSection("ElkSettings").GetValue<string>("CloudUrl")!;
            var elkAccessKey = configuration.GetSection("ElkSettings").GetValue<string>("AccessKey")!;
            var elkAccessSecrect = configuration.GetSection("ElkSettings").GetValue<string>("SecretKey")!;
            
            var elkSettings = new ConnectionSettings(new Uri(elkCloudUrl))
                                .BasicAuthentication(elkAccessKey, elkAccessSecrect);
                                
            services.AddSingleton(x => new ElasticClient(elkSettings));
        }
    }
}
