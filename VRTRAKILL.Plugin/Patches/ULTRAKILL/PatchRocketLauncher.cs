using HarmonyLib;
using Plugin.Systems;
using Plugin.Systems.VRAvatar;
using UnityEngine;

namespace Plugin.Patches.ULTRAKILL;

[HarmonyPatch(typeof(RocketLauncher))]
internal class PatchRocketLauncher
{
    // Places the rocket launcher at the avatar forearm and points it at the gun hand.
    [HarmonyPostfix]
    [HarmonyPatch(nameof(RocketLauncher.Update))]
    private static void Update_AddIK(RocketLauncher __instance)
    {
        if (VRigController.Instance != null)
        {
            __instance.transform.position = VRigController.Instance.Rig.FeedbackerB.Forearm.position;
            __instance.transform.LookAt(Vars.DominantHand.transform);
        }
    }

    static Vector3 oldPos;
    static Quaternion oldRot;
    // Moves the camera to the gun hand before a rocket launcher shot so the shot fires from the hand; __state says it moved.
    [HarmonyPrefix]
    [HarmonyPatch(nameof(RocketLauncher.Shoot))]
    [HarmonyPatch(nameof(RocketLauncher.ShootCannonball))]
    [HarmonyPatch(nameof(RocketLauncher.ShootNapalm))]
    private static void FixFiringDir(RocketLauncher __instance, out bool __state)
    {
        __state = false;
        CameraController cam = MonoSingleton<CameraController>.Instance;
        if (cam == null || Vars.DominantHand == null) return;
        __state = true;

        oldPos = cam.transform.position;
        oldRot = cam.transform.rotation;

        cam.transform.SetPositionAndRotation(Vars.DominantHand.transform.position, Vars.DominantHand.transform.rotation);
    }

    // Puts the camera back after a rocket launcher shot, only if the prefix moved it; else oldPos is from an earlier shot.
    [HarmonyPostfix]
    [HarmonyPatch(nameof(RocketLauncher.Shoot))]
    [HarmonyPatch(nameof(RocketLauncher.ShootCannonball))]
    [HarmonyPatch(nameof(RocketLauncher.ShootNapalm))]
    static void FixFiringDir2(bool __state)
    {
        CameraController cam = MonoSingleton<CameraController>.Instance;
        if (!__state || cam == null) return;

        cam.transform.SetPositionAndRotation(oldPos, oldRot);
    }
}
