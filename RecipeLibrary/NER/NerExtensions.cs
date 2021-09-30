
using System.Text.Json;


namespace RecipeLibrary.NER
{
    public static class NerExtensions
    {

        public static string[] ToNamedEntitiesArray(this string namedEntities)
        {

            return JsonSerializer.Deserialize<string[]>(namedEntities);

        }
    }
}
