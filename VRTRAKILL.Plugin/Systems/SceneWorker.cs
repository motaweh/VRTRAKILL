using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;
using VRTRAKILL.Utilities;

namespace Plugin.Systems
{
    // per-scene setup
    internal static class SceneWorker
    {
        public static void Init()
        => SceneManager.activeSceneChanged += (_, _) => SceneChanged();

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
