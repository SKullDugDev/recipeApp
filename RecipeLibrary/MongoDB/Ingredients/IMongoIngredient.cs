using System;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;
using RecipeLibrary.Ingredients;
using RecipeLibrary.MongoDB.Recipes;
using RecipeLibrary.MongoDB.Extensions;
using RecipeLibrary.MongoDB.MongoSearch;
using MongoDB.Bson.Serialization.Attributes;
using RecipeLibrary.MongoDB.Exceptions;

namespace RecipeLibrary.MongoDB.Ingredients
{
    public class IMongoIngredient : IIngredient<ObjectId>
    {

        [BsonId]
        public ObjectId IngredientId { get; set; }


        [BsonElement("RecipeId")]
        public int RecipeId { get; set; }


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
            string recipeIndexName = await recipeCollection.BuildDocumentIndex(recipe => recipe.RecipeId);
            Console.WriteLine($"{recipeIndexName} Index built for Recipe Collection...");

            // get or create the ingredient collection and connect it to the mapping ingredient entity class
            var ingredientCollection = recipesDB.GetCollection<IMongoIngredient>("ingredients");
            Console.WriteLine("Ingredient Collection found and mapped...");

            // create an index for the ingredients collection on the id
            string ingredientIdIndexName = await ingredientCollection.BuildDocumentIndex(ingredient => ingredient.IngredientId);
            Console.WriteLine($"{ingredientIdIndexName} Index built for Ingredient Collection...");

            // create an index for the ingredients collection on the recipeId
            string ingredientRecipeIdIndexName = await ingredientCollection.BuildDocumentIndex(ingredient => ingredient.RecipeId);
            Console.WriteLine($"{ingredientRecipeIdIndexName} Index built for Ingredient Collection...");

            // create a unique index on the named entity property so it won't accept duplicates
            string uniqueIngredientIndexName = await ingredientCollection.BuildUniqueDocumentIndex(ingredient => ingredient.NamedEntity);
            Console.WriteLine($"{uniqueIngredientIndexName} Unique Index built for Ingredient Collection...");

            // get the recipe definition builder so we can find and sort the documents properly
            var mongoRecipeSearchLogic = DefinitionBuilder<IMongoRecipe>.MakeMongoEqualitySearch(recipe => recipe.Source, "Gathered", "RecipeId");
            Console.WriteLine("Sorting and filtering logic determined...");

            // start off recipe count as 0
            int totalRecipeCount = 0;

            // start off ingredient count as 0
            int totalIngredientCount = 0;

            // start off duplicate count at 0
            int totalDuplicateCount = 0;

            // start off skip count as 0
            int skipCount = 0;

            // the try catch has to run until the cursor is empty proper
            // we need a while loop to run it until the cursor is empty
            // if the cursor starts off empty, it throws a handled exception
            // ProcessRecipesFromCursor finishes only when the cursor is empty
            // the cursor should only be empty after the method finishes running
            // we can make a start condition that changes when the method ends
            // create a bool variable, set it to true, and call it startSort
            bool startSort = true;

            while (startSort)
            {

                try
                {

                    Console.WriteLine("Initial sorting through collection in progress...");

                    //from the collection, find only documents that match the filter, sort them by ascending RecipeId,
                    //asynchronously return a cursor variable that holds the memory address of the documents; the cursor is just a pointer
                    //implement the cursor via a using statement to properly dispose of it

                    using IAsyncCursor<IMongoRecipe> cursor = await recipeCollection.Find(mongoRecipeSearchLogic.Filter).Sort(mongoRecipeSearchLogic.Sort).Skip(skipCount).ToCursorAsync();

                    (int finalRecipeCount, int finalIngredientCount, int finalDuplicateCount) = await cursor.ProcessRecipesFromCursor(ingredientCollection);

                    totalRecipeCount = finalRecipeCount;
                    totalIngredientCount = finalIngredientCount;
                    totalDuplicateCount = finalDuplicateCount;
                    
                    Console.WriteLine($"{totalRecipeCount} recipes processed in total...");
                    Console.WriteLine($"{totalIngredientCount} ingredients processed in total...");
                    Console.WriteLine($"{totalDuplicateCount} duplicate ingredients ignored in total...");

                    startSort = false;

                }
                catch (MongoRecipeCursorGoneException e)
                {
                    skipCount = e.RecipeCount;
                }
                catch (MongoConnectionException e)
                {

                    string connectionErrorMessage = $"Connection Id: {e.ConnectionId}";
                    Console.WriteLine(connectionErrorMessage);
                    throw;

                }
            }

        }

    }
}
