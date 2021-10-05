using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace RecipeLibrary.MongoDB.Exceptions
{
    [Serializable]
    public class MongoRecipeCursorGoneException : Exception
    {
        public int RecipeCount { get; }

        public MongoRecipeCursorGoneException(
            string message,
            int recipeCount,
            Exception innerException = default)
            : base(message, innerException)
        {
            RecipeCount = recipeCount;
        }
    }
}
