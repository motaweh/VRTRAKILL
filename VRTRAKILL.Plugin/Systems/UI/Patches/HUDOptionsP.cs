using HarmonyLib;
using Plugin.Systems.VRCamera.Patches;
using ULTRAKILL.Portal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR;
using VRTRAKILL.Utilities;

namespace Plugin.Systems.UI.Patches
{
    [HarmonyPatch] internal class HUDOptionsP
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CanvasController), nameof(CanvasController.Awake))]
        static void ResizeCanvases(CanvasController __instance)
        {
            // Stretches screen effects so it's not a small square in the middle of the hud
            string[] ScreenEffects =
            {
                "HurtScreen", "BlackScreen", "ParryFlash",
                "UnderwaterOverlay", "Black", "White"
            };
            foreach (string ScreenEffect in ScreenEffects)
                try
                {
                    Transform T = __instance.gameObject.transform.Find(ScreenEffect);
                    T.transform.localScale *= 10;
                    for (int i = 0; i < T.childCount; i++)
                        T.GetChild(i).transform.localScale /= 10;
                }
                catch { continue; }

            // prime bosses specific
            try { Object.FindObjectOfType<FlashImage>().transform.localScale *= 10; } catch { }

            // hide the power-up vignette
            string[] ScreenEffectsToDisable =
            {
                "PowerUpVignette",
            };
            foreach (string ScreenEffectToDisable in ScreenEffectsToDisable)
                try { __instance.gameObject.transform.Find(ScreenEffectToDisable).GetComponent<Image>().enabled = false; } catch { continue; }

            // Relayer skybox in 2-4
            try { GameObject.Find("CityFromAbove").layer = 0; } catch { }

            UIConverter.RecursiveConvertCanvas(__instance.gameObject);
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CanvasController), nameof(CanvasController.OnEnable))]
        static void DeployGTFOTW(CanvasController __instance)
        {
            // OnEnable runs again on every re-enable, don't stack copies
            if (__instance.transform.Find("VRTRAKILL_GetOutOfTheWall") != null)
                return;

            if (Assets.UI_GTFOTW == null) Assets.LoadAllCustomAssets();
            GameObject UI_GTFOTW = Object.Instantiate(Assets.UI_GTFOTW, Vector3.zero, Quaternion.identity, __instance.transform);
            UI_GTFOTW.name = "VRTRAKILL_GetOutOfTheWall";
            UI_GTFOTW.transform.localScale = Vector3.zero;

            UI_GTFOTW.transform.SetSiblingIndex(0);
            UI_GTFOTW.transform.localScale = Vector3.one;
            UI_GTFOTW.transform.localPosition = Vector3.zero;

            UIConverter.ConvertCanvas(UI_GTFOTW.GetComponent<Canvas>());
            VRTRAKILL.Utilities.Unity.RecursiveChangeLayer(UI_GTFOTW, (int)Layers.UI);

            GTFOTW GTFOTW = UI_GTFOTW.AddComponent<GTFOTW>();
            GTFOTW.DetectorTransform = Vars.MainCamera.transform;
        }

        [HarmonyPostfix] [HarmonyPatch(typeof(ScreenZone), nameof(ScreenZone.OnTriggerEnter))] static void ConvertThing()
        {
            UIConverter.RecursiveConvertCanvas();
        }

        [HarmonyPostfix] [HarmonyPatch(typeof(FlashImage), nameof(FlashImage.Flash))] static void FlashImageTweak(FlashImage __instance)
        {
            if (__instance.gameObject.name.Contains("White") || __instance.gameObject.name.Contains("Black"))
                __instance.transform.localScale *= 20;
        }
    }
}
