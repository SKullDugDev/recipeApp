using MongoDB.Driver;
using RecipeLibrary.Recipes.MongoDB;

namespace RecipeLibrary.MongoSearch
{
    public class DefinitionBuilder
    {
        public FilterDefinition<MongoRecipe> Filter { get; set; }
        public SortDefinition<MongoRecipe> Sort { get; set; }
        public FindOptions Options { get; set; }
        public static DefinitionBuilder GetMongoSearchLogic()
        {
            // implement a filter definition builder for the MongoRecipe entity class 
            var filterBuilder = Builders<MongoRecipe>.Filter;

            // implement a sort definition builder for the MongoRecipe entity class
            var sortBuilder = Builders<MongoRecipe>.Sort;

            var optionBuilder = new FindOptions()
            {
                BatchSize = 101
            };

            // initiate a new instance of the MongoRecipe defintion builder class
            var mongoSearchLogic = new DefinitionBuilder
            {

                // use a filter so we can sort through all the documents with the Gathered source
                // store it in the Filter property of the MongoRecipe definition builder
                Filter = filterBuilder.Eq(MongoRecipe => MongoRecipe.Source, "Gathered"),

                // create a sort definiton logic to sort by RecipeID in ascending order
                // store it in the Sort property of the MongoRecipe defintion builder
                Sort = sortBuilder.Ascending("RecipeID"),

                Options = optionBuilder

            };

            // return the MongoRecipe definition builder
            return mongoSearchLogic;

        }

    }

}