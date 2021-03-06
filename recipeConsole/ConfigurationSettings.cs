//using Microsoft.Extensions.Logging;
//using System.Text.Json;
//using System.Text.Json.Serialization;

//namespace recipeConsole
//{
//    public class ConfigurationSettings
//    {
//        public SettingsFields.Logging Logging { get; set; }

//        public SettingsFields.AppSettings AppSettings { get; set; }

//        public override string ToString()
//        {
//            return JsonSerializer.Serialize(this, new JsonSerializerOptions
//            {
//                WriteIndented = true
//            });
//        }
//    }

//    public class SettingsFields
//    {
//        public class Logging
//        {
//            public ConsoleSettings Console { get; set; }

//            public bool IncludeScopes { get; set; }

//            public class ConsoleSettings
//            {
//                public LogLevelSettings LogLevel { get; set; }

//                public class LogLevelSettings
//                {
//                    [JsonConverter(typeof(JsonStringEnumConverter))]
//                    public LogLevel Default { get; set; }

//                    [JsonConverter(typeof(JsonStringEnumConverter))]
//                    public LogLevel System { get; set; }

//                    [JsonConverter(typeof(JsonStringEnumConverter))]
//                    public LogLevel Microsoft { get; set; }
//                }
//            }
//        }

//        public class AppSettings
//        {
//            public MongoDBSettings MongoDBInfo { get; set; }

//            public class MongoDBSettings
//            {
//                public string host { get; set; }
//                public int port { get; set; }
//                public string mongodbpass { get; set; }
//            }

//        }
//    }
//}
