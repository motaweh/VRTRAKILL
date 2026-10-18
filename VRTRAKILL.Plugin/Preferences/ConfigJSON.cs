using Newtonsoft.Json;
using System.IO;
using Plugin.Systems;

namespace Plugin.Prefs
{
    public class ConfigJSON
    {
        [JsonProperty("VRTRAKILL Settings")] public NewConfig Config { get; set; }

        public static ConfigJSON Instance { get; set; }

        // Creates a config with default settings.
        public ConfigJSON()
        {
            Config = new NewConfig();
        }

        // Returns the loaded config, reading the file on first use.
        public static ConfigJSON GetConfig()
        {
            if (Instance == null) Instance = Deserialize();
            return Instance;
        }

        // Reads the config file; writes defaults if it is missing, and returns defaults if it is missing or invalid.
        public static ConfigJSON Deserialize()
        {
            try
            {
                string Temp = File.ReadAllText(ConfigMaster.ConfigPath);
                ConfigJSON Config = JsonConvert.DeserializeObject<ConfigJSON>(Temp);
                return Config;
            }
            catch (FileNotFoundException)
            {
                Vars.Log.LogError("Unable to find VRTRAKILL_Config.json.\n" +
                                     "Generating a new one. Please quit the game and fill it out.\n" +
                                     "Starting up using default settings.");
                Serialize(new ConfigJSON()); return new ConfigJSON();
            }
            catch (JsonException)
            {
                Vars.Log.LogError("Something went wrong when trying to read VRTRAKILL_Config.json\n" +
                                     "Please fix any typos, formatting errors, etc.\n" +
                                     "Or delete the config and let it generate once more.\n" +
                                     "Starting up using default settings.");
                return new ConfigJSON();
            }
        }
        // Writes the config to the config file as indented JSON.
        public static void Serialize(ConfigJSON Config)
        {
            string JSON = JsonConvert.SerializeObject(Config, Formatting.Indented);
            File.WriteAllText(ConfigMaster.ConfigPath, JSON);
        }
    }
}
