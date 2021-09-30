
namespace RecipeLibrary.Ingredients
{
    public interface IIngredient
    {
        public int RecipeID { get; set; }

        public int IngredientID { get; set; }

        public string NamedEntity { get; set; }

    }


}
