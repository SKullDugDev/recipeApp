
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RecipeLibrary.Recipes.MongoDB
{
    public class IMongoRecipe : IRecipe
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

    }

}

