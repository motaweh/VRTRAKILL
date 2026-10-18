using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;
using VRTRAKILL.Utilities;

namespace Plugin.Systems
{
    // per-scene setup
    internal static class SceneWorker
    {
        // Runs the scene setup each time the active scene changes.
        public static void Init()
        => SceneManager.activeSceneChanged += (_, _) => SceneChanged();

        // Reloads the config and assets, moves the assets to the AlwaysOnTop layer and converts the canvases.
        private static void SceneChanged()
        {
            VRTRAKILL.Utilities.Unity.EnableOffscreenRendering();

            // reload config
            Prefs.ConfigJSON.Instance = null;
            Prefs.ConfigMaster.Init();

            Assets.LoadAllCustomAssets();
            RelayerAssetsStuff();

            UI.UIConverter.ConvertAllCanvases();
        }
        // Moves every loaded mod asset and its children to the AlwaysOnTop layer.
        private static void RelayerAssetsStuff()
        {
            // the Assets properties hold references to the instantiated objects, so this relayers them in place
            foreach (PropertyInfo I in typeof(Assets).GetProperties())
            {
                if (I.GetValue(typeof(Assets), null) != null)
                {
                    GameObject GO = (GameObject)I.GetValue(typeof(Assets), null);
                    GO.RecursiveChangeLayer((int)Layers.AlwaysOnTop);
                }
                else continue;
            }
        }
    }
}
