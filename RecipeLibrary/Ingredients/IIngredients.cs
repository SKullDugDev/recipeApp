
namespace RecipeLibrary.Ingredients
{
    public interface IIngredient<TIdentification>
    {

        public TIdentification IngredientId { get; set; }

        public int RecipeId { get; set; }
        
        public string NamedEntity { get; set; }

    }


}
