
using MongoDB.Bson;
using RecipeLibrary.Recipes;
using MongoDB.Bson.Serialization.Attributes;

namespace RecipeLibrary.MongoDB.Recipes
{
    public class IMongoRecipe : IRecipe
    {
        [BsonId]
        public ObjectId ObjectId { get; set; }


        [BsonElement("recipeId")]
        public int RecipeId { get; set; }


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

}

