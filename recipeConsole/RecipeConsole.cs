using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RecipeLibrary.Configuration;
using RecipeLibrary.MongoDB.Extensions;
using RecipeLibrary.MongoDB.Ingredients;
using Microsoft.Extensions.DependencyInjection;

namespace RecipeConsole
{
    class RecipeConsole
    {
        static async Task Main()
        {
            // check execution time with a stopwatch
            var watch = System.Diagnostics.Stopwatch.StartNew();

            // run the configuration setup and store the results in the configurationResults object
            var startupResults = Startup.SetupConfiguration();

            Console.WriteLine("Configuration Setup successful, results retrieved...");

            // from the configurationResults object, get the service provider
            using var serviceProvider = startupResults.ServiceProvider;

            // from the services, get the service ILogger for and use it on the console
            var logger = serviceProvider.GetRequiredService<ILogger<RecipeConsole>>();

            Console.WriteLine("Starting up the Ingredient Builder...");

            try
            {
                // send config results to builder
                string connectionString = startupResults.Settings.GetMongoConnectionString();

                await IMongoIngredient.MakeIngredientJSONFromRecipes(connectionString);

            }
            catch (Exception e)
            {
                logger.LogError("Error produced during processing of data: {e}", e);

            }

            watch.Stop();
            Console.WriteLine($"Total execution time was {watch.ElapsedMilliseconds}ms");


        }
    }
}
