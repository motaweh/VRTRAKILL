using Plugin.Systems;
using UnityEngine;

namespace Plugin.Patches.ULTRAKILL;
internal class VRGunsController : MonoSingleton<VRGunsController>
{
    public Camera TargeterCamera;

    // Registers this as the instance, so readers can use Vars.Registered instead of Instance, which auto-creates a hand-less copy.
    public void Awake() => MakeCurrent();

    // Creates the hidden targeter camera on the guns and gives it to the frustum targeter.
    public void Start()
    {
        if (TargeterCamera == null || TargeterCamera is null)
        {
            TargeterCamera = new GameObject("TargeterCamera").AddComponent<Camera>();
            TargeterCamera.enabled = false;
            TargeterCamera.stereoTargetEye = StereoTargetEyeMask.None;
            TargeterCamera.clearFlags = CameraClearFlags.Nothing;
            TargeterCamera.depth = 99;
            TargeterCamera.fieldOfView = 90;
            TargeterCamera.nearClipPlane = .01f;
            TargeterCamera.cullingMask = -1;

            TargeterCamera.transform.parent = transform;
            TargeterCamera.transform.localPosition = Vector3.zero;
            TargeterCamera.transform.localEulerAngles = Vector3.zero;
        }
        if (CameraFrustumTargeter.Instance is CameraFrustumTargeter Targeter && Targeter != null) Targeter.camera = TargeterCamera;
    }

    // Moves the guns and the targeter camera to the gun hand, and turns the guns with it unless auto aim has a target.
    public void LateUpdate()
    {
        if (TargeterCamera != null)
            TargeterCamera.transform.position = Vars.DominantHand.transform.position;

        transform.position = Vars.DominantHand.transform.position;
        if (CameraFrustumTargeter.Instance is CameraFrustumTargeter Targeter && Targeter != null
            && Targeter.isActiveAndEnabled && CameraFrustumTargeter.isEnabled && Targeter.CurrentTarget)
            return;
        transform.rotation = Vars.DominantHand.transform.rotation;
    }
}
