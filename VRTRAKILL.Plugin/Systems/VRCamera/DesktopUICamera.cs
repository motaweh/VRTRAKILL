using UnityEngine;

namespace Plugin.Systems.VRCamera
{
    internal class DesktopUICamera : MonoBehaviour
    {
        Camera C;
        // Gets the camera on this object.
        public void Start() { C = gameObject.GetComponent<Camera>(); }
        // Copies the UI camera layers and clear flags to the desktop UI camera, with the configured FOV, off the headset.
        public void Update()
        {
            if (C == null || Vars.UICamera == null) return;
            C.nearClipPlane = 0.1f;
            C.depth = 70;
            C.stereoTargetEye = StereoTargetEyeMask.None;
            C.cullingMask = Vars.UICamera.cullingMask;
            C.clearFlags = Vars.UICamera.clearFlags;
            C.fieldOfView = Vars.Config.DesktopView.UICamFOV;
        }
    }
}
