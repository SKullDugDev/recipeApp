using System;
using System.Collections.Generic;

namespace RecipeLibrary.Extensions
{
    public static class DistinctByExtension
    {

        /// <summary>
        // the method returns an IEnumberal of general type TSource
        // and by naming it DistinctBy, an existing method, we make it an extension method
        // the extension method's parameters are TSource and TKey
        // call the IEnumberal<TSource> source; this is the source input; for example List<nER>
        // keyselector will take in any general source and return a general key, TKey
        // make a new instance of the Hashset<> class, hashing of general type TKey
        // for each element of general source type, TSource, in source
        // because of the lambda expression x => x.property, every element in source is evaluated as element.property
        // retun the value of element.property as a general value and add it to the hash set
        // if successful,  yield and return the element;
        // if not successful, move on
        // yield, iterate again, and wrap it all in an IEnumberable
        /// </summary>
        /// <typeparam name="TSource"> a general source type such as List<Ingredient> </typeparam>
        /// <typeparam name="TKey"> a general key type </typeparam>
        /// <param name="source"> the input source object </param>
        /// <param name="keySelector"> take in a general source and return a general key </param>
        /// <returns> IEnumberal<typeparamref name="TSource"/>; an IEnumberal of a general type TSource </returns>

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>

        (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            
            HashSet<TKey> seenKeys = new();
            
            foreach (TSource element in source)
           
            {
                
                if (seenKeys.Add(keySelector(element)))
                {

                    yield return element;
                }
            }
        }
    }
}
