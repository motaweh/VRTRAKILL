using HarmonyLib;
using Plugin.Systems;
using Plugin.Systems.Arms;
using Sandbox.Arm;
using System;

namespace Plugin.Patches.Visual;

// Hides the game's arm on the shotgun and adds the VR arm transformer to the revolver, sandbox arm and fishing rod.
[HarmonyPatch]
internal class PatchRemoveArms
{
    // Adds the VR arm transformer to the revolver arm.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Revolver), nameof(Revolver.Start))]
    static void RemoveRevolverArm(Revolver __instance)
    {
        try { __instance.gameObject.AddComponent<VRArmTransformer>(); }
        catch (NullReferenceException) { Vars.Log.LogWarning($"Revolver is null???"); }
    }
    // Adds the VR arm transformer to the sandbox arm.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SandboxArm), nameof(SandboxArm.Awake))]
    static void RemoveSandboxArm(SandboxArm __instance)
    {
        try { __instance.gameObject.AddComponent<VRArmTransformer>(); }
        catch (NullReferenceException) { Vars.Log.LogWarning($"Sandbox arm is null???"); }
    }
    // Hides the game's right arm on the shotgun.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Shotgun), nameof(Shotgun.Start))]
    static void RemoveShotgunArm(Shotgun __instance)
    {
        try
        {
            __instance.gameObject.transform.Find("Shotgun_New")
                .gameObject.transform.Find("RightArm")
                .gameObject.SetActive(false);
        }
        catch (NullReferenceException) { Vars.Log.LogWarning("Shotgun is null???"); }
    }
    // Adds the VR arm transformer to the fishing rod arm.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(FishingRodWeapon), nameof(FishingRodWeapon.Awake))]
    static void RemoveFRArm(FishingRodWeapon __instance)
    {
        try { __instance.gameObject.AddComponent<VRArmTransformer>(); }
        catch (NullReferenceException) { Vars.Log.LogWarning("Fishing rod is null???"); }
    }
}
