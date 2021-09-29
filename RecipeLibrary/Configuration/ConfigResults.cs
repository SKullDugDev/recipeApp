using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RecipeLibrary.Configuration
{
    public class ConfigResults
    {
        public ServiceProvider ServiceProvider { get; set; }

        public SettingsConfig Settings { get; set; }

        public IHttpClientFactory HttpClientFactory { get; set; }

        public static ConfigResults ConfigurationSetup()
        {
            // intiate a new configuration builder to create our configurations
            var configuration = new ConfigurationBuilder()
                // add json file, appsettings.json; mark as not optional; reload on change
                .AddJsonFile("appsettings.json", false, true)
                // add user secrets to the mongodb file
                .AddUserSecrets<Recipes.MongoDB.MongoRecipe>()
                // build our configurations
                .Build();

            // initiate a new service collection to build up our services
            var services = new ServiceCollection();

            // adding logging as a service using the logging configuration
            // this includes adding the configuration for the console from logging in appsettings.json
            // now that configure has the console settings, add the appropriate console logger
            services.AddLogging(configure =>
            {
                configure.AddConfiguration(configuration.GetSection("Logging"));
                configure.AddConsole();
            });

            // for best practice, we need to use an IHttpClientFactory for making our http client
            // add the IHttpClientFactory and related services to our service collection using AddHttpClient
            services.AddHttpClient();

            // use our service collection to build a service provider
            var serviceProvider = services.BuildServiceProvider();

            // create a new instance of the ConfigurationResults object/class
            var configurationResults = new ConfigResults
            {

                // store the service provider in configurationResults
                ServiceProvider = serviceProvider,

                // get the IHttpClientFactory from our service provider and store it in configurationResults
                HttpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>(),

                // from our configurations, get our configuration settings object and store the result in configurationResults
                Settings = configuration.Get<SettingsConfig>()
            };

            // return the configurationResults object
            return configurationResults;
        }

    }


}