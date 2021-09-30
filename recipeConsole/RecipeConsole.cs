using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using RecipeLibrary.Configuration;
using RecipeLibrary.MongoDB;
using RecipeLibrary.Ingredients.MongoDB;

namespace RecipeConsole
{
    class RecipeConsole
    {
        static async Task Main()
        {
            // run the configuration setup and store the results in the configurationResults object
            var startupResults = Startup.ConfigurationSetup();

            Console.WriteLine("Configuration Setup successful, results retrieved...");

            // from the configurationResults object, get the service provider
            var serviceProvider = startupResults.ServiceProvider;

            // from the services, get the service ILogger for and use it on the console
            var logger = serviceProvider.GetRequiredService<ILogger<RecipeConsole>>();            

            Console.WriteLine("Starting up the Ingredient Builder...");

            try
            {
                // send config results to builder
                string connectionString = startupResults.Settings.GetMongoConnectionString();

                string ingredientsJSON = await MongoIngredient.MakeIngredientJSONFromRecipes(connectionString);
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
