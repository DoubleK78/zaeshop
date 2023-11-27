using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

namespace HangFireServer.Extensions
{
    public static class ElasticSearchExtensions
    {
        public static void AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
        {
            var elkCloudUrl = configuration.GetSection("ElkSettings").GetValue<string>("CloudUrl")!;
            var elkAccessKey = configuration.GetSection("ElkSettings").GetValue<string>("AccessKey")!;
            var elkAccessSecrect = configuration.GetSection("ElkSettings").GetValue<string>("SecretKey")!;
            
            var elkSettings = new ElasticsearchClientSettings(new Uri(elkCloudUrl))
                                .Authentication(new BasicAuthentication(elkAccessKey, elkAccessSecrect));
                                
            services.AddSingleton(x => new ElasticsearchClient(elkSettings));
        }
    }
}
