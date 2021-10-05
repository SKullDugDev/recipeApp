
namespace RecipeLibrary.Recipes
{
    public interface IRecipe
    {

        public int RecipeId { get; set; }

        public string Title { get; set; }

        public string Ingredients { get; set; }

        public string Directions { get; set; }

        public string Link { get; set; }

        public string Source { get; set; }

        public string NER { get; set; }

    }
}