using System;
using recipeClass;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace recipeConsole
{
    class RecipeConsole
    {
        static async Task Main()
        {
            // run the configuration setup and store the results in the configurationResults object
            var configurationResults = Configuration.Methods.ConfigurationSetup();

            Console.WriteLine("Configuration Setup successful, results retrieved...");

            // from the configurationResults object, get the service provider
            var serviceProvider = configurationResults.ServiceProvider;

            // from the services, get the service ILogger for and use it on the console
            var logger = serviceProvider.GetRequiredService<ILogger<RecipeConsole>>();            

            Console.WriteLine("Starting up the Ingredient Builder...");

            try
            {
                // send config results to builder
                await recipeClass.MongoDB.Methods.IngredientBuilder(configurationResults);
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
