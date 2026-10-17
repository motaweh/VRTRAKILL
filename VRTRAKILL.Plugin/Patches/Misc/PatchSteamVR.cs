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
        // _head stays unset: OnEnable would treat this camera as the old head and deactivate it
        __instance.gameObject.AddComponent<SteamVR_Ears>();
        return false;
    }
}
