
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using RecipeLibrary.Extensions;
using RecipeLibrary.Ingredients.MongoDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;


namespace RecipeLibrary.Recipes.MongoDB
{
    public class MongoRecipe : IRecipe
    {
        [BsonId]
        public ObjectId ObjectId { get; set; }

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


        public static IMongoCollection<MongoRecipe> GetRecipeCollection(Configuration.SettingsConfig settings)
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
            return recipesDB.GetCollection<MongoRecipe>("recipes");
        }

        public static async Task RecipeIndexBuilder(IMongoCollection<MongoRecipe> recipeCollection)
        {

            // first use a builder to start up the index key logic; then make an indexModel to hold the
            var recipeIndexBuilder = Builders<MongoRecipe>.IndexKeys;

            // then create an index model to establish the index logic: index using RecipeID in an ascending order
            var indexModel = new CreateIndexModel<MongoRecipe>(recipeIndexBuilder.Ascending(recipe => recipe.RecipeID));

            // finally add the index to the collection
            await recipeCollection.Indexes.CreateOneAsync(indexModel);
        }

        public static async Task<string[]> GetNamedEntitiesFromRecipe(MongoRecipe recipe)
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





        public static async Task<int> ProcessRecipesFromCursor(IAsyncCursor<MongoRecipe> cursor, bool cursorEmpty, List<MongoIngredient> ingredients)
        {
            // we need to keep track of how many recipes are processed each time this runs, not in total
            var recipeCount = 0;

            // get the current documents in the cursor and place it in an IEnumberal of type Entities.Recipe named recipes
            IEnumerable<MongoRecipe> recipes = cursor.Current;

            // for each recipe of type Entities.Recipe in the recipes list
            foreach (MongoRecipe recipe in recipes)
            {

                // get the set of named objects
                var namedEntitiesSet = await GetNamedEntitiesFromRecipe(recipe);

                // for each named entity in the array of named objects
                foreach (string item in namedEntitiesSet)
                {

                    MongoIngredient.BuildIngredient(item, recipe, ingredients);

                }

                // increase the count of recipes processed
                recipeCount++;
            }



            // check to make sure it the list of ingredients doesn't have copies
            // then fix the ingredient list so it includes only the distinct ingredients
            ingredients = ingredients.DistinctBy(ingredient => ingredient.NamedEntity).ToList();

            // set the cursor empty check variable to the opposite of the result of the move
            // if there is more, then cursor empty will be false and the loop continues
            // if there is no more, then cursor empty will be true and the loop ends
            cursorEmpty = !await cursor.MoveNextAsync();
            return recipeCount;

        }

    }


}

