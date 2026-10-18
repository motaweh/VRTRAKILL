using HarmonyLib;
using Plugin.Systems.Controllers;
using System.Collections;
using UnityEngine;
using VRTRAKILL.Utilities;

namespace Plugin.Systems.VRAvatar.Patches
{
    // patched only when "Enable VRAvatar" is on; based on Guillaume-Lechevallier's VRTRAKILL_FRAUD
    [HarmonyPatch]
    internal class VRIKP
    {
        // Starts the wait that adds the VR avatar rig controller to the player when it spawns.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(NewMovement), nameof(NewMovement.Start))]
        static void AttachVRIK(NewMovement __instance)
        {
            __instance.StartCoroutine(AttachWhenReady(__instance));
        }
        // Waits up to 300 frames for the avatar asset and both hand systems, then adds the rig controller once.
        static IEnumerator AttachWhenReady(NewMovement Movement)
        {
            for (int i = 0; i < 300 && !Ready(); i++) yield return null;
            if (Movement == null || !Ready())
            {
                Vars.Log.LogWarning("VR avatar skipped: the avatar asset or the hand controllers are not ready.");
                yield break;
            }
            if (Movement.gameObject.HasComponent<VRigController>()) yield break;

            MetaRig Rig = MetaRig.CreateVCustomPreset(null);
            if (Rig == null) yield break;
            Movement.gameObject.AddComponent<VRigController>().Rig = Rig;
            Rig.Root.localPosition = Vector3.zero;
        }
        // Returns true once the avatar asset and both hand systems with their controllers exist; Find does not auto-create them like Instance.
        static bool Ready()
        {
            if (Assets.VRig == null) return false;
            VRArmsSystem Arms = Object.FindObjectOfType<VRArmsSystem>();
            VRGunsSystem Guns = Object.FindObjectOfType<VRGunsSystem>();
            return Arms != null && Arms.CC != null && Guns != null && Guns.CC != null;
        }
    }
}
