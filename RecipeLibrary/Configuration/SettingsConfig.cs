
using System.Text.Json;

namespace RecipeLibrary.Configuration
{
    public class SettingsConfig
    {
        // get and set the logging object
        // it will be based on the Fields.Logging class
        public LogSettings Logging { get; set; }

        // get and set the app settings object
        // it wil be based on the Fields.Appsettings class
        public AppSettings AppSettings { get; set; }

        // override the ToString method for this class
        // it will take in the string and serialize it with indentation
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
    }
}