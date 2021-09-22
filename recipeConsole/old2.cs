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
//            List<Entities.Ingredient> ingredientList = new List<Entities.Ingredient>();

//            // create an empty list of strings to collect the ingredient json string data
//            List<string> ingredientJSONList = new List<string>();

//            // query the recipe collection for recipes where the source is "Gathered"
//            Console.WriteLine("Querying Recipe collection...");
//            var recipeQuery = recipeCollection.AsQueryable<Entities.Recipe>()
//                .Where(recipe => recipe.Source == "Gathered");


//            // set the JSONSerliazerOptions first to the default for the web and then override it to include fields and indentation
//            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
//            options.IncludeFields = true;
//            options.WriteIndented = true;

//            // start i off as 0
//            int i = 0;

//            // for each recipe in the query, operate
//            Console.WriteLine("Sorting through query results, gathering named entities, and forming a list of ingredients...");
//            foreach (Entities.Recipe recipe in recipeQuery)
//            {
//                // once we recieve the recipe, store the named entities of that recipe
//                string namedEntities = recipe.NER;

//                // from the string of named entities, create a UTF-8 encoded byte array using GetBytes();
//                // from this, create a new memory stream
//                var namedEntitesStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(namedEntities));

//                // asynchronously deserialize the stream made from the string and put it in the form of an array of strings
//                string[] namedEntitiesSet = await JsonSerializer.DeserializeAsync<string[]>(namedEntitesStream);

//                // for each named entity in the array of named entities
//                foreach (string namedEntity in namedEntitiesSet)
//                {

//                    // initiate a new instance of the NER class
//                    // Entities.NER nER = new Entities.NER();

//                    // assign the nER objectid to the objectid of the set
//                    // nER.ObjectID = recipe.ObjectID.ToString();

//                    //// assign the nER recipeid to the recipeid of the set
//                    // nER.RecipeID = recipe.RecipeID;

//                    //// assign the nER a named entity
//                    // nER.NamedEntity = namedEntity;

//                    //// add the nER object to the nERList
//                    // nERList.Add(nER);

//                    // initiate a new instance of the ingredient class
//                    Entities.Ingredient ingredient = new Entities.Ingredient();

//                    // assign the ingredient objectid to the objectid of the set
//                    ingredient.ObjectID = recipe.ObjectID.ToString();

//                    // assign the ingredient recipeid to the recipeid of the set
//                    ingredient.RecipeID = recipe.RecipeID;

//                    // assign the ingredient a named entity id, i
//                    ingredient.IngredientID = i;

//                    // assign the ingredient a named entity
//                    ingredient.NamedEntity = namedEntity;



//                    // add the ingredient string to the list of ingredients
//                    ingredientList.Add(ingredient);

//                    // increase the value of i by 1
//                    i++;

//                }

//            }

//            // take the list of ingredient objects and evaluate the NamedEntity property value for duplicates, remove them, and make a new list of the same type
//            Console.WriteLine("Ingredients gathered into list...cleaning data and transforming into JSON string");
//            List<Entities.Ingredient> distinctIngredients = (List<Entities.Ingredient>)ingredientList.DistinctBy(ingredient => ingredient.NamedEntity);

//            // for each ingredient in the distinct ingredient list
//            distinctIngredients.ForEach(ingredient =>
//            {
//                // serialize the individual ingredient object into what it should be, a JSON string and store it as a string variable
//                string ingredientJSON = JsonSerializer.Serialize<Entities.Ingredient>(ingredient, options);

//                // add ingredientJSON string to the ingredient json list
//                ingredientJSONList.Add(ingredientJSON);

//            });


//            Console.WriteLine("Forming final JSON object string");

//            // take all the ingredient json objects in the ingredient json list and combine them into one json string containing all ingredients

//            string ingredientsJSON = String.Join(",", ingredientJSONList);

//            // take the list of named entity objects; namedEntity is evaluated as namedEntity.NamedEntity
//            // var distinctNamedEntities = nERList.DistinctBy(namedEntity => namedEntity.NamedEntity);

//            // convert the list to an array of strings so that we can later iterate through it
//            // Entities.NER[] distinctNamedEntitiesArray = distinctNamedEntities.ToArray();

//            // make a new list of strings which will be a list of the unique ingredients
//            // List<string> ingredientsList = new List<string>();

//            // start counting at i = 0, the beginning array index;
//            // for each i, as long as i is less than the length of the distinct named entities array:
//            // perform an operation and then increase the value of i at the end
//            //for (int i = 0; i < distinctNamedEntitiesArray.Length; i++)
//            //{
//            //    // first, initialize a new instance of the Entities.Ingredients class object; call it ingredient
//            //    Entities.Ingredient ingredient = new Entities.Ingredient();

//            //    // assign to namedObject an ObjectID
//            //    ingredient.ObjectID = distinctNamedEntitiesArray.ElementAt(i).ObjectID.ToString();

//            //    // assign to namedObject a RecipeID
//            //    ingredient.RecipeID = distinctNamedEntitiesArray.ElementAt(i).RecipeID;

//            //    // assign to namedObject an NEID equal to the value of i
//            //    ingredient.NEID = i;

//            //    // get the value of the element at index i of the distinct named entities array and capture it to the NamedEntity property of namedObject
//            //    ingredient.NamedEntity = distinctNamedEntitiesArray.ElementAt(i).NamedEntity;

//            //}

//            //string ingredients = String.Join(",", ingredientsList);
//            //Console.WriteLine(ingredients);
//            //Console.WriteLine("Breakpoint");

//            //string ingredientString = String.Join(",", ingredientsList);

//            // take the list of distinct named entities and convert it to a string of namedEntities
//            //string namedEntities = String.Join(",", distinctNamedEntities);

//            //string ingredientsJSON = "{{\"ingredients\"";

//            // Check if they are really duplicates
//            //var duplicateNamedEntities = namedEntityList.GroupBy(x => x)
//            //    .Where(g => g.Count() > 1)
//            //    .Select(y => new { Element = y.Key, Counter = y.Count() })
//            //    .ToList();

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
//                    // yield and the above method type declaration will make it so it's an IEnumberable<elemen>
//                    yield return element;
//                }
//            }
//        }
//    }

//}
