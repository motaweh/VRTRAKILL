using UnityEngine;
using System.Collections;

namespace Plugin.Systems.Controllers
{
    public class VRArmsSystem : MonoSingleton<VRArmsSystem>
    {
        public VRControllersSystem CC;
        public GameObject GunOffset;

        public Vector3 ArmIKOffset = new Vector3(0, .05f, -.11f);

        private Vector3 _PreviousPosition, _CurrentVelocity; public float Speed = 0; // for punch detection
        private Vector3 LastPosition; // for direction
        // Measures the hand speed from now to the end of the frame for punch detection.
        private IEnumerator CalculateSpeed()
        {
            _PreviousPosition = transform.position;

            yield return new WaitForEndOfFrame();

            _CurrentVelocity = (_PreviousPosition - transform.position) / Time.deltaTime;
            Speed = _CurrentVelocity.magnitude;
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

            StartCoroutine(CalculateSpeed());

            if (LastPosition != transform.position)
            {
                Vars.punchDirection = Vars.NonDominantHand.transform.forward;
                LastPosition = transform.position;
            }

            SetControllers();
        }

        // Shows the controller model when no arms are spawned or in the main menu.
        private void SetControllers()
        {
            FistControl Fists = FistControl.Instance;
            bool NoArms = Fists != null && Fists.spawnedArms.Count == 0;
            if ((NoArms && !Vars.IsPlayerFrozen) || (NoArms && !Fists.activated) || Vars.IsMainMenu)
                CC.RenderModel.SetActive(true);
            else CC.RenderModel.SetActive(false);
        }
    }
}
