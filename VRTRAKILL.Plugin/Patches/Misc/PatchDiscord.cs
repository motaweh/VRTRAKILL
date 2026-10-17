using HarmonyLib;

namespace Plugin.Patches.Misc;

[HarmonyPatch]
internal sealed class PatchDiscord
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(DiscordController), nameof(DiscordController.SendActivity))]
    static void SendActivity(DiscordController __instance)
    => __instance.cachedActivity.State = "Playing in VR via VRTRAKILL";
}
