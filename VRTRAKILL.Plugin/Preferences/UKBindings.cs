using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Plugin.Prefs
{
    /// <summary>
    /// Represents the Binds.json file located in ULTRAKILL\\Preferences\\Binds.json
    /// </summary>
    public class UKBindings
    {
        /// <summary>
        /// Represents the modifiedActions object in Binds.json
        /// </summary>
        public class ModifiedActions
        {
            [JsonProperty("Dodge")] public ModifiedAction[] Dodge { get; set; }
            [JsonProperty("Slide")] public ModifiedAction[] Slide { get; set; }
            [JsonProperty("Jump")] public ModifiedAction[] Jump { get; set; }
            [JsonProperty("Change Fist")] public ModifiedAction[] ChangeFist { get; set; }
            [JsonProperty("Punch")] public ModifiedAction[] Punch { get; set; }
            [JsonProperty("Hook")] public ModifiedAction[] Hook { get; set; }
            [JsonProperty("Primary Fire")] public ModifiedAction[] PrimaryFire { get; set; }
            [JsonProperty("Secondary Fire")] public ModifiedAction[] SecondaryFire { get; set; }
            [JsonProperty("Next Variation")] public ModifiedAction[] ChangeVariation { get; set; }
            [JsonProperty("Previous Variation")] public ModifiedAction[] PrevVariation { get; set; }
            [JsonProperty("Slot 0")] public ModifiedAction[] Slot0 { get; set; }
            [JsonProperty("Revolver")] public ModifiedAction[] Slot1 { get; set; }
            [JsonProperty("Shotgun")] public ModifiedAction[] Slot2 { get; set; }
            [JsonProperty("Nailgun")] public ModifiedAction[] Slot3 { get; set; }
            [JsonProperty("Railcannon")] public ModifiedAction[] Slot4 { get; set; }
            [JsonProperty("Rocket Launcher")] public ModifiedAction[] Slot5 { get; set; }
            [JsonProperty("Spawner Arm")] public ModifiedAction[] Slot6 { get; set; }
            [JsonProperty("Slot 7")] public ModifiedAction[] Slot7 { get; set; }
            [JsonProperty("Slot 8")] public ModifiedAction[] Slot8 { get; set; }
            [JsonProperty("Slot 9")] public ModifiedAction[] Slot9 { get; set; }
            [JsonProperty("Next Weapon")] public ModifiedAction[] NextWeapon { get; set; }
            [JsonProperty("Previous Weapon")] public ModifiedAction[] PrevWeapon { get; set; }
            [JsonProperty("Last Used Weapon")] public ModifiedAction[] LastWeapon { get; set; }

            /// <summary>
            /// Creates a new <see cref="ModifiedActions"/> class with the default game parameters.
            /// </summary>
            public ModifiedActions()
            {
                Dodge           = new ModifiedAction[] { new ModifiedAction("LeftShift") };
                Slide           = new ModifiedAction[] { new ModifiedAction("LeftControl") };
                Jump            = new ModifiedAction[] { new ModifiedAction("Space") };
                ChangeFist      = new ModifiedAction[] { new ModifiedAction("G") };
                Punch           = new ModifiedAction[] { new ModifiedAction("F") };
                Hook            = new ModifiedAction[] { new ModifiedAction("R") };
                PrimaryFire     = new ModifiedAction[] { new ModifiedAction("LMB") };
                SecondaryFire   = new ModifiedAction[] { new ModifiedAction("RMB") };
                ChangeVariation = new ModifiedAction[] { new ModifiedAction("E") };
                PrevVariation   = new ModifiedAction[] { new ModifiedAction("Q") };
                Slot0           = new ModifiedAction[] { new ModifiedAction("0") };
                Slot1           = new ModifiedAction[] { new ModifiedAction("1") };
                Slot2           = new ModifiedAction[] { new ModifiedAction("2") };
                Slot3           = new ModifiedAction[] { new ModifiedAction("3") };
                Slot4           = new ModifiedAction[] { new ModifiedAction("4") };
                Slot5           = new ModifiedAction[] { new ModifiedAction("5") };
                Slot6           = new ModifiedAction[] { new ModifiedAction("6") };
                Slot7           = new ModifiedAction[] { new ModifiedAction("7") };
                Slot8           = new ModifiedAction[] { new ModifiedAction("8") };
                Slot9           = new ModifiedAction[] { new ModifiedAction("9") };
                NextWeapon      = new ModifiedAction[] { new ModifiedAction("") };
                PrevWeapon      = new ModifiedAction[] { new ModifiedAction("") };
                LastWeapon      = new ModifiedAction[] { new ModifiedAction("") }; // the game has no keyboard default for it
            }
        }
        /// <summary>
        /// Represents a modified action object of Binds.json.
        /// If it's an array, use only the zeroth index.
        /// </summary>
        public class ModifiedAction
        {
            [JsonProperty("path")] public string LocalPath { get; set; }
            [JsonIgnore] public string Path => GetPath();
            public string GetPath()
            {
                if (LocalPath.Contains("/"))
                {
                    string[] Temp = LocalPath.Split('/');
                    return Temp[Temp.Length - 1];
                }
                return LocalPath;
            }
            public ModifiedAction(string LocalPath)
            {
                this.LocalPath = LocalPath;
            }
        }
        static readonly Dictionary<string, string> LegacyNames = new Dictionary<string, string>
        {
            { "Slot 1", "Revolver" }, { "Slot 2", "Shotgun" }, { "Slot 3", "Nailgun" }, { "Slot 4", "Railcannon" },
            { "Slot 5", "Rocket Launcher" }, { "Change Variation", "Next Variation" }, { "Last Weapon", "Last Used Weapon" }
        };

        [JsonProperty("controlScheme")] public string ControlScheme { get; set; } = "Keyboard & Mouse";
        [JsonProperty("modifiedActions")] public ModifiedActions Actions { get; set; }

        public static ModifiedActions GetBinds()
        {
            var Binds = new ModifiedActions();

            // the game only writes Binds.json after the plugin loads on a fresh install, so fall back to defaults
            string BindsPath = $"{PluginInfo.GamePath}\\Preferences\\Binds.json";
            if (!File.Exists(BindsPath)) return Binds;

            UKBindings Json;
            try
            {
                // older Binds.json files use the pre-rename names; the game maps them the same way (JsonBindingMap.bindAliases)
                JObject Root = JObject.Parse(File.ReadAllText(BindsPath));
                if (Root["modifiedActions"] is JObject Modified)
                    foreach (KeyValuePair<string, string> Alias in LegacyNames)
                        if (Modified[Alias.Key] != null && Modified[Alias.Value] == null)
                        { Modified[Alias.Value] = Modified[Alias.Key]; Modified.Remove(Alias.Key); }
                Json = Root.ToObject<UKBindings>();
            }
            catch (System.Exception E) when (E is IOException || E is JsonException)
            {
                Systems.Vars.Log.LogWarning("Unable to read Binds.json, using default keys: " + E.Message);
                return Binds;
            }

            if (Json == null || Json.Actions == null) return Binds;

            // What this does is basically scroll thru all the properties in the Binds object
            // For each property in Binds scroll through all the properties in the Json.Actions obejct
            // If it has the matching names (as stated in the JsonProperty) and different values,
            // swap the default value with the .json one.
            PropertyInfo[] PropInfo = Binds.GetType().GetProperties();
            for (int i = 0; i < PropInfo.Length; i++)
            {
                foreach (PropertyInfo JsonPropInfo in Json.Actions.GetType().GetProperties())
                    if (PropInfo[i].GetCustomAttribute<JsonPropertyAttribute>().PropertyName == JsonPropInfo.GetCustomAttribute<JsonPropertyAttribute>().PropertyName
                    && PropInfo[i].GetValue(Binds) != JsonPropInfo.GetValue(Json.Actions)
                    && JsonPropInfo.GetValue(Json.Actions) is ModifiedAction[] Saved && Saved.Length > 0) // an unbound action saves as []
                        PropInfo[i].SetValue(Binds, JsonPropInfo.GetValue(Json.Actions));
            }
            Json.Actions = Binds;
            return Json.Actions;
        }
    }
}
