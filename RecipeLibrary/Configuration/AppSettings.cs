using System.Text.Json.Serialization;

namespace RecipeLibrary.Configuration
{
    public class AppSettings
    {
        public MongoDBSettings MongoDBInfo { get; set; }

        public class MongoDBSettings
        {
            [JsonPropertyName("host")]
            public string Host { get; set; }

            [JsonPropertyName("port")]
            public int Port { get; set; }

            [JsonPropertyName("mongodbpass")]
            public string MongoDBPass { get; set; }
        }
    }
}