
using System;
using MongoDB.Driver;
using RecipeLibrary.NER;
using System.Threading.Tasks;
using System.Collections.Generic;
using RecipeLibrary.MongoDB.Recipes;
using RecipeLibrary.MongoDB.Ingredients;

namespace RecipeLibrary.MongoDB.Extensions
{
    public static class RecipeExtensions
    {
        // gets the recipe collection
        public static IMongoDatabase GetRecipeDatabase(this string connectionString)
        {

            // create a new instance of the MongoDB Client server and call it client
            var client = new MongoClient(connectionString);
            Console.WriteLine("Formed Client connection to MongoDB Server...");

            // from the client server get and return the recipes database                        
            return client.GetDatabase("recipes");

        }

        // builds an index for the recipe
        //public static async Task<string> RecipeIndexBuilder(this IMongoCollection<IMongoRecipe> recipeCollection)
        //{

        //    // first use a builder to start up the index key logic; then make an indexModel to hold the
        //    var recipeIndexBuilder = Builders<IMongoRecipe>.IndexKeys;

        //    // then create an index model to establish the index logic: index using RecipeID in an ascending order
        //    var indexModel = new CreateIndexModel<IMongoRecipe>(recipeIndexBuilder.Ascending(recipe => recipe.RecipeID));

        //    // finally add the index to the collection and the name of the index
        //    return await recipeCollection.Indexes.CreateOneAsync(indexModel); ;
        //}

        // add ingredients to a list from the list of recipes
        ///

        // add ingredients from recipes
        public static async Task<(int newRecipeCount, int newIngredientCount, int newDuplicateCount)> AddIngredientsFromRecipes(this IEnumerable<IMongoRecipe> recipes, int recipeCount, int ingredientCount, int duplicateCount, IMongoCollection<IMongoIngredient> ingredientCollection)
        {

            // for each recipe of type MongoRecipe in the recipes list
            foreach (IMongoRecipe recipe in recipes)
            {

                // get the named entitiy recognition information string
                string NER = recipe.NER;

                // get the array of named entities
                string[] namedEntities = NER.ToNamedEntitiesArray();

                // for each named entity in the array of named objects
                foreach (string item in namedEntities)
                {

                    // store item locally as namedEntity
                    string namedEntity = item;

                    // initiate a new instance of the ingredient class
                    IMongoIngredient ingredient = new()
                    {

                        // no need to assign an object id in this case, the db will make one for us
                        // assign the ingredient objectid to the objectid of the set
                        //ObjectID = recipe.ObjectId.ToString(),

                        // assign the ingredient recipeid to the recipeid of the set
                        RecipeID = recipe.RecipeID,

                        // assign the ingredient an id based on the ingredient count
                        IngredientID = ingredientCount,

                        // assign the ingredient a named entity
                        NamedEntity = namedEntity
                    };

                    // try to insert ingredient into collection
                    // if we succeed increase the ingredient count by 1
                    // if it fails on a duplicate in the way we expect, add to the duplicate count
                    // if it fails in a way we don't expect, either throw it to the outer method or it'll catch it itself

                    try
                    {

                        // insert ingredient into collection
                        await ingredientCollection.InsertOneAsync(ingredient);

                        // increase the number of ingredients by 1
                        ingredientCount++;

                    }
                    catch (Exception e) when (e is MongoWriteException mongoWriteException)
                    // catch when the exception is an expected MongoWriteException
                    {
                        // if it's the expected duplicate error, add to the count and move on
                        if (mongoWriteException.WriteError.Code == 11000)
                        {
                            duplicateCount++;
                        }
                        else
                        // otherwise throw back the original error
                        {
                            throw;
                        }

                    }
                }

                // increase the count of recipes processed
                recipeCount++;
            }


            // check to make sure it the list of ingredients doesn't have copies
            // then fix the ingredient list so it includes only the distinct ingredients
            //ingredients = ingredients.DistinctBy(ingredient => ingredient.NamedEntity).ToList();

            // set the cursor empty check variable to the opposite of the result of the move
            // if there is more, then cursor empty will be false and the loop continues
            // if there is no more, then cursor empty will be true and the loop ends            
            return (recipeCount, ingredientCount, duplicateCount);

        }

        // process recipes from cursor
        public static async Task<(int finalRecipesCount, int finalIngredientCount, int finalDuplicateCount)> ProcessRecipesFromCursor(this IAsyncCursor<IMongoRecipe> cursor, IMongoCollection<IMongoIngredient> ingredientCollection)
        {

            // start off with no recipes processed
            var recipeCount = 0;

            // start off with no ingredients processed
            var ingredientCount = 0;

            // start off with no duplicates
            var duplicateCount = 0;

            // when ToCursorAsync is used, the cursor originally has no content
            // MoveNextAsync()/something similar needs to be called to get the first batch of docs
            // MoveNextAsync() returns true if there are more docs to be avail and false otherwise
            // we create a bool cursorEmpty to check if the cursor is ever empty
            // then we assign it to the opposite of the result we get from moving the cursor
            // we catch and explain a fail condition later below for when it's empty at the start

            Console.WriteLine("Checking cursor for recipes...");
            bool cursorEmpty = !await cursor.MoveNextAsync();

            // while it is true that the cursor is not empty, run
            // can also be read as: while (not cursorEmpty) evaluates as true, run
            
            Console.WriteLine("Checking recipes for ingredients...");
            while (!cursorEmpty)
            {
                // get the current documents in the cursor and place it in am IEnumeral of type MongoRecipe
                IEnumerable<IMongoRecipe> recipes = cursor.Current;

                // add ingredients to a list from the list of recipes
                // second batch goes in with ingredientCount at 5
                (int newRecipeCount, int newIngredientCount, int newDuplicateCount) = await recipes.AddIngredientsFromRecipes(recipeCount, ingredientCount, duplicateCount, ingredientCollection);

                // recipeCount is always the number processed this round
                // recipesProcessedCount accumulates by adding recipeCount
                recipeCount = newRecipeCount;

                ingredientCount = newIngredientCount;

                duplicateCount = newDuplicateCount;

                // move to next batch and check if there is more
                cursorEmpty = !await cursor.MoveNextAsync();

                Console.WriteLine($"{recipeCount} recipes processed thus far...");
                Console.WriteLine($"{ingredientCount} ingredients added thus far...");
                Console.WriteLine($"{duplicateCount} duplicates not added so far...");

            }

            // if at the start when we do cursor.MoveNextAsync() there are no docs in the first batch
            // check first to see if ingredients is empty as it should be if the while loop never ran
            // by doing this we should never throw an exception cuz of the loop, only the empty first batch

            if (recipeCount == 0 && cursorEmpty)
            {
                string exceptionMessage = "Cursor has no documents...Please check the collection or the cursor logic...";
                throw new ArgumentException(exceptionMessage);
            }

            return (recipeCount, ingredientCount, duplicateCount);
        }

    }
}
