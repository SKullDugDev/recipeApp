//using System;
//using System.IO;
//using System.Linq;
//using MongoDB.Bson;
//using MongoDB.Driver;
//using System.Text.Json;
//using System.Threading.Tasks;
//using System.Collections.Generic;
//using MongoDB.Bson.Serialization.Attributes;


//namespace recipeClass
//{
//    public class MongoDBProcess
//    {
//        public static async Task Connect(string mongoDBHost, int mongoDBPort, string mongoDBPass)
//        {
//            // form connection string for communicating with mongoDB
//            string connectionString = $"mongodb://SKDev:{mongoDBPass}@{mongoDBHost}:{mongoDBPort}/?authSource=admin";
//            Console.WriteLine("Connection string formed...");

//            // create a new instance of the MongoDB Client server and call it client
//            MongoClient client = new MongoClient(connectionString);
//            Console.WriteLine("Formed Client connection to MongoDB Server");

//            // from the client server get the recipes database
//            var recipesDB = client.GetDatabase("recipes");
//            Console.WriteLine("Accessed Recipes Database...");

//            // from the recipes database, get the recipes collection and connect it to the mapping recipe entity class
//            var recipeCollection = recipesDB.GetCollection<Entities.Recipe>("recipes");
//            Console.WriteLine("Recipe Collection found...");

//            // create an empty list of NER objects to collect the named entity entries
//            // List<Entities.NER> nERList = new List<Entities.NER>();

//            // create an empty list of ingredient objects to collect the ingredients
//            List<Entities.Ingredient> ingredients = new List<Entities.Ingredient>();

//            // create an empty list of strings to collect the ingredient json string data
//            List<string> ingredientStrings = new List<string>();


//            //int recipeCount = recipeCollection.AsQueryable<Entities.Recipe>().Where(recipe => recipe.Source == "Gathered").Count<Entities.Recipe>();

//            // implement a filter definition builder for the recipe entity class 
//            var buiilder = Builders<Entities.Recipe>.Filter;

//            // use an empty filter so we can sort through all the documents
//            var filter = buiilder.Eq(recipe => recipe.Source, "Gathered");

//            // sort logic
//            SortDefinition<Entities.Recipe> sort = "{RecipeID: 1}";

//            // start i off as 0
//            int i = 0;

//            // start off recipe count as 0
//            int recipeCount = 0;

//            Console.WriteLine("Sorting through collection, gathering named entities, and forming a list of ingredients...");

//            // from the collection, find only documents that match the filter, sort them by ascending RecipeID, and limit the results to the first 50;
//            // asynchronously return a cursor
//            using (IAsyncCursor<Entities.Recipe> cursor = await recipeCollection.Find(filter).Sort(sort).Limit(50).ToCursorAsync())
//            {
//                while (cursor.Any() == true)
//                {
//                    List<Entities.Recipe> recipes = await cursor.ToListAsync();

//                    foreach (Entities.Recipe recipe in recipes)
//                    {
//                        // once we recieve the recipe, store the named entities of that recipe
//                        string namedEntities = recipe.NER;

//                        // from the string of named entities, create a UTF-8 encoded byte array using GetBytes();
//                        // from this, create a new memory stream
//                        var namedEntitesStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(namedEntities));

//                        // asynchronously deserialize the stream made from the string and put it in the form of an array of strings
//                        string[] namedEntitiesSet = await JsonSerializer.DeserializeAsync<string[]>(namedEntitesStream);

//                        // for each named entity in the array of named entities
//                        foreach (string namedEntity in namedEntitiesSet)
//                        {

//                            // initiate a new instance of the ingredient class
//                            Entities.Ingredient ingredient = new Entities.Ingredient();

//                            // assign the ingredient objectid to the objectid of the set
//                            ingredient.ObjectID = recipe.ObjectID.ToString();

//                            // assign the ingredient recipeid to the recipeid of the set
//                            ingredient.RecipeID = recipe.RecipeID;

//                            // assign the ingredient a named entity id, i
//                            ingredient.IngredientID = i;

//                            // assign the ingredient a named entity
//                            ingredient.NamedEntity = namedEntity;

//                            // add the ingredient object to the list of ingredients
//                            ingredients.Add(ingredient);

//                            // increase the value of i by 1
//                            i++;

//                        }

//                        recipeCount++;

//                    };

//                    await cursor.MoveNextAsync();

//                }
//            };



















//            // query the recipe collection for recipes where the source is "Gathered"
//            //Console.WriteLine("Creating query...");
//            //var recipeQuery = recipeCollection.AsQueryable<Entities.Recipe>()
//            //    .Where(recipe => recipe.Source == "Gathered")

//            //int recipeCount = recipeQuery.Count();   

//            // set the JSONSerliazerOptions first to the default for the web and then override it to include fields and indentation
//            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
//            options.IncludeFields = true;
//            options.WriteIndented = true;



//            //for (int recipeID = 0; recipeID < recipeCount; recipeID++)
//            //{
//            //    var recipe = recipeQuery.Where(recipe => recipe.RecipeID == recipeID).First();


//            //}

//            // for each recipe in the query, operate

//            //foreach (Entities.Recipe recipe in recipeQuery)
//            //{
//            //}

//            // take the list of ingredient objects and evaluate the NamedEntity property value for duplicates, remove them, and make a new list of the same type
//            Console.WriteLine("Ingredients gathered into list...cleaning data and transforming into JSON string");
//            var distinctIngredients = ingredients.DistinctBy(ingredient => ingredient.NamedEntity);

//            // for each ingredient in the distinct ingredient list

//            foreach (Entities.Ingredient ingredient in distinctIngredients)
//            {
//                // serialize the individual ingredient object into what it should be, a JSON string and store it as a string variable
//                string ingredientString = JsonSerializer.Serialize<Entities.Ingredient>(ingredient, options);

//                // add serialized ingredient string to the ingredient strings list
//                ingredientStrings.Add(ingredientString);
//            };

//            Console.WriteLine("Forming final JSON object string");

//            // take all the ingredient json objects in the ingredient json list and combine them into one json string containing all ingredients

//            string ingredientsJSON = String.Join(",", ingredientStrings);

//            Console.WriteLine(ingredientStrings);

//            return;

//        }


//    }

//    public class Entities
//    {
//        public class Recipe
//        {
//            [BsonId]
//            public ObjectId ObjectID { get; set; }

//            [BsonElement("recipeID")]
//            public int RecipeID { get; set; }

//            [BsonElement("title")]
//            public string Title { get; set; }

//            [BsonElement("ingredients")]
//            public string Ingredients { get; set; }

//            [BsonElement("directions")]
//            public string Directions { get; set; }

//            [BsonElement("link")]
//            public string Link { get; set; }

//            [BsonElement("source")]
//            public string Source { get; set; }

//            [BsonElement("NER")]
//            public string NER { get; set; }
//        }

//        public class Ingredient
//        {
//            [BsonId]
//            public string ObjectID { get; set; }

//            [BsonElement("RecipeID")]
//            public int RecipeID { get; set; }

//            [BsonElement("IngredientID")]
//            public int IngredientID { get; set; }

//            [BsonElement("NamedEntity")]
//            public string NamedEntity { get; set; }
//        }

//        public class NER
//        {
//            [BsonId]
//            public string ObjectID { get; set; }

//            [BsonElement("RecipeID")]
//            public int RecipeID { get; set; }

//            [BsonElement("NamedEntity")]
//            public string NamedEntity { get; set; }
//        }

//    }

//    public static class Helpers
//    {
//        // the method returns an IEnumberal of general type TSource
//        // and by naming it DistinctBy, an existing method, we make it an extension method
//        // the extension method's parameters are TSource and TKey
//        public static IEnumerable<TSource> DistinctBy<TSource, TKey>

//        // call the IEnumberal<TSource> source; this is the source input; for example List<nER>
//        // keyselector will take in any general source and return a general key, TKey
//        (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
//        {
//            // make a new instance of the Hashset<> class, hashing of general type TKey
//            HashSet<TKey> seenKeys = new HashSet<TKey>();
//            // for each element of general source type, TSource, in source
//            foreach (TSource element in source)
//            // because of the lambda expression x => x.property, every element in source is evaluated as element.property
//            {
//                // retun the value of element.property as a general value and add it to the hash set
//                if (seenKeys.Add(keySelector(element)))
//                {
//                    // if successful,  yield and return the element;
//                    // if not successful, move on
//                    // yield, iterate again, and wrap it all in an IEnumberable
//                    yield return element;
//                }
//            }
//        }
//    }

//}
