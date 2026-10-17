using HarmonyLib;
using UnityEngine;
using Valve.VR;

namespace Plugin.Patches.Misc;

[HarmonyPatch]
internal static class PatchSteamVR
{
    // skip Expand: its renaming and re-parenting breaks the game's camera hierarchy
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SteamVR_Camera), nameof(SteamVR_Camera.Expand))]
    static bool SVRCExpand(SteamVR_Camera __instance, Transform ____ears, Transform ____head)
    {
        __instance.gameObject.AddComponent<SteamVR_Ears>();
        ____ears = __instance.transform;
        ____head = __instance.transform;

        return false;
    }
}
