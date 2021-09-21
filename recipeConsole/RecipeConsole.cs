using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System;
using recipeClass;
using System.Threading.Tasks;

namespace recipeConsole
{
    class RecipeConsole
    {


        static async Task Main(string[] args)
        {

            // load the app settings into configuration
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .AddUserSecrets<RecipeConsole>()
                .Build();

            // parse all settings into the settings class structure
            var settings = configuration.Get<ConfigurationSettings>();


            // setup logging
            var services = new ServiceCollection() as IServiceCollection;

            services.AddLogging(configure =>
            {
                configure.AddConfiguration(configuration.GetSection("Logging"));
                configure.AddConsole();
            });

            var serviceProvider = services.BuildServiceProvider();

            // log settings that were parsed
            var logger = serviceProvider.GetRequiredService<ILogger<RecipeConsole>>();

            // Main bit of code

            // set mongodb connection variables

            string mongoDBHost = settings.AppSettings.MongoDBInfo.host;
            int mondoDBPort = settings.AppSettings.MongoDBInfo.port;
            string mongoDBPass = Uri.EscapeDataString(settings.AppSettings.MongoDBInfo.mongodbpass);

            Console.WriteLine("Beginning process...");
            try
            {
                await MongoDBProcess.Connect(mongoDBHost, mondoDBPort, mongoDBPass);
            }
            catch (Exception e)
            {
                logger.LogDebug("Error produced during processing of data: {e}", e);
            }


            // dispose the serviceProvider; this will ensure all logs get flushed
            serviceProvider.Dispose();

        }



    }
}
