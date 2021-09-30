using System;
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

    }
}
