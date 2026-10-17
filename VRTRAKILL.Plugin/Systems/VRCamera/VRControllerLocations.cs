using Plugin.Systems;
using Plugin.Systems.Input;
using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

namespace VRBasePlugin.Systems.VRCamera
{
    [ConfigureSingleton(SingletonFlags.PersistAutoInstance)]
    internal class VRControllerLocations : MonoSingleton<VRControllerLocations>
    {
        public Vector3 headPos;
        public Quaternion headRot = Quaternion.identity;

        public Vector3 leftPos;
        public Quaternion leftRot = Quaternion.identity;
        private Transform _left;

        public Vector3 rightPos;
        public Quaternion rightRot = Quaternion.identity;
        private Transform _right;

        IEnumerator Start()
        {
            transform.position = Vector3.zero;

            SteamVR_Actions._default.Activate();
            yield return null;

            if (_left != null) GameObject.DestroyImmediate(_left.gameObject);
            _left = new GameObject("Left").transform;
            _left.SetParent(transform);
            _left.gameObject.SetActive(false); // OnEnable would bind the default "Pose" action before poseAction is set
            SteamVR_Behaviour_Pose l = _left.gameObject.AddComponent<SteamVR_Behaviour_Pose>();
            l.poseAction = SteamVR_Actions._default.LeftPose;
            l.inputSource = SteamVR_Input_Sources.LeftHand;

            if (_right != null) GameObject.DestroyImmediate(_right.gameObject);
            _right = new GameObject("Right").transform;
            _right.SetParent(transform);
            _right.gameObject.SetActive(false);
            SteamVR_Behaviour_Pose r = _right.gameObject.AddComponent<SteamVR_Behaviour_Pose>();
            r.poseAction = SteamVR_Actions._default.RightPose;
            r.inputSource = SteamVR_Input_Sources.RightHand;

            l.gameObject.SetActive(false);
            r.gameObject.SetActive(false);
            yield return null;
            l.gameObject.SetActive(true);
            r.gameObject.SetActive(true);
        }

        private bool IsTurning; private float SnapTurnTimer;
        public void Update()
        {
            headPos = InputTracking.GetLocalPosition(XRNode.Head);
            headRot = InputTracking.GetLocalRotation(XRNode.Head);
            if (_left != null)
            {
                leftPos = _left.position;
                leftRot = _left.rotation;
            }
            if (_right != null)
            {
                rightPos = _right.position;
                rightRot = _right.rotation;
            }

            if (!Vars.Config.Controllers.SnapTurn)
            {
                if (InputVars.TurnVector.x > 0 + Vars.Config.Controllers.Deadzone)
                    InputVars.TurnOffset += Vars.Config.Controllers.SmoothSpeed * Time.deltaTime;
                if (InputVars.TurnVector.x < 0 - Vars.Config.Controllers.Deadzone)
                    InputVars.TurnOffset -= Vars.Config.Controllers.SmoothSpeed * Time.deltaTime;
            }
            else
            {
                if (IsTurning)
                {
                    SnapTurnTimer += Time.deltaTime;
                    if (SnapTurnTimer >= .2f || InputVars.TurnVector.x == 0) { IsTurning = false; SnapTurnTimer = 0; }
                }
                else
                {
                    if (InputVars.TurnVector.x > 0 + Vars.Config.Controllers.Deadzone)
                    { IsTurning = true; InputVars.TurnOffset += Vars.Config.Controllers.SnapAngles; }
                    else if (InputVars.TurnVector.x < 0 - Vars.Config.Controllers.Deadzone)
                    { IsTurning = true; InputVars.TurnOffset -= Vars.Config.Controllers.SnapAngles; }
                }
            }
        }
    }
}
