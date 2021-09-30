using System;
using MongoDB.Driver;
using System.Threading.Tasks;
using RecipeLibrary.MongoSearch;
using System.Collections.Generic;
using RecipeLibrary.Recipes.MongoDB;
using MongoDB.Bson.Serialization.Attributes;

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

        public static async Task<string> MakeIngredientJSONFromRecipes(string connectionString)
        {

            // get the recipe collection from the database
            var recipeCollection = connectionString.GetRecipeCollection();
            Console.WriteLine("Recipe Collection found and mapped...");

            // in order to better performance when sorting and querying, we need to create an index for our recipe collection
            string indexName = await recipeCollection.RecipeIndexBuilder();
            Console.WriteLine("{0} Index built...", indexName);

            // create an empty list of ingredient objects to collect the ingredients
            List<MongoIngredient> ingredients = new();

            // create an empty list of strings to collect the ingredient serialized string
            List<string> ingredientStrings = new();

            // get the recipe definition builder so we can find and sort the documents properly
            var mongoSearchLogic = DefinitionBuilder.GetMongoSearchLogic();
            Console.WriteLine("Sorting and filtering logic determined...");

            // start off recipe count as 0
            int totalRecipeCount = 0;

            Console.WriteLine("Initial sorting through collection in progress...");

            //from the collection, find only documents that match the filter, sort them by ascending RecipeID,
            //asynchronously return a cursor variable that holds the memory address of the documents; the cursor is just a pointer
            //implement the cursor via a using statement to properly dispose of it

            using (IAsyncCursor<IMongoRecipe> cursor = await recipeCollection.Find(mongoSearchLogic.Filter, mongoSearchLogic.Options).Sort(mongoSearchLogic.Sort).ToCursorAsync())
            {

                totalRecipeCount = await cursor.ProcessRecipesFromCursor(ingredients);

            }

            Console.WriteLine("{0} recipes processed in total...", totalRecipeCount);

            Console.WriteLine("Parsing through ingredients...");

            ingredients.ParseIngredients(ingredientStrings);            

            Console.WriteLine("Forming final JSON object string");

            // take all the ingredient json objects in the ingredient json list and combine them into one json string containing all ingredients
            string ingredientsJSON = String.Join(",", ingredientStrings);

            return ingredientsJSON;

        }

    }
}
