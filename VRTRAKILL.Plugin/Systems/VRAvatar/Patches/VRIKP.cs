using HarmonyLib;
using VRTRAKILL.Utilities;

namespace Plugin.Systems.VRAvatar.Patches
{
    [HarmonyPatch]
    internal class VRIKP
    {
        // Adds the VR avatar rig controller to the player when it spawns.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(NewMovement), nameof(NewMovement.Start))]
        static void AttachVRIK(NewMovement __instance)
        {
            if (!__instance.gameObject.HasComponent<VRigController>())
            {
                __instance.gameObject.AddComponent<VRigController>();
                VRigController.Instance.Rig = MetaRig.CreateVCustomPreset(null);
                VRigController.Instance.Rig.Root.localPosition = new UnityEngine.Vector3(0, 0, 0);
            }
        }
    }
}
