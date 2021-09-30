
using System;

namespace RecipeLibrary.Web
{
    class UriHandler
    {
        public static Uri MakeRecipeURI(string recipeLink)
        {
            var uriBuilder = new UriBuilder
            {
                Host = String.Empty,

                Scheme = "http",

                Path = recipeLink
            };

            return uriBuilder.Uri;
        }
    }
}
