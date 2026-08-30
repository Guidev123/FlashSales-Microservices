using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace FlashSales.Infrastructure.Mongo
{
    public static class MongoExtensions
    {
        public static IServiceCollection AddMongo(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MongoOptions>(configuration.GetSection(MongoOptions.SectionName));
            services.PostConfigure<MongoOptions>(options =>
                options.ConnectionString = configuration.GetConnectionString("Mongo")
                    ?? throw new InvalidOperationException("Connection string 'Mongo' is not configured."));

            RegisterGuidSerializer();

            services.AddSingleton<IMongoClient>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<MongoOptions>>().Value;
                return new MongoClient(options.ConnectionString);
            });

            services.AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<IOptions<MongoOptions>>().Value;
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(options.DatabaseName);
            });

            return services;
        }

        private static void RegisterGuidSerializer()
        {
            try
            {
                BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
            }
            catch (BsonSerializationException)
            {
            }
        }
    }
}