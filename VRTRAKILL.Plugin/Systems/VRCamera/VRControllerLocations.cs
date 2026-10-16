using Plugin.Systems;
using Plugin.Systems.Input;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            transform.position = Vector3.zero; //IK its unneccessary but just make sure

            //if (_head != null) GameObject.DestroyImmediate(_head.gameObject);
            //_head = new GameObject("Head").transform;
            //_head.SetParent(transform);
            //_head.gameObject.AddComponent<SteamVR_TrackedObject>();

            SteamVR_Actions._default.Activate();
            yield return null;

            if (_left != null) GameObject.DestroyImmediate(_left.gameObject);
            _left = new GameObject("Left").transform;
            _left.SetParent(transform);
            _left.gameObject.SetActive(false); //OnEnable would bind the default "Pose" action before poseAction is set
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

            // OnEnable reads the pose at once, which logs NoData while SteamVR Input still has no pose for the hand
            float waitStart = Time.realtimeSinceStartup; bool warnedL = false, warnedR = false;
            while (!l.gameObject.activeSelf || !r.gameObject.activeSelf)
            {
                yield return null;
                if (!SteamVR_Input.initialized || SteamVR_Input.isStartupFrame || OpenVR.Input == null) continue;

                EnableWhenPoseReady(l, ref warnedL, waitStart);
                EnableWhenPoseReady(r, ref warnedR, waitStart);
            }
        }

        private const float PoseWaitWarnTime = 10f;
        private static InputPoseActionData_t probeData;

        // raw probe so the wait itself logs nothing; any error other than NoData enables the pose so SteamVR reports it
        private static void EnableWhenPoseReady(SteamVR_Behaviour_Pose Pose, ref bool Warned, float WaitStart)
        {
            if (Pose.gameObject.activeSelf) return;

            EVRInputError error = OpenVR.Input.GetPoseActionDataForNextFrame(Pose.poseAction.handle, SteamVR_Settings.instance.trackingSpace,
                ref probeData, (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(InputPoseActionData_t)), SteamVR_Input_Source.GetHandle(Pose.inputSource));
            if (error != EVRInputError.NoData) { Pose.gameObject.SetActive(true); return; }

            if (!Warned && Time.realtimeSinceStartup - WaitStart > PoseWaitWarnTime)
            {
                Warned = true;
                Vars.Log.LogWarning($"No SteamVR pose data for {Pose.inputSource} after {PoseWaitWarnTime}s (NoData). Hand tracking starts once the controller reports a pose.");
            }
        }

        private bool IsTurning; private float SnapTurnTimer;
        public void Update()
        {
            //if (_head != null)
            {
                //headPos = _head.position;
                //headRot = _head.rotation;
                headPos = InputTracking.GetLocalPosition(XRNode.Head);
                headRot = InputTracking.GetLocalRotation(XRNode.Head);
            }
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
