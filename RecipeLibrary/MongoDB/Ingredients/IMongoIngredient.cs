using System;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;
using RecipeLibrary.Ingredients;
using RecipeLibrary.MongoDB.Recipes;
using RecipeLibrary.MongoDB.Extensions;
using RecipeLibrary.MongoDB.MongoSearch;
using MongoDB.Bson.Serialization.Attributes;

namespace RecipeLibrary.MongoDB.Ingredients
{
    public class IMongoIngredient : IIngredient
    {
        [BsonId]
        public ObjectId ObjectID { get; set; }

        [BsonElement("RecipeID")]
        public int RecipeID { get; set; }

        [BsonElement("IngredientID")]
        public int IngredientID { get; set; }

        [BsonElement("NamedEntity")]
        public string NamedEntity { get; set; }

        public static async Task MakeIngredientJSONFromRecipes(string connectionString)
        {

            // get the recipe database from the connection string
            var recipesDB = connectionString.GetRecipeDatabase();
            Console.WriteLine("Accessed the Recipes Database from the Client...");

            // from the recipes database, get the recipes collection and connect it to the mapping recipe entity class
            var recipeCollection = recipesDB.GetCollection<IMongoRecipe>("recipes");
            Console.WriteLine("Recipe Collection found and mapped...");

            // in order to better performance when sorting and querying, we need to create an index for our recipe collection
            string recipeIndexName = await recipeCollection.BuildDocumentIndex(recipe => recipe.RecipeID);
            Console.WriteLine($"{recipeIndexName} Index built for Recipe Collection...");

            // get or create the ingredient collection and connect it to the mapping ingredient entity class
            var ingredientCollection = recipesDB.GetCollection<IMongoIngredient>("ingredients");
            Console.WriteLine("Ingredient Collection found and mapped...");

            // create an index for the ingredients collection
            string ingredientIndexName = await ingredientCollection.BuildDocumentIndex(ingredient => ingredient.IngredientID);
            Console.WriteLine($"{ingredientIndexName} Index built for Ingredient Collection...");

            // create a unique index on the named entity property so it won't accept duplicates
            string uniqueIngredientIndexName = await ingredientCollection.BuildUniqueDocumentIndex(ingredient => ingredient.NamedEntity);
            Console.WriteLine($"{uniqueIngredientIndexName} Unique Index built for Ingredient Collection...");

            // get the recipe definition builder so we can find and sort the documents properly
            var mongoSearchLogic = DefinitionBuilder.GetMongoSearchLogic();
            Console.WriteLine("Sorting and filtering logic determined...");

            // start off recipe count as 0
            int totalRecipeCount = 0;

            // start off ingredient count as 0
            int totalIngredientCount = 0;

            // start off duplicate count at 0
            int totalDuplicateCount = 0;

            //from the collection, find only documents that match the filter, sort them by ascending RecipeID,
            //asynchronously return a cursor variable that holds the memory address of the documents; the cursor is just a pointer
            //implement the cursor via a using statement to properly dispose of it

            Console.WriteLine("Initial sorting through collection in progress...");

            using IAsyncCursor<IMongoRecipe> cursor = await recipeCollection.Find(mongoSearchLogic.Filter, mongoSearchLogic.Options).Sort(mongoSearchLogic.Sort).ToCursorAsync();

            (int finalRecipeCount, int finalIngredientCount, int finalDuplicateCount) = await cursor.ProcessRecipesFromCursor(ingredientCollection);

            totalRecipeCount = finalRecipeCount;
            totalIngredientCount = finalIngredientCount;
            totalDuplicateCount = finalDuplicateCount;

            Console.WriteLine($"{totalRecipeCount} recipes processed in total...");
            Console.WriteLine($"{totalIngredientCount} recipes processed in total...");
            Console.WriteLine($"{totalDuplicateCount} duplicates ignored in total...");

        }

    }
}
