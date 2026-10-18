using HarmonyLib;
using Plugin.Systems.Input;
using System.Linq;
using ULTRAKILL.Portal;
using UnityEngine;
using UnityEngine.XR;
using VRBasePlugin.Systems.VRCamera;

namespace Plugin.Systems.VRCamera.Patches
{
    [HarmonyPatch]
    public class CameraConverterP
    {
        // adapted from HuskVR
        public static Camera DesktopWorldCam, DesktopUICam;
        public static Camera leftEye, rightEye;
        // null until the desktop view is toggled; then the toggle state carries over to the next scene
        public static bool? DesktopViewOn;

        // Sets up the main camera for VR and creates the left and right eye cameras; logs and returns if there is no camera yet.
        public static void SetupEyeCameras()
        {
            // PortalManagerV2.OnEnable can run before CameraController registers or sets cam (Start, CheckAspectRatio)
            CameraController cc = Vars.Registered<CameraController>() ?? Object.FindAnyObjectByType<CameraController>();
            if (cc != null && cc.cam == null) cc.cam = cc.GetComponent<Camera>();
            if (cc == null || cc.cam == null)
            {
                Vars.Log.LogWarning("No camera controller when the portal manager started; VR eye cameras not created.");
                return;
            }

            cc.cam.nearClipPlane = 0.01f;
            cc.cam.stereoTargetEye = StereoTargetEyeMask.None;

            // render the AlwaysOnTop layer (hands)
            cc.cam.cullingMask |= 1 << (int)Layers.AlwaysOnTop;
            if (cc.hudCamera != null) cc.hudCamera.enabled = false;

            leftEye = new GameObject("Left", typeof(Camera)).GetComponent<Camera>();
            VRTRAKILL.Utilities.Unity.CopyCameraValues(leftEye, cc.cam);
            leftEye.transform.SetParent(cc.transform.parent);
            leftEye.stereoTargetEye = StereoTargetEyeMask.Left;

            rightEye = new GameObject("Right", typeof(Camera)).GetComponent<Camera>();
            VRTRAKILL.Utilities.Unity.CopyCameraValues(rightEye, cc.cam);
            rightEye.transform.SetParent(cc.transform.parent);
            rightEye.stereoTargetEye = StereoTargetEyeMask.Right;

            SetupDesktopCameras(cc);
        }
        // Creates the monitor-only world and UI cameras on the left eye, shown when the desktop view is on.
        static void SetupDesktopCameras(CameraController cc)
        {
            if (DesktopWorldCam != null) Object.Destroy(DesktopWorldCam.gameObject);
            if (DesktopUICam != null) Object.Destroy(DesktopUICam.gameObject);

            DesktopWorldCam = new GameObject("Desktop World Camera", typeof(Camera)).GetComponent<Camera>();
            VRTRAKILL.Utilities.Unity.CopyCameraValues(DesktopWorldCam, cc.cam);
            DesktopWorldCam.transform.SetParent(leftEye.transform, false);
            DesktopWorldCam.stereoTargetEye = StereoTargetEyeMask.None;
            DesktopWorldCam.gameObject.AddComponent<DesktopCamera>();

            DesktopUICam = new GameObject("Desktop UI Camera", typeof(Camera)).GetComponent<Camera>();
            DesktopUICam.transform.SetParent(leftEye.transform, false);
            DesktopUICam.stereoTargetEye = StereoTargetEyeMask.None;
            DesktopUICam.clearFlags = CameraClearFlags.Depth;
            DesktopUICam.cullingMask = 1 << (int)Layers.UI;
            DesktopUICam.gameObject.AddComponent<DesktopUICamera>();

            bool on = DesktopViewOn ?? Vars.Config.DesktopView.Enabled;
            DesktopWorldCam.gameObject.SetActive(on);
            DesktopUICam.gameObject.SetActive(on);
        }
        // Turns off the virtual camera and the main camera before the camera controller starts, if the camera is set.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CameraController), nameof(CameraController.Start))]
        static void ConvertCameras(CameraController __instance)
        {
            // OnEnable sets cam through CheckAspectRatio; the old busy-wait here could never end on the main thread
            if (__instance.cam == null)
            {
                Vars.Log.LogWarning("CameraController.Start ran without a camera; VR camera conversion skipped.");
                return;
            }

            // deactivate, not destroy: destroying it breaks the game
            GameObject VirtualCamera = GameObject.Find("Virtual Camera");
            if (VirtualCamera != null) VirtualCamera.SetActive(false);

            __instance.cam.enabled = false;
        }
        // Restores the player rigidbody constraints and turns cc back on when a hit kills the player.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(NewMovement), nameof(NewMovement.GetHurt))]
        public static void FixWeirdDeathThing(NewMovement __instance)
        {
            if (__instance.dead)
            {
                __instance.rb.constraints = __instance.defaultRBConstraints;
                __instance.cc.enabled = true;
            }

        }
        // Adds the VR keybinds controller to the player and resets the turn offset.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(NewMovement), nameof(NewMovement.Start))]
        static void ScaleObjects(NewMovement __instance)
        {
            __instance.gameObject.AddComponent<VRPlayer.VRKeybindsController>();

            InputVars.TurnOffset = 0;
        }


        // Drives the FPS camera rotation from the headset and places the eye cameras on the FPS player, through portals if needed.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CameraController), nameof(CameraController.LateUpdate))]
        static void HandleRotationsAndPositions(CameraController __instance)
        {
            // the platformer camera is handled by CrashBandicrootCCP; leave its eyes there
            if (__instance.platformerCamera) return;
            if (leftEye == null || rightEye == null) return;
            // back from 4-S or Clash mode: return the eyes to the FPS player
            if (__instance == CameraController.Instance && leftEye.transform.parent != __instance.transform.parent)
            {
                leftEye.transform.SetParent(__instance.transform.parent);
                rightEye.transform.SetParent(__instance.transform.parent);
            }

            if (!__instance.player && Vars.Registered<NewMovement>() is NewMovement NM) { __instance.nm = NM; __instance.player = NM.gameObject; }

            __instance.rotationX = -VRControllerLocations.Instance.headRot.eulerAngles.x;
            __instance.rotationY = VRControllerLocations.Instance.headRot.eulerAngles.y + InputVars.TurnOffset;
            __instance.tiltRotationZ = VRControllerLocations.Instance.headRot.eulerAngles.z;
            __instance.ApplyRotations();

            PortalAwareSetTransformFromBody(leftEye.transform, InputTracking.GetLocalPosition(XRNode.LeftEye), __instance.transform.rotation);
            PortalAwareSetTransformFromBody(rightEye.transform, InputTracking.GetLocalPosition(XRNode.RightEye), __instance.transform.rotation);
        }

        // Places a transform at a pose relative to the player body, with turn and gravity applied, then moves it through crossed portals.
        public static void PortalAwareSetTransformFromBody(Transform t, Vector3 localPosition, Quaternion worldRotation, bool hands = false)
        {
            CameraController cc = Vars.Registered<CameraController>();
            if (cc == null || cc.transform.parent == null) return;
            Quaternion parentRot =
                cc.gravityRotation *
                Quaternion.AngleAxis(InputVars.TurnOffset, Vector3.up);

            Vector3 transformedLocalPos = parentRot * localPosition;
            t.position = cc.transform.parent.position + transformedLocalPos;

            if (hands) t.rotation = parentRot * worldRotation;
            else t.rotation = worldRotation;

            MoveFromPlayerThroughPortals(t);
        }

        // Moves a transform through any portal crossed between the player and the transform.
        public static void MoveFromPlayerThroughPortals(Transform t)
        {
            CameraController cc = Vars.Registered<CameraController>();
            PortalManagerV2 pm = Vars.Registered<PortalManagerV2>();
            if (cc == null || cc.transform.parent == null || pm == null || pm.Scene == null) return;
            PortalScene Scene = pm.Scene;

            PortalHandle portalHandle;
            Vector3 intersection;
            PortalTravellerFlags portalTravellerFlags = PortalTravellerFlags.Player;

            // adapted from PortalManagerV2
            if (Scene.FindCrossedPortal(cc.transform.parent.position, t.transform.position, out portalHandle, out intersection))
            {
                Portal portalObject = Scene.GetPortalObject(portalHandle);
                PortalTravellerFlags travelFlags = portalObject.GetTravelFlags(portalHandle.side);
                bool canTravel = travelFlags.HasFlag(portalTravellerFlags);
                PortalHandleSequence portalSequence = new PortalHandleSequence(new PortalHandle[]
                {
                    portalHandle
                });
                if (canTravel)
                {
                    Matrix4x4 travelMatrix = Scene.GetTravelMatrix(portalHandle);
                    Vector3 vector2 = travelMatrix.MultiplyPoint3x4(intersection);
                    Vector3 direction = travelMatrix.MultiplyPoint3x4(t.transform.position) - vector2;
                    PortalTraversalV2[] intersections;
                    PortalPhysicsV2.ProjectThroughPortals(vector2, direction, pm.empty_lm, out _, out _, out intersections);
                    for (int i = 0; i < intersections.Length; i++)
                    {
                        PortalHandle portalHandle2 = intersections[i].portalHandle;
                        if (!Scene.GetPortalObject(portalHandle2).GetTravelFlags(intersections[i].portalHandle.side).HasFlag(portalTravellerFlags))
                        {
                            canTravel = false;
                            break;
                        }
                    }
                    if (canTravel)
                    {
                        if (intersections.Length != 0)
                        {
                            portalSequence = PortalHandleSequence.Prepend(portalHandle, intersections);
                            travelMatrix = Scene.GetTravelMatrix(portalSequence);
                        }
                        PortalTravelDetails details = PortalTravelDetails.WithInteresction(portalSequence, intersections, travelMatrix, intersection);

                        t.transform.position = details.enterToExit.MultiplyPoint3x4(t.transform.position);
                        t.transform.rotation = details.enterToExit.rotation * t.transform.rotation;
                    }
                }
            }
        }

        // Stores the camera yaw before CameraController.Transform runs.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CameraController), nameof(CameraController.Transform))]
        static void TransformBefore(CameraController __instance, ref float __state)
        {
            __state = __instance.rotationY;
        }
        // Adds the yaw change from CameraController.Transform to the turn offset.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CameraController), nameof(CameraController.Transform))]
        static void TransformAfter(CameraController __instance, ref float __state)
        {
            InputVars.TurnOffset += __instance.rotationY - __state;
        }

        // Returns the gun hand position as the camera default position.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CameraController), nameof(CameraController.GetDefaultPos))]
        static bool GetDefaultPos(ref Vector3 __result)
        {
            // Instance never returns null: without a VR hand it auto-creates a VRGunsSystem with no GunOffset
            Controllers.VRGunsSystem Guns = Controllers.VRGunsSystem.Instance;
            __result = Guns != null && Guns.GunOffset != null ? Guns.GunOffset.transform.position : Vector3.zero;
            return false;
        }

        // Adds the VR head to the left eye and removes the player model renderers.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerAnimations), nameof(PlayerAnimations.Start))]
        static void RemovePlayerModel(PlayerAnimations __instance)
        {
            // VR head
            GameObject head = GameObject.Instantiate(Assets.VHead, leftEye.transform);
            head.transform.localPosition = new Vector3(0, 0, -0.07f);
            head.transform.localScale *= 2f;
            Object.Destroy(head.GetComponent<CapsuleCollider>());
            foreach (Transform t in head.GetComponentsInChildren<Transform>(true))
                t.gameObject.layer = LayerMask.NameToLayer("Portal");
            // reuse the player model's material for the correct unlit look
            head.GetComponentInChildren<SkinnedMeshRenderer>().material = __instance.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material;

            __instance.GetComponentsInChildren<SkinnedMeshRenderer>(true).ToList().ForEach(x => UnityEngine.Object.Destroy(x));
            __instance.GetComponentsInChildren<MeshRenderer>(true).ToList().ForEach(x => UnityEngine.Object.Destroy(x));
            __instance.GetComponentsInChildren<GunColorGetter>(true).ToList().ForEach(x => UnityEngine.Object.Destroy(x));
        }
    }
}
