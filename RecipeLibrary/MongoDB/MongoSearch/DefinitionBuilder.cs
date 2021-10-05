using MongoDB.Driver;
using RecipeLibrary.MongoDB.Recipes;
using System;
using System.Linq.Expressions;

namespace RecipeLibrary.MongoDB.MongoSearch
{
    public class DefinitionBuilder<TDocument>
    {
        public FilterDefinition<TDocument> Filter { get; set; }
        public SortDefinition<TDocument> Sort { get; set; }
        public static DefinitionBuilder<TDocument> MakeMongoEqualitySearch(Expression<Func<TDocument, Object>> field, Object value, string sort)
        {
                
            var mongoSearchLogic = new DefinitionBuilder<TDocument>
            {

                Filter = Builders<TDocument>.Filter.Eq(field, value),

                Sort = Builders<TDocument>.Sort.Ascending(sort),

            };

            // return the MongoRecipe definition builder
            return mongoSearchLogic;

        }
        

    }

}