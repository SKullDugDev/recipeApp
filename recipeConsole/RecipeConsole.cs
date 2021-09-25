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
        static async Task Main()
        {
            // run the configuration setup and store the results in the configurationResults object
            var configurationResults = Helpers.ConfigurationSetup();

            // from the configurationResults object, get the service provider
            var serviceProvider = configurationResults.ServiceProvider;

            // from the services, get the service ILogger for RecipeConsole
            var logger = serviceProvider.GetRequiredService<ILogger<RecipeConsole>>();

            // using the settings, get the arguments for the MongoDB Processa          
            var mongoArgs = Helpers.GetMongoArgs(configurationResults.Settings);

            Console.WriteLine("Beginning communication with MongoDB...");

            try
            {
                await MongoDBProcess.IngredientBuilder(mongoArgs);
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
