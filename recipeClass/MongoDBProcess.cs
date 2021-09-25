using System;
using System.IO;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MongoDB.Bson.Serialization.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace recipeClass
{
    public class MongoDBProcess
    {
        public static async Task IngredientBuilder(Entities.MongoArgs mongoArgs)
        {
            // get the recipe collection from the database
            var recipeCollection = Helpers.GetRecipeCollection(mongoArgs);

            Console.WriteLine("Recipe Collection found and mapped...");

            // in order to better performance when sorting and querying, we need to create an index for our recipe collection
            await Helpers.RecipeIndexBuilder(recipeCollection);

            Console.WriteLine("Recipe Index built...");

            // create an empty list of ingredient objects to collect the ingredients
            List<Entities.Ingredient> ingredients = new List<Entities.Ingredient>();

            // create an empty list of strings to collect the ingredient serialized string
            List<string> ingredientStrings = new List<string>();

            // get the recipe definition builder so we can find and sort the documents properly
            var recipeDefinitionBuilder = Helpers.GetRecipeDefinitionBuilder();

            // start off recipe count as 0
            int recipeCount = 0;

            Console.WriteLine("Sorting through collection, gathering named entities, and forming a list of processed ingredients...");

            //from the collection, find only documents that match the filter, sort them by ascending RecipeID,
            //asynchronously return a cursor variable that holds the memory address of the documents; the cursor is just a pointer
            //implement the cursor via a using statement to properly dispose of it
            using (IAsyncCursor<Entities.Recipe> cursor = await recipeCollection.Find(recipeDefinitionBuilder.Filter).Sort(recipeDefinitionBuilder.Sort).ToCursorAsync())
            {

                // create a check to see if the cursor is empty
                // start it off as false at the start; it should never start off as empty
                bool cursorEmpty = !await cursor.MoveNextAsync();

                // while it is true that the cursor is not empty, run
                // can also be read as: while (not cursorEmpty) evaluates as true, run
                while (!cursorEmpty)
                {
                    // get the current documents in the cursor and place it in an IEnumberal of type Entities.Recipe named recipes
                    IEnumerable<Entities.Recipe> recipes = cursor.Current;

                    // for each recipe of type Entities.Recipe in the recipes list
                    foreach (Entities.Recipe recipe in recipes)
                    {

                        // get the set of named entities
                        var namedEntitiesSet = await Helpers.GetNamedEntitiesSet(recipe);

                        // for each named entity in the array of named entities
                        foreach (string item in namedEntitiesSet)
                        {

                            Helpers.BuildIngredient(item, recipe, ingredients);

                        }

                        recipeCount++;

                    };

                    // check to make sure it the list of ingredients doesn't have copies
                    // then fix the ingredient list so it includes only the distinct ingredients
                    ingredients = ingredients.DistinctBy(ingredient => ingredient.NamedEntity).ToList();

                    // set the cursor empty check variable to the opposite of the result of the move
                    // if there is more, then cursor empty will be false and the loop continues
                    // if there is no more, then cursor empty will be true and the loop ends
                    cursorEmpty = !await cursor.MoveNextAsync();

                }
            };

            // set the JSONSerliazerOptions first to the default for the web and then override it to include fields and indentation
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            options.IncludeFields = true;
            options.WriteIndented = true;

            // start off i as 0
            int i = 0;

            Console.WriteLine("Ingredients gathered into list...assigning id and transforming into JSON string...");

            foreach (Entities.Ingredient ingredient in ingredients)
            {
                // assign ingredient an id equal to the value of i
                ingredient.IngredientID = i;

                // serialize the individual ingredient object into what it should be, a JSON string and store it as a string variable
                string ingredientString = JsonSerializer.Serialize<Entities.Ingredient>(ingredient, options);

                // add serialized ingredient string to the ingredient strings list
                ingredientStrings.Add(ingredientString);

                // increase the number of i by 1
                i++;
            };

            Console.WriteLine("Forming final JSON object string");

            // take all the ingredient json objects in the ingredient json list and combine them into one json string containing all ingredients
            string ingredientsJSON = String.Join(",", ingredientStrings);

            Console.WriteLine(ingredientsJSON);

            return;

        }


    }

    // This is an internal class of custom entities/objects
    public class Entities
    {
        // a class of settings
        public class Settings
        {
            // the configuration settings
            public class ConfigurationSettings
            {
                public Fields.Logging Logging { get; set; }

                public Fields.AppSettings AppSettings { get; set; }

                public override string ToString()
                {
                    return JsonSerializer.Serialize(this, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                }
            }


            // a class of setting fields
            public class Fields
            {
                // a defintion for the mapping of the logging section
                public class Logging
                {
                    public ConsoleSettings Console { get; set; }

                    public bool IncludeScopes { get; set; }

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
                        }
                    }
                }

                public class AppSettings
                {
                    public MongoDBSettings MongoDBInfo { get; set; }

                    public class MongoDBSettings
                    {
                        public string host { get; set; }
                        public int port { get; set; }
                        public string mongodbpass { get; set; }
                    }
                }
            }
        }

        public class ConfigurationResults
        {
            public ServiceProvider ServiceProvider { get; set; }

            public Settings.ConfigurationSettings Settings { get; set; }

        }

        public class MongoArgs
        {
            public string mongoDBHost { get; set; }
            public int mongoDBPort { get; set; }
            public string mongoDBPass { get; set; }
        }
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

    public static class Helpers
    {
        // this method sets up the configuration for the console
        public static Entities.ConfigurationResults ConfigurationSetup()
        {
            // intiate a new configuration builder to create our configurations
            var configuration = new ConfigurationBuilder()
                // add json file, appsettings.json; mark as not optional; reload on change
                .AddJsonFile("appsettings.json", false, true)
                // add user secrets to the RecipeConsole class
                .AddUserSecrets<MongoDBProcess>()
                // build our configurations
                .Build();

            // from our configurations, get our configurations
            // they should come in the form established in Con
            Entities.Settings.ConfigurationSettings settings = configuration.Get<Entities.Settings.ConfigurationSettings>();

            // setup logging
            var services = new ServiceCollection() as IServiceCollection;

            // adding logging as a service using the logging configuration
            // this includes adding the configuration for the console from logging
            // now that configure has the console settings, add the appropriate console logger
            services.AddLogging(configure =>
            {
                configure.AddConfiguration(configuration.GetSection("Logging"));
                configure.AddConsole();
            });

            // build the services provider
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            // create a new instance of the ConfigurationResults object/class
            Entities.ConfigurationResults configurationResults = new Entities.ConfigurationResults();

            // store the service provider in configurationResults under the ServiceProvider property
            configurationResults.ServiceProvider = serviceProvider;

            // store the settings in configurationResults under the Settings property
            configurationResults.Settings = settings;

            // return the configurationResults object
            return configurationResults;
        }


        public static Entities.MongoArgs GetMongoArgs(Entities.Settings.ConfigurationSettings settings)
        {
            // create a new instance of MongoArgs object to hold arguments
            Entities.MongoArgs mongoArgs = new Entities.MongoArgs();

            // store host from appsettings in settings into mongoArgs
            mongoArgs.mongoDBHost = settings.AppSettings.MongoDBInfo.host;

            // store port from appsettings in settings into mongoArgs
            mongoArgs.mongoDBPort = settings.AppSettings.MongoDBInfo.port;

            // store password from appsettings in settings into mongoArgs
            mongoArgs.mongoDBPass = Uri.EscapeDataString(settings.AppSettings.MongoDBInfo.mongodbpass);

            // return the mongoArgs object
            return mongoArgs;
        }

        public static IMongoCollection<Entities.Recipe> GetRecipeCollection(Entities.MongoArgs mongoArgs)
        {
            // form connection string for communicating with mongoDB
            string connectionString = $"mongodb://SKDev:{mongoArgs.mongoDBPass}@{mongoArgs.mongoDBHost}:{mongoArgs.mongoDBPort}/?authSource=admin";
            Console.WriteLine("Connection string formed...");

            // create a new instance of the MongoDB Client server and call it client
            var client = new MongoClient(connectionString);
            Console.WriteLine("Formed Client connection to MongoDB Server");

            // from the client server get the recipes database
            var recipesDB = client.GetDatabase("recipes");
            Console.WriteLine("Accessed Recipes Database from Client...");

            // from the recipes database, get the recipes collection and connect it to the mapping recipe entity class
            return recipesDB.GetCollection<Entities.Recipe>("recipes");
        }

        public static async Task RecipeIndexBuilder(IMongoCollection<Entities.Recipe> recipeCollection)
        {

            // first use a builder to start up the index key logic; then make an indexModel to hold the
            var recipeIndexBuilder = Builders<Entities.Recipe>.IndexKeys;

            // then create an index model to establish the index logic: index using RecipeID in an ascending order
            var indexModel = new CreateIndexModel<Entities.Recipe>(recipeIndexBuilder.Ascending(recipe => recipe.RecipeID));

            // finally add the index to the collection
            await recipeCollection.Indexes.CreateOneAsync(indexModel);
        }

        public static Entities.RecipeDefinitionBuilder GetRecipeDefinitionBuilder()
        {
            // implement a filter definition builder for the recipe entity class 
            var filterBuilder = Builders<Entities.Recipe>.Filter;

            // implement a sort definition builder for the recipe entity class
            var sortBuilder = Builders<Entities.Recipe>.Sort;

            // initiate a new instance of the recipe defintion builder class
            var recipeDefinitionBuilder = new Entities.RecipeDefinitionBuilder();

            // use a filter so we can sort through all the documents with the Gathered source
            // store it in the Filter property of the recipe definition builder
            recipeDefinitionBuilder.Filter = filterBuilder.Eq(recipe => recipe.Source, "Gathered");

            // create a sort definiton logic to sort by RecipeID in ascending order
            // store it in the Sort property of the recipe defintion builder
            recipeDefinitionBuilder.Sort = sortBuilder.Ascending("RecipeID");

            // return the recipe definition builder
            return recipeDefinitionBuilder;
        }

        public static async Task<string[]> GetNamedEntitiesSet(Entities.Recipe recipe)
        {
            // we recieve the recipe, store the named entities of that recipe
            string namedEntities = recipe.NER;

            // from the string of named entities, create a UTF-8 encoded byte array using GetBytes();
            // from this, create a new memory stream
            var namedEntitesStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(namedEntities));

            // asynchronously deserialize the stream made from the string and put it in the form of an array of strings
            string[] namedEntitiesSet = await JsonSerializer.DeserializeAsync<string[]>(namedEntitesStream);

            return namedEntitiesSet;
        }

        public static void BuildIngredient(string item, Entities.Recipe recipe, List<Entities.Ingredient> ingredients)
        {
            // store item locally as namedEntity
            string namedEntity = item;

            // strip anything not basic latin from string using Regex
            namedEntity = Regex.Replace(namedEntity, @"[^\u0000-\u007F]", String.Empty);

            // initiate a new instance of the ingredient class
            Entities.Ingredient ingredient = new Entities.Ingredient();

            // assign the ingredient objectid to the objectid of the set
            ingredient.ObjectID = recipe.ObjectID.ToString();

            // assign the ingredient recipeid to the recipeid of the set
            ingredient.RecipeID = recipe.RecipeID;

            // assign the ingredient a named entity
            ingredient.NamedEntity = namedEntity;

            // add the ingredient object to the list of ingredients
            ingredients.Add(ingredient);
        }

        // the method returns an IEnumberal of general type TSource
        // and by naming it DistinctBy, an existing method, we make it an extension method
        // the extension method's parameters are TSource and TKey
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>

        // call the IEnumberal<TSource> source; this is the source input; for example List<nER>
        // keyselector will take in any general source and return a general key, TKey
        (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            // make a new instance of the Hashset<> class, hashing of general type TKey
            HashSet<TKey> seenKeys = new HashSet<TKey>();
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
