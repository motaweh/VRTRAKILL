using HarmonyLib;
using UnityEngine;
using UnityEngine.XR;

namespace Plugin.Systems.VRCamera.Patches
{
    // 4-S and Clash mode turn off the FPS player, and the eye cameras with it; based on Guillaume-Lechevallier's VRTRAKILL_FRAUD
    [HarmonyPatch]
    internal class CrashBandicrootCCP
    {
        // Puts both eye cameras on the platformer camera, turned by the headset, after the platformer camera moves.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlatformerMovement), nameof(PlatformerMovement.Update))]
        static void FollowPlatformerCamera(PlatformerMovement __instance)
        {
            Transform origin = __instance.platformerCamera;
            if (origin == null || CameraConverterP.leftEye == null || CameraConverterP.rightEye == null) return;

            Quaternion headRot = InputTracking.GetLocalRotation(XRNode.Head);
            Vector3 headPos = InputTracking.GetLocalPosition(XRNode.Head);
            PlaceEye(CameraConverterP.leftEye.transform, origin, InputTracking.GetLocalPosition(XRNode.LeftEye) - headPos, headRot);
            PlaceEye(CameraConverterP.rightEye.transform, origin, InputTracking.GetLocalPosition(XRNode.RightEye) - headPos, headRot);
        }
        // Parents an eye to the platformer camera so it stays active, then places it at its offset from the head centre.
        static void PlaceEye(Transform eye, Transform origin, Vector3 offsetFromHead, Quaternion headRot)
        {
            if (eye.parent != origin) eye.SetParent(origin, false);
            eye.SetPositionAndRotation(origin.TransformPoint(offsetFromHead), origin.rotation * headRot);
        }
    }
}
