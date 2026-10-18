using HarmonyLib;
using Plugin.Systems.VRAvatar;

namespace Plugin.Patches.ULTRAKILL;

[HarmonyPatch(typeof(ShotgunHammer))]
internal class PatchShotgunHammer
{
    // Places the jackhammer at the avatar forearm and points it at the avatar hand.
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ShotgunHammer.LateUpdate))]
    static void LateUpdate_AddIK(ShotgunHammer __instance)
    {
        if (VRigController.Instance != null)
        {
            __instance.transform.position = VRigController.Instance.Rig.FeedbackerB.Forearm.position;
            __instance.transform.LookAt(VRigController.Instance.Rig.FeedbackerB.Hand.Root.position);
        }
    }
}
