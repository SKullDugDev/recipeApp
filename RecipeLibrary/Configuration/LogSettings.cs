using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace RecipeLibrary.Configuration
{
    public class LogSettings
    {
        public bool IncludeScopes { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LogLevel Default { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LogLevel System { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LogLevel Microsoft { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LogLevel HttpClient { get; set; }
    }

}