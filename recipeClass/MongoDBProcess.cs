using System;
using System.IO;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;


namespace recipeClass
{
    public class MongoDBProcess
    {
        public static async Task Connect(string mongoDBHost, int mongoDBPort, string mongoDBPass)
        {
            // form connection string for communicating with mongoDB
            string connectionString = $"mongodb://SKDev:{mongoDBPass}@{mongoDBHost}:{mongoDBPort}/?authSource=admin";
            Console.WriteLine("Connection string formed...");

            // create a new instance of the MongoDB Client server and call it client
            MongoClient client = new MongoClient(connectionString);
            Console.WriteLine("Formed Client connection to MongoDB Server");

            // from the client server get the recipes database
            var recipesDB = client.GetDatabase("recipes");
            Console.WriteLine("Accessed Recipes Database...");

            // from the recipes database, get the recipes collection and connect it to the mapping recipe entity class
            var recipeCollection = recipesDB.GetCollection<Entities.Recipe>("recipes");
            Console.WriteLine("Recipe Collection found...");

            // implement a filter definition builder for the recipe entity class 
            var buiilder = Builders<Entities.Recipe>.Filter;

            // use an empty filter so we can sort through all the documents
            var filter = buiilder.Empty;

            // create an empty list of strings to collect the named entity entries
            List<string> namedEntitiesList = new List<string>();

            // wait as, from the collection of recipes, we find every recipe and, for each recipe, we recieve it asynchronously
            Console.WriteLine("Processing documents for Named Entities...");
            await recipeCollection.Find(filter).ForEachAsync<Entities.Recipe>(async (recipe) =>

            {
                // once we recieve the recipe, store the named entities of that recipe
                string namedEntities = recipe.NER;

                // from the string of named entities, create a UTF-8 encoded byte array using GetBytes(); from this, create a new memory stream
                var namedEntitesStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(namedEntities));

                // asynchronously deserialize the stream made from the string and put it in the form of an array of strings
                string[] namedEntitiesArray = await JsonSerializer.DeserializeAsync<string[]>(namedEntitesStream);

                // for each named entity in the array of named entities
                foreach (string namedEntity in namedEntitiesArray)
                {
                    // add the named entity to the 
                    namedEntitiesList.Add(namedEntity);

                }

            });
            Console.WriteLine("Named Entities gathered...cleaning data...");

            // take the list of named entities and pull out the distinct entities into a list
            List<string> distinctNamedEntitiesList = namedEntitiesList.Distinct().ToList();

            // convert the list to an array of strings so that we can later iterate through it
            string[] distinctNamedEntitiesArray = distinctNamedEntitiesList.ToArray();

            // make a new list of strings which will be a list of the unique ingredients
            List<string> ingredientsList = new List<string>();

            // start counting at i = 0, the beginning array index;
            // for each i, as long as i is less than the length of the distinct named entities array:
            // perform an operation and then increase the value of i at the end
            for (int i = 0; i < distinctNamedEntitiesArray.Length; i++)
            {
                // first, initialize a new instance of the Entities.Ingredients class object; call it namedObject
                Entities.Ingredients namedObject = new Entities.Ingredients();

                // assign to namedObject an NEID equal to the value of i
                namedObject.NEID = i;

                // get the value of the element at index i of the distinct named entities array and capture it to the NamedEntity property of namedObject
                namedObject.NamedEntity = (string)distinctNamedEntitiesArray.GetValue(i);

                // set the JSONSerliazerOptions first to the default for the web and then override it to include fields and indentation
                var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
                options.IncludeFields = true;
                options.WriteIndented = true;

                // serialize the namedObject into what it should be, a JSON string and store it as a string variable
                string ingredientJSONString = JsonSerializer.Serialize<Entities.Ingredients>(namedObject, options);

                // add the object string to the list of ingredients
                ingredientsList.Add(ingredientJSONString);
            }

            string ingredients = String.Join(",", ingredientsList);
            Console.WriteLine(ingredients);
            Console.WriteLine("Breakpoint");

            //string ingredientString = String.Join(",", ingredientsList);

            // take the list of distinct named entities and convert it to a string of namedEntities
            //string namedEntities = String.Join(",", distinctNamedEntities);

            //string ingredientsJSON = "{{\"ingredients\"";

            // Check if they are really duplicates
            //var duplicateNamedEntities = namedEntityList.GroupBy(x => x)
            //    .Where(g => g.Count() > 1)
            //    .Select(y => new { Element = y.Key, Counter = y.Count() })
            //    .ToList();

            return;


        }


    }

    public class Entities
    {
        public class Recipe
        {
            [BsonId]
            public MongoDB.Bson.ObjectId ObjectID { get; set; }

            [BsonElement("recipeID")]
            public int RecipeID { get; set; }

            [BsonElement("title")]
            public string Title { get; set; }

            [BsonElement("ingredients")]
            public string Ingredients { get; set; }

            [BsonElement("directions")]
            public string Directions { get; set; }

            [BsonElement("link")]
            public string Link { get; set; }

            [BsonElement("source")]
            public string Source { get; set; }

            [BsonElement("NER")]
            public string NER { get; set; }
        }

        public class Ingredients
        {
            [BsonElement("NEID")]
            public int NEID { get; set; }

            [BsonElement("NamedEntity")]
            public string NamedEntity { get; set; }
        }


    }



}
