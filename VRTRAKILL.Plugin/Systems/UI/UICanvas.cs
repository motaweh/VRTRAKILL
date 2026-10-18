using UnityEngine;
using UnityEngine.UI;

namespace Plugin.Systems.UI
{
    // adapted from HuskVR
    internal sealed class UICanvas : MonoBehaviour
    {
        private Vector3 LastCamFwd = Vector3.zero;
        private int LastChildren;

        private const float Distance = 72f;
        // Returns the UI size from the config.
        private static float Scale => Vars.Config.UIInteraction.UISize;

        // Stores the gaze direction at the UI distance and turns the canvas with the camera.
        private void UpdatePos()
        {
            LastCamFwd = Vars.MainCamera.transform.forward * Distance;
            transform.rotation = Vars.MainCamera.transform.rotation;
        }
        // Flattens the stored direction against gravity and turns the canvas upright along it.
        private void ResetPos()
        {
            Vector3 customUp = Vars.Registered<CameraController>() is CameraController CC ? CC.gravityRotation * Vector3.up : Vector3.up;

            LastCamFwd = Vector3.ProjectOnPlane(LastCamFwd, customUp);
            if (LastCamFwd != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(LastCamFwd, customUp);
        }
        // Re-takes the gaze yaw at full distance; a near-vertical gaze takes its heading from the head's up axis.
        private void Recenter()
        {
            UpdatePos();
            Vector3 customUp = Vars.Registered<CameraController>() is CameraController CC ? CC.gravityRotation * Vector3.up : Vector3.up;
            Vector3 Flat = Vector3.ProjectOnPlane(LastCamFwd, customUp);
            if (Flat.sqrMagnitude < Distance * Distance * .0625f)
                Flat = Vector3.ProjectOnPlane(Vars.MainCamera.transform.up * (Vector3.Dot(LastCamFwd, customUp) < 0 ? 1 : -1), customUp);
            if (Flat != Vector3.zero) LastCamFwd = Flat.normalized * Distance;
        }

        // Returns a hash of the active direct children, which changes when the menu changes page.
        private int ActiveChildren()
        {
            int Sig = 17;
            foreach (Transform Child in transform)
                if (Child.gameObject.activeSelf) Sig = Sig * 31 + Child.GetInstanceID();
            return Sig;
        }

        // Scales the canvas, sizes a root scaled canvas to its reference resolution and places it in front of the view.
        public void Start()
        {
            transform.localScale = Vector3.one * Scale;
            // the converted rect keeps the monitor's aspect, which pushes edge-anchored panels off to the sides
            if (GetComponent<Canvas>() is Canvas C && C.isRootCanvas
                && GetComponent<CanvasScaler>() is CanvasScaler CS && CS.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                RectTransform RT = (RectTransform)transform;
                RT.sizeDelta = CS.referenceResolution;
                RT.pivot = new Vector2(.5f, .5f); // keeps the rect centred on the gaze point
            }
            LastCamFwd = Vector3.back * Distance;
            UpdatePos();
            LastChildren = ActiveChildren();
        }
        // Keeps the canvas in front of the view; while frozen, holds it in place unless the menu page changes.
        public void LateUpdate()
        {
            int Children = ActiveChildren();
            if (!Vars.IsPlayerFrozen) UpdatePos();
            else
            {
                if (Children != LastChildren) Recenter();
                ResetPos();
            }
            LastChildren = Children;
            transform.position = Vars.MainCamera.transform.position + LastCamFwd;
        }
    }
}
