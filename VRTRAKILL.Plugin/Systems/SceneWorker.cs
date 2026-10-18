using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Reflection;
using VRTRAKILL.Utilities;

namespace Plugin.Systems
{
    // per-scene setup
    internal static class SceneWorker
    {
        private static MonoBehaviour Host;
        private static Coroutine LateCanvases;

        // Runs the scene setup each time the active scene changes; the host runs the late canvas conversion.
        public static void Init(MonoBehaviour host)
        {
            Host = host;
            SceneManager.activeSceneChanged += (_, _) => SceneChanged();
        }

        // Reloads the config and assets, moves the assets to the AlwaysOnTop layer, converts the canvases and starts the late conversion.
        private static void SceneChanged()
        {
            VRTRAKILL.Utilities.Unity.EnableOffscreenRendering();

            // reload config
            Prefs.ConfigJSON.Instance = null;
            Prefs.ConfigMaster.Init();

            Assets.LoadAllCustomAssets();
            RelayerAssetsStuff();

            bool HasCamera = Vars.MainCamera != null;
            if (HasCamera) UI.UIConverter.ConvertAllCanvases();

            if (Host == null) return;
            if (LateCanvases != null) Host.StopCoroutine(LateCanvases);
            LateCanvases = Host.StartCoroutine(ConvertLateCanvases(!HasCamera));
        }
        // Waits up to 180 frames for the main camera if there was none, then converts canvases that appear in the next 180 frames.
        private static IEnumerator ConvertLateCanvases(bool WaitForCamera)
        {
            // based on Guillaume-Lechevallier's VRTRAKILL_FRAUD: HUD and menu canvases created after the scene change stayed screen space
            yield return null;
            if (WaitForCamera)
            {
                for (int i = 0; Vars.MainCamera == null && i < 180; i++) yield return null;
                if (Vars.MainCamera == null)
                {
                    Vars.Log.LogWarning("No main camera in this scene; canvases are not converted.");
                    LateCanvases = null;
                    yield break;
                }
                UI.UIConverter.ConvertAllCanvases();
            }

            for (int i = 0; i < 180; i++)
            {
                yield return null;
                if (i < 12 || i % 10 == 0) UI.UIConverter.RecursiveConvertCanvas();
            }
            LateCanvases = null;
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
