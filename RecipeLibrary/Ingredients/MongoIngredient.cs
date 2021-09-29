using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using RecipeLibrary.Recipes.MongoDB;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RecipeLibrary.Ingredients.MongoDB
{
    public class MongoIngredient : IIngredient
    {
        [BsonId]
        public string ObjectID { get; set; }

        [BsonElement("RecipeID")]
        public int RecipeID { get; set; }

        [BsonElement("IngredientID")]
        public int IngredientID { get; set; }

        [BsonElement("NamedEntity")]
        public string NamedEntity { get; set; }


        public static async Task<int> GetIngredientsFromCursor(IAsyncCursor<MongoRecipe> cursor, List<MongoIngredient> ingredients)
        {

            // start off with no recipes processed
            var recipesProcessedCount = 0;

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

            Console.WriteLine("Checking recipes for ingredients...");
            while (!cursorEmpty)
            {
                recipesProcessedCount += await MongoRecipe.ProcessRecipesFromCursor(cursor, cursorEmpty, ingredients);
                Console.WriteLine("{0} recipes processed thus far...", recipesProcessedCount);

            }

            // if at the start when we do cursor.MoveNextAsync() there are no docs in the first batch
            // check first to see if ingredients is empty as it should be if the while loop never ran
            // by doing this we should never throw an exception cuz of the loop, only the empty first batch

            if (recipesProcessedCount == 0 && cursorEmpty)
            {
                string exceptionMessage = "Cursor has no documents...Please check the collection or the cursor logic...";
                throw new ArgumentException(exceptionMessage);
            }

            return recipesProcessedCount;
        }


        public static void BuildIngredient(string item, MongoRecipe recipe, List<MongoIngredient> ingredients)
        {
            // store item locally as namedEntity
            string namedEntity = item;

            // strip anything not basic latin from string using Regex
            namedEntity = Regex.Replace(namedEntity, @"[^\u0000-\u007F]", String.Empty);

            // initiate a new instance of the ingredient class
            MongoIngredient ingredient = new()
            {

                // assign the ingredient objectid to the objectid of the set
                ObjectID = recipe.ObjectId.ToString(),

                // assign the ingredient recipeid to the recipeid of the set
                RecipeID = recipe.RecipeID,

                // assign the ingredient a named entity
                NamedEntity = namedEntity
            };

            // add the ingredient object to the list of ingredients
            ingredients.Add(ingredient);
        }


        public static void ParseIngredients(List<string> ingredientStrings, List<MongoIngredient> ingredients)
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

            foreach (MongoIngredient ingredient in ingredients)
            {
                // assign ingredient an id equal to the value of i
                ingredient.IngredientID = i;

                // serialize the individual ingredient object into what it should be, a JSON string and store it as a string variable
                string ingredientString = JsonSerializer.Serialize<MongoIngredient>(ingredient, options);

                // add serialized ingredient string to the ingredient strings list
                ingredientStrings.Add(ingredientString);

                // increase the number of i by 1
                i++;
            };
        }


        public static async Task IngredientBuilder(Configuration.ConfigResults configurationResults)
        {
            // get the recipe collection from the database
            var recipeCollection = MongoRecipe.GetRecipeCollection(configurationResults.Settings);

            Console.WriteLine("Recipe Collection found and mapped...");

            // in order to better performance when sorting and querying, we need to create an index for our recipe collection
            await MongoRecipe.RecipeIndexBuilder(recipeCollection);

            Console.WriteLine("Recipe Index built...");

            // create an empty list of ingredient objects to collect the ingredients
            List<MongoIngredient> ingredients = new();

            // create an empty list of strings to collect the ingredient serialized string
            List<string> ingredientStrings = new();

            // get the recipe definition builder so we can find and sort the documents properly
            var mongoSearchLogic = MongoSearch.DefinitionBuilder.GetMongoSearchLogic();
            Console.WriteLine("Sorting and filtering logic determined...");

            // start off recipe count as 0
            int totalRecipeCount = 0;

            Console.WriteLine("Initial sorting through collection in progress...");

            //from the collection, find only documents that match the filter, sort them by ascending RecipeID,
            //asynchronously return a cursor variable that holds the memory address of the documents; the cursor is just a pointer
            //implement the cursor via a using statement to properly dispose of it

            using (IAsyncCursor<MongoRecipe> cursor = await recipeCollection.Find(mongoSearchLogic.Filter).Sort(mongoSearchLogic.Sort).ToCursorAsync())
            {

                totalRecipeCount = await GetIngredientsFromCursor(cursor, ingredients);

            }

            Console.WriteLine("{0} recipes processed in total...", totalRecipeCount);

            Console.WriteLine("Parsing through ingredients...");
            ParseIngredients(ingredientStrings, ingredients);

            Console.WriteLine("Forming final JSON object string");

            // take all the ingredient json objects in the ingredient json list and combine them into one json string containing all ingredients
            string ingredientsJSON = String.Join(",", ingredientStrings);

            Console.WriteLine(ingredientsJSON);
            Console.WriteLine("Breakpoint");
            return;

        }


    }
}
