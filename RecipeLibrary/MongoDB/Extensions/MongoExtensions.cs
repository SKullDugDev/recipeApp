using System;
using MongoDB.Driver;
using System.Threading.Tasks;
using System.Linq.Expressions;
using RecipeLibrary.Configuration;


namespace RecipeLibrary.MongoDB.Extensions
{
    public static class MongoExtensions
    {

        public static string GetMongoConnectionString(this SettingsConfig settings)
        {

            // store host from appsettings in settings into mongoDBHost
            var mongoDBHost = settings.AppSettings.MongoDBInfo.Host;

            // store port from appsettings in settings into mongoDBPort
            var mongoDBPort = settings.AppSettings.MongoDBInfo.Port;

            // store password from appsettings in settings into mongoArgs
            var mongoDBPass = Uri.EscapeDataString(settings.AppSettings.MongoDBInfo.MongoDBPass);

            Console.WriteLine("Connection string formed...");

            // form connection string for communicating with mongoDB
            return $"mongodb://SKDev:{mongoDBPass}@{mongoDBHost}:{mongoDBPort}/?authSource=admin";

        }


        public static async Task<string> BuildDocumentIndex<TDocument>(this IMongoCollection<TDocument> documentCollection, Expression<Func<TDocument, object>> indexFieldName)
        {

            // first use a builder to start up the index key logic; then make an indexModel to hold the
            var documentIndexBuilder = Builders<TDocument>.IndexKeys;

            // then create an index model to establish the index logic: index using RecipeId in an ascending order
            var indexModel = new CreateIndexModel<TDocument>(documentIndexBuilder.Ascending(indexFieldName));

            // finally add the index to the collection and the name of the index
            return await documentCollection.Indexes.CreateOneAsync(indexModel); ;
        }

        public static async Task<string> BuildUniqueDocumentIndex<TDocument>(this IMongoCollection<TDocument> documentCollection, Expression<Func<TDocument, object>> indexFieldName)
        {

            // first use a builder to start up the index key logic; then make an indexModel to hold the
            var documentIndexBuilder = Builders<TDocument>.IndexKeys;

            // create the options for the index so we can set it to be unique
            var indexOptions = new CreateIndexOptions
            {
                Unique = true
            };

            // then create an index model to establish the index logic: index using RecipeId in an ascending order
            var indexModel = new CreateIndexModel<TDocument>(documentIndexBuilder.Ascending(indexFieldName), indexOptions);

            // finally add the index to the collection and the name of the index
            return await documentCollection.Indexes.CreateOneAsync(indexModel); ;
        }


    }
}
