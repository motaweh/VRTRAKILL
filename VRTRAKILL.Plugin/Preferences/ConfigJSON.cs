using Newtonsoft.Json;
using System;
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

        // Reads the config file; writes defaults if it is missing, and returns defaults if it is missing, empty, invalid or unreadable.
        public static ConfigJSON Deserialize()
        {
            try
            {
                string Temp = File.ReadAllText(ConfigMaster.ConfigPath);
                ConfigJSON Config = JsonConvert.DeserializeObject<ConfigJSON>(Temp);
                // an empty file or "null" deserializes to null, and a section set to null stays null
                if (Config?.Config == null) throw new JsonSerializationException("The file is empty or has no \"VRTRAKILL Settings\" object.");
                Config.Config.FillMissingSections();
                return Config;
            }
            catch (FileNotFoundException)
            {
                Vars.Log.LogError("Unable to find VRTRAKILL_Config.json.\n" +
                                     "Generating a new one. Please quit the game and fill it out.\n" +
                                     "Starting up using default settings.");
                Serialize(new ConfigJSON()); return new ConfigJSON();
            }
            catch (Exception E) when (E is IOException || E is UnauthorizedAccessException)
            {
                Vars.Log.LogError("Unable to read VRTRAKILL_Config.json: " + E.Message + "\nStarting up using default settings.");
                return new ConfigJSON();
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
        // Writes the config to the config file as indented JSON through a temporary file; logs and keeps going if the folder is not writable.
        public static void Serialize(ConfigJSON Config)
        {
            string Temp = ConfigMaster.ConfigPath + ".tmp";
            try
            {
                File.WriteAllText(Temp, JsonConvert.SerializeObject(Config, Formatting.Indented));
                // a crash during the write leaves the old file whole
                if (File.Exists(ConfigMaster.ConfigPath)) File.Replace(Temp, ConfigMaster.ConfigPath, null);
                else File.Move(Temp, ConfigMaster.ConfigPath);
            }
            catch (Exception E)
            {
                Vars.Log.LogWarning("Unable to write VRTRAKILL_Config.json: " + E.Message);
                try { if (File.Exists(Temp)) File.Delete(Temp); } catch (Exception) { }
            }
        }
    }
}
