using UnityEngine;
using System.Collections;
using Plugin.Patches.ULTRAKILL;

namespace Plugin.Systems.Controllers
{
    public class VRArmsSystem : MonoSingleton<VRArmsSystem>
    {
        public VRControllersSystem CC;
        public GameObject GunOffset;

        public Vector3 ArmIKOffset = new Vector3(0, .05f, -.11f);

        private Vector3 _PreviousPosition, _CurrentVelocity; public float Speed = 0; // for punch detection
        private Vector3 LastPosition, Velocity; // for direction
        private IEnumerator CalculateSpeed()
        {
            _PreviousPosition = transform.position;

            yield return new WaitForEndOfFrame();

            _CurrentVelocity = (_PreviousPosition - transform.position) / Time.deltaTime;
            Speed = _CurrentVelocity.magnitude;
        }

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

        public void Update()
        {
            CC.ArmOffset.transform.localPosition = ArmIKOffset;

            StartCoroutine(CalculateSpeed());

            if (LastPosition != transform.position)
            {
                Velocity = (transform.position - LastPosition).normalized;
                Vars.punchDirection = Vars.NonDominantHand.transform.forward;
                LastPosition = transform.position;
            }

            SetControllers();
        }

        private void SetControllers()
        {
            if ((FistControl.Instance?.spawnedArms.Count == 0 && !Vars.IsPlayerFrozen)
            || ((bool)!FistControl.Instance?.activated && FistControl.Instance?.spawnedArms.Count == 0)
            || Vars.IsMainMenu)
                CC.RenderModel.SetActive(true);
            else CC.RenderModel.SetActive(false);
        }
    }
}
