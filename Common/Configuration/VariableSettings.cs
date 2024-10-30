using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Common.Configuration
{
    public class Settings
    {
        public bool SaveToDatabase { get; set; }
    }

    public static class SettingsLoader
    {
        public static Settings LoadSettings(string filePath)
        {
            var json = System.IO.File.ReadAllText(filePath);

            if (json == null)
            {
                throw new Exception("Failed to Load Application Settings");
            }

            return JsonSerializer.Deserialize<Settings>(json);
        }
    }
}
