using System.Text.Json;

namespace IpInformation
{
    public class AppSettings
    {
        public required string ApplicationUrl { get; set; }
        public required ConnectionStrings ConnectionStrings { get; set; }
        public required VariableSettings VariableSettings { get; set; }
    }


    public class VariableSettings
    {
        public required bool SaveToDatabase { get; set; }
    }

    public class ConnectionStrings
    {
        public required string DefaultConnection { get; set; }
        public required string WorkConnectionString { get; set; }
    }

    public class Constants
    {
        public static readonly string GoogleDNS = "8.8.8.8";
        public static readonly TimeSpan DatabaseUpdateIntervalTime = TimeSpan.FromMinutes(60);
        public static readonly string DefaultCookie = "TempCookieName";
    }

    public static class SettingsLoader
    {
        public static T LoadSettings<T>(string filePath)
        {
            var json = System.IO.File.ReadAllText(filePath) ?? throw new Exception($"Error trying to load the {filePath}");
            var settings = JsonSerializer.Deserialize<T>(json);

            if (settings != null)
            {
                return settings;
            }
            else throw new Exception("Failed to Load filePath Settings");
        }
    }
}
