using UnityEngine;

namespace Plugin.Systems.VRCamera
{
    internal class DesktopCamera : MonoBehaviour
    {
        Camera C;
        // Gets the camera on this object.
        public void Start() { C = gameObject.GetComponent<Camera>(); }
        // Copies the main camera settings to the desktop camera each frame and keeps it off the headset.
        public void Update()
        {
            C.nearClipPlane = Vars.MainCamera.nearClipPlane;
            C.farClipPlane = Vars.MainCamera.farClipPlane;
            C.depth = 69;
            C.stereoTargetEye = StereoTargetEyeMask.None;
            C.backgroundColor = Vars.MainCamera.backgroundColor;
            C.cullingMask = Vars.MainCamera.cullingMask;
            C.clearFlags = Vars.MainCamera.clearFlags;
            C.fieldOfView = Vars.MainCamera.fieldOfView;
        }
    }
}
