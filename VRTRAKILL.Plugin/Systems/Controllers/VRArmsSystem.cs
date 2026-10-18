using UnityEngine;
using Valve.VR;
using VRBasePlugin.Systems.VRCamera;

namespace Plugin.Systems.Controllers
{
    public class VRArmsSystem : MonoSingleton<VRArmsSystem>
    {
        public VRControllersSystem CC;
        public GameObject GunOffset;

        public Vector3 ArmIKOffset = new Vector3(0, .05f, -.11f);

        private Vector3 _PreviousPosition; private bool _HasPreviousPosition; public float Speed = 0; // for punch detection
        private Vector3 LastPosition; // for direction
        // Measures the hand speed since the last frame in tracking space, for punch detection.
        private void CalculateSpeed()
        {
            // world position and scaled time added locomotion and multiplied by the weapon wheel and hitstop time scales
            Vector3 Position = CC.source == SteamVR_Input_Sources.LeftHand ? VRControllerLocations.Instance.leftPos : VRControllerLocations.Instance.rightPos;
            float Delta = Time.unscaledDeltaTime;
            if (_HasPreviousPosition && Delta > 0f) Speed = (Position - _PreviousPosition).magnitude / Delta;
            _PreviousPosition = Position; _HasPreviousPosition = true;
        }

        // Gets the controller and gun offset, and puts the UI event camera on the hand for controller based UI.
        public void Awake()
        {
            CC = gameObject.GetComponent<VRControllersSystem>();
            GunOffset = CC.GunOffset;
            LastPosition = transform.position;

            if (Vars.Config.UIInteraction.ControllerBased)
            {
                // local zero keeps the ray on the hand wherever the hand is at creation
                UI.UIConverter.UIEventCamera.transform.SetParent(GunOffset.transform, false);
                UI.UIConverter.UIEventCamera.transform.localPosition = Vector3.zero;
                UI.UIConverter.UIEventCamera.transform.localRotation = Quaternion.identity;
            }
        }

        // Sets the arm IK offset, measures hand speed, updates the punch direction and toggles the controller model.
        public void Update()
        {
            CC.ArmOffset.transform.localPosition = ArmIKOffset;

            CalculateSpeed();

            if (LastPosition != transform.position)
            {
                Vars.punchDirection = Vars.NonDominantHand.transform.forward;
                LastPosition = transform.position;
            }

            SetControllers();
        }

        // Shows the controller model when no arms are spawned or in the main menu, if the controller has a model.
        private void SetControllers()
        {
            if (CC.RenderModel == null) return;
            FistControl Fists = Vars.Registered<FistControl>();
            bool NoArms = Fists != null && Fists.spawnedArms.Count == 0;
            if ((NoArms && !Vars.IsPlayerFrozen) || (NoArms && !Fists.activated) || Vars.IsMainMenu)
                CC.RenderModel.SetActive(true);
            else CC.RenderModel.SetActive(false);
        }
    }
}
