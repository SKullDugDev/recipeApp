using System;
using System.IO;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson.Serialization.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Configuration;



namespace recipeClass
{
    public class MongoDB
    {
        public class Objects
        {
            public class Recipe
            {
                [BsonId]
                public ObjectId ObjectID { get; set; }

                [BsonElement("recipeID")]
                public int RecipeID { get; set; }

                [BsonElement("title")]
                public string Title { get; set; }

                [BsonElement("ingredients")]
                public string Ingredients { get; set; }

                [BsonElement("directions")]
                public string Directions { get; set; }

                [BsonElement("link")]
                public string Link { get; set; }

                [BsonElement("source")]
                public string Source { get; set; }

                [BsonElement("NER")]
                public string NER { get; set; }
            }

            public class RecipeDefinitionBuilder
            {
                public FilterDefinition<Recipe> Filter { get; set; }
                public SortDefinition<Recipe> Sort { get; set; }

                public FindOptions<Objects.Recipe> Options { get; set; }
            }

            public class Ingredient
            {
                [BsonId]
                public string ObjectID { get; set; }

                [BsonElement("RecipeID")]
                public int RecipeID { get; set; }

                [BsonElement("IngredientID")]
                public int IngredientID { get; set; }

                [BsonElement("NamedEntity")]
                public string NamedEntity { get; set; }
            }

        }

        public class Methods
        {

            public static IMongoCollection<Objects.Recipe> GetRecipeCollection(Configuration.Objects.Settings.ConfigurationSettings settings)
            {
                // store host from appsettings in settings into mongoDBHost
                var mongoDBHost = settings.AppSettings.MongoDBInfo.Host;

                // store port from appsettings in settings into mongoDBPort
                var mongoDBPort = settings.AppSettings.MongoDBInfo.Port;

                // store password from appsettings in settings into mongoArgs
                var mongoDBPass = Uri.EscapeDataString(settings.AppSettings.MongoDBInfo.MongoDBPass);

                // form connection string for communicating with mongoDB
                string connectionString = $"mongodb://SKDev:{mongoDBPass}@{mongoDBHost}:{mongoDBPort}/?authSource=admin";
                Console.WriteLine("Connection string formed...");

                // create a new instance of the MongoDB Client server and call it client
                var client = new MongoClient(connectionString);
                Console.WriteLine("Formed Client connection to MongoDB Server");

                // from the client server get the recipes database
                var recipesDB = client.GetDatabase("recipes");
                Console.WriteLine("Accessed Recipes Database from Client...");

                // from the recipes database, get the recipes collection and connect it to the mapping recipe entity class
                return recipesDB.GetCollection<Objects.Recipe>("recipes");
            }

            public static async Task RecipeIndexBuilder(IMongoCollection<Objects.Recipe> recipeCollection)
            {

                // first use a builder to start up the index key logic; then make an indexModel to hold the
                var recipeIndexBuilder = Builders<Objects.Recipe>.IndexKeys;

                // then create an index model to establish the index logic: index using RecipeID in an ascending order
                var indexModel = new CreateIndexModel<Objects.Recipe>(recipeIndexBuilder.Ascending(recipe => recipe.RecipeID));

                // finally add the index to the collection
                await recipeCollection.Indexes.CreateOneAsync(indexModel);
            }

            public static Objects.RecipeDefinitionBuilder GetRecipeDefinitionBuilder()
            {
                // implement a filter definition builder for the recipe entity class 
                var filterBuilder = Builders<Objects.Recipe>.Filter;

                // implement a sort definition builder for the recipe entity class
                var sortBuilder = Builders<Objects.Recipe>.Sort;

                var optionBuilder = new FindOptions<Objects.Recipe>()
                {
                    BatchSize = 101
                };

                // initiate a new instance of the recipe defintion builder class
                var recipeDefinitionBuilder = new Objects.RecipeDefinitionBuilder
                {

                    // use a filter so we can sort through all the documents with the Gathered source
                    // store it in the Filter property of the recipe definition builder
                    Filter = filterBuilder.Eq(recipe => recipe.Source, "Gathered"),

                    // create a sort definiton logic to sort by RecipeID in ascending order
                    // store it in the Sort property of the recipe defintion builder
                    Sort = sortBuilder.Ascending("RecipeID"),

                    Options = optionBuilder

                };

                // return the recipe definition builder
                return recipeDefinitionBuilder;
            }



            public static async Task<string[]> GetNamedEntitiesSet(Objects.Recipe recipe)
            {
                // we recieve the recipe, store the named objects of that recipe
                string namedEntities = recipe.NER;

                // from the string of named objects, create a UTF-8 encoded byte array using GetBytes();
                // from this, create a new memory stream
                var namedEntitesStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(namedEntities));

                // asynchronously deserialize the stream made from the string and put it in the form of an array of strings
                string[] namedEntitiesSet = await JsonSerializer.DeserializeAsync<string[]>(namedEntitesStream);

                return namedEntitiesSet;
            }



            public static void BuildIngredient(string item, Objects.Recipe recipe, List<Objects.Ingredient> ingredients)
            {
                // store item locally as namedEntity
                string namedEntity = item;

                // strip anything not basic latin from string using Regex
                namedEntity = Regex.Replace(namedEntity, @"[^\u0000-\u007F]", String.Empty);

                // initiate a new instance of the ingredient class
                Objects.Ingredient ingredient = new()
                {

                    // assign the ingredient objectid to the objectid of the set
                    ObjectID = recipe.ObjectID.ToString(),

                    // assign the ingredient recipeid to the recipeid of the set
                    RecipeID = recipe.RecipeID,

                    // assign the ingredient a named entity
                    NamedEntity = namedEntity
                };

                // add the ingredient object to the list of ingredients
                ingredients.Add(ingredient);
            }


            public static async Task ProcessRecipesFromCursor(IAsyncCursor<Objects.Recipe> cursor, bool cursorEmpty, IHttpClientFactory httpClientFactory, int recipeCount, List<Objects.Ingredient> ingredients)
            {

                Console.WriteLine("Checking recipes for ingredients...");

                // get the current documents in the cursor and place it in an IEnumberal of type Entities.Recipe named recipes
                IEnumerable<Objects.Recipe> recipes = cursor.Current;

                // for each recipe of type Entities.Recipe in the recipes list
                foreach (Objects.Recipe recipe in recipes)
                {
                    var siteResponse = await WebClient.GetSiteResponse(recipe.Link, httpClientFactory);

                    if (siteResponse.IsSuccessStatusCode == true)
                    {

                        // get the set of named objects
                        var namedEntitiesSet = await GetNamedEntitiesSet(recipe);

                        // for each named entity in the array of named objects
                        foreach (string item in namedEntitiesSet)
                        {

                            BuildIngredient(item, recipe, ingredients);

                        }

                        recipeCount++;
                    }
                }

                // check to make sure it the list of ingredients doesn't have copies
                // then fix the ingredient list so it includes only the distinct ingredients
                ingredients = ingredients.DistinctBy(ingredient => ingredient.NamedEntity).ToList();

                // set the cursor empty check variable to the opposite of the result of the move
                // if there is more, then cursor empty will be false and the loop continues
                // if there is no more, then cursor empty will be true and the loop ends
                cursorEmpty = !await cursor.MoveNextAsync();
            }


            public static async Task GetIngredientsFromCursor(IAsyncCursor<Objects.Recipe> cursor, IHttpClientFactory httpClientFactory, int recipeCount, List<Objects.Ingredient> ingredients)
            {

                // when ToCursorAsync is used, the cursor originally has no content
                // MoveNextAsync()/something similar needs to be called to get the first batch of docs
                // MoveNextAsync() returns true if there are more docs to be avail and false otherwise
                // we create a bool cursorEmpty to check if the cursor is ever empty
                // then we assign it to the opposite of the result we get from moving the cursor
                // we catch and explain a fail condition later below for when it's empty at the start

                Console.WriteLine("Checking for recipes...");
                bool cursorEmpty = !await cursor.MoveNextAsync();

                // while it is true that the cursor is not empty, run
                // can also be read as: while (not cursorEmpty) evaluates as true, run

                while (!cursorEmpty)
                {
                    await ProcessRecipesFromCursor(cursor, cursorEmpty, httpClientFactory, recipeCount, ingredients);

                }

                // if at the start when we do cursor.MoveNextAsync() there are no docs in the first batch
                // check first to see if ingredients is empty as it should be if the while loop never ran
                // by doing this we should never throw an exception cuz of the loop, only the empty first batch

                if (!ingredients.Any() && cursorEmpty)
                {
                    string exceptionMessage = "Cursor has no documents...Please check the collection or the cursor logic...";
                    throw new ArgumentException(exceptionMessage);
                }

            }

            public static void ParseIngredients(List<string> ingredientStrings, List<Objects.Ingredient> ingredients)
            {

                // set the JSONSerliazerOptions first to the default for the web and then override it to include fields and indentation
                var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
                {
                    IncludeFields = true,
                    WriteIndented = true
                };
                // start off i as 0
                int i = 0;

                Console.WriteLine("Ingredients gathered into list...assigning id and transforming into JSON string...");

                foreach (Objects.Ingredient ingredient in ingredients)
                {
                    // assign ingredient an id equal to the value of i
                    ingredient.IngredientID = i;

                    // serialize the individual ingredient object into what it should be, a JSON string and store it as a string variable
                    string ingredientString = JsonSerializer.Serialize<Objects.Ingredient>(ingredient, options);

                    // add serialized ingredient string to the ingredient strings list
                    ingredientStrings.Add(ingredientString);

                    // increase the number of i by 1
                    i++;
                };
            }

            public static async Task IngredientBuilder(Configuration.Objects.ConfigurationResults configurationResults)
            {
                // get the recipe collection from the database
                var recipeCollection = GetRecipeCollection(configurationResults.Settings);

                Console.WriteLine("Recipe Collection found and mapped...");

                // in order to better performance when sorting and querying, we need to create an index for our recipe collection
                await RecipeIndexBuilder(recipeCollection);

                Console.WriteLine("Recipe Index built...");

                // create an empty list of ingredient objects to collect the ingredients
                List<Objects.Ingredient> ingredients = new();

                // create an empty list of strings to collect the ingredient serialized string
                List<string> ingredientStrings = new();

                // get the recipe definition builder so we can find and sort the documents properly
                var recipeDefinitionBuilder = GetRecipeDefinitionBuilder();
                Console.WriteLine("Sorting and filtering logic determined...");

                // start off recipe count as 0
                int recipeCount = 0;

                // create the httpClient we will be using in processing the documents


                Console.WriteLine("Initial sorting through collection in progress...");

                //from the collection, find only documents that match the filter, sort them by ascending RecipeID,
                //asynchronously return a cursor variable that holds the memory address of the documents; the cursor is just a pointer
                //implement the cursor via a using statement to properly dispose of it

                using (IAsyncCursor<Objects.Recipe> cursor = await recipeCollection.Find(recipeDefinitionBuilder.Filter).Sort(recipeDefinitionBuilder.Sort).ToCursorAsync())
                {

                    await GetIngredientsFromCursor(cursor, configurationResults.HttpClientFactory, recipeCount, ingredients);

                }

                Console.WriteLine("Parsing through ingredients...");
                ParseIngredients(ingredientStrings, ingredients);

                Console.WriteLine("Forming final JSON object string");

                // take all the ingredient json objects in the ingredient json list and combine them into one json string containing all ingredients
                string ingredientsJSON = String.Join(",", ingredientStrings);

                Console.WriteLine(ingredientsJSON);

                return;

            }

        }



    }


    // this is the class for configuration related stuff
    public class Configuration
    {
        // this class contains classes which will be the base for our custom config objects
        public class Objects
        {

            // custom configuration classes related to settings 
            // we will make objeccts from these
            public class Settings
            {
                // the config settings class used to create the config settings object
                public class ConfigurationSettings
                {
                    // get and set the logging object
                    // it will be based on the Fields.Logging class
                    public Fields.Logging Logging { get; set; }

                    // get and set the app settings object
                    // it wil be based on the Fields.Appsettings class
                    public Fields.AppSettings AppSettings { get; set; }

                    // override the ToString method for this class
                    // it will take in the string and serialize it with indentation
                    public override string ToString()
                    {
                        return JsonSerializer.Serialize(this, new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });
                    }
                }


                // class used to create the field objects for the config settings object
                public class Fields
                {
                    // defines the scheme with which the logging object will be be made
                    public class Logging
                    {
                        // we will get the console settings
                        public ConsoleSettings Console { get; set; }

                        // we will get includescopes
                        public bool IncludeScopes { get; set; }

                        // we quickly define how console settings will look like
                        // define each log level object
                        public class ConsoleSettings
                        {
                            public LogLevelSettings LogLevel { get; set; }

                            public class LogLevelSettings
                            {
                                [JsonConverter(typeof(JsonStringEnumConverter))]
                                public LogLevel Default { get; set; }

                                [JsonConverter(typeof(JsonStringEnumConverter))]
                                public LogLevel System { get; set; }

                                [JsonConverter(typeof(JsonStringEnumConverter))]
                                public LogLevel Microsoft { get; set; }

                                [JsonConverter(typeof(JsonStringEnumConverter))]
                                public LogLevel HttpClient { get; set; }
                            }
                        }
                    }

                    // defines the scheme for the appsettings object
                    public class AppSettings
                    {
                        public MongoDBSettings MongoDBInfo { get; set; }

                        public class MongoDBSettings
                        {
                            [JsonPropertyName("host")]
                            public string Host { get; set; }

                            [JsonPropertyName("port")]
                            public int Port { get; set; }

                            [JsonPropertyName("mongodbpass")]
                            public string MongoDBPass { get; set; }
                        }
                    }
                }
            }

            public class ConfigurationResults
            {
                public ServiceProvider ServiceProvider { get; set; }

                public Settings.ConfigurationSettings Settings { get; set; }

                public IHttpClientFactory HttpClientFactory { get; set; }

            }

        }

        public class Methods
        {

            // this method sets up the configuration for the class library and console
            public static Objects.ConfigurationResults ConfigurationSetup()
            {
                // intiate a new configuration builder to create our configurations
                var configuration = new ConfigurationBuilder()
                    // add json file, appsettings.json; mark as not optional; reload on change
                    .AddJsonFile("appsettings.json", false, true)
                    // add user secrets to the mongodb file
                    .AddUserSecrets<MongoDB>()
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
                var configurationResults = new Objects.ConfigurationResults
                {

                    // store the service provider in configurationResults
                    ServiceProvider = serviceProvider,

                    // get the IHttpClientFactory from our service provider and store it in configurationResults
                    HttpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>(),

                    // from our configurations, get our configuration settings object and store the result in configurationResults
                    Settings = configuration.Get<Objects.Settings.ConfigurationSettings>()
                };

                // return the configurationResults object
                return configurationResults;
            }

        }
    }

    // this is the class for the webclient
    public class WebClient
    {
        // use this to get the site response 
        public static async Task<HttpResponseMessage> GetSiteResponse(string recipeLink, IHttpClientFactory httpClientFactory)
        {

            var httpClient = httpClientFactory.CreateClient();

            // if the link doesn't include the http:// scheme already
            if (recipeLink.Contains("http:") == false)
            {

                // make a proper uri out of the link
                var recipeURI = AddUriScheme(recipeLink);

                try
                {
                    // make a header request and return the response
                    return await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, recipeURI.ToString()));

                }
                catch (Exception e)
                {
                    Console.WriteLine("Header Request refused...attempting a normal Get request...error as follows: {0}", e);
                    return await httpClient.GetAsync(recipeURI);
                }

            }

            // if the link includes the https:// scheme already
            try
            {
                // send a header request
                return await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, recipeLink));
            }
            catch (Exception e)
            {
                Console.WriteLine("Header Request refused...attempting a normal Get request...error as follows: {0}", e);
                return await httpClient.GetAsync(recipeLink);
            }

        }

        public static Uri AddUriScheme(string recipeLink)
        {
            var uriBuilder = new UriBuilder
            {
                Host = String.Empty,

                Scheme = "http",

                Path = recipeLink
            };

            return uriBuilder.Uri;
        }
    }

    public static class ExtensionMethods
    {

        // the method returns an IEnumberal of general type TSource
        // and by naming it DistinctBy, an existing method, we make it an extension method
        // the extension method's parameters are TSource and TKey
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>

        // call the IEnumberal<TSource> source; this is the source input; for example List<nER>
        // keyselector will take in any general source and return a general key, TKey
        (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            // make a new instance of the Hashset<> class, hashing of general type TKey
            HashSet<TKey> seenKeys = new();
            // for each element of general source type, TSource, in source
            foreach (TSource element in source)
            // because of the lambda expression x => x.property, every element in source is evaluated as element.property
            {
                // retun the value of element.property as a general value and add it to the hash set
                if (seenKeys.Add(keySelector(element)))
                {
                    // if successful,  yield and return the element;
                    // if not successful, move on
                    // yield, iterate again, and wrap it all in an IEnumberable
                    yield return element;
                }
            }
        }
    }
}