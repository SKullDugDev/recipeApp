using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RecipeLibrary.Recipes
{
    public interface IRecipe
    {

        public int RecipeID { get; set; }

        public string Title { get; set; }
       
        public string Ingredients { get; set; }
       
        public string Directions { get; set; }
     
        public string Link { get; set; }
   
        public string Source { get; set; }

        public string NER { get; set; }

    }
}