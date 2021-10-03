using System;
using System.Text.Json;
using System.Collections.Generic;
using RecipeLibrary.MongoDB.Ingredients;


namespace RecipeLibrary.MongoDB.Extensions
{
    static class IngredientExtensions
    {

        public static void ParseIngredients(this List<IMongoIngredient> ingredients, List<string> ingredientStrings)
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

            foreach (IMongoIngredient ingredient in ingredients)
            {
                // assign ingredient an id equal to the value of i
                ingredient.IngredientID = i;

                // serialize the individual ingredient object into what it should be, a JSON string and store it as a string variable
                string ingredientString = JsonSerializer.Serialize<IMongoIngredient>(ingredient, options);

                // add serialized ingredient string to the ingredient strings list
                ingredientStrings.Add(ingredientString);

                // increase the number of i by 1
                i++;
            };
        }

    }
}
