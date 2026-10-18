using UnityEngine;
using Valve.VR;

namespace Plugin.Systems.Input
{
    // Reads the hand skeleton actions from actions.json; the generated SteamVR_Actions class does not declare them.
    internal static class SkeletonInput
    {
        private const string LeftPath = "/actions/default/in/SkeletonLeftHand",
                             RightPath = "/actions/default/in/SkeletonRightHand";

        private static SteamVR_Action_Skeleton Left, Right;
        private static bool Failed, LoggedLeft, LoggedRight;
        private static int UpdatedFrame = -1;

        // Creates both skeleton actions once SteamVR input is initialized; returns false if they cannot be used.
        private static bool EnsureCreated()
        {
            if (Failed) return false;
            if (Left != null) return true;
            if (!SteamVR_Input.initialized) return false;
            try
            {
                Left = Create(LeftPath);
                Right = Create(RightPath);
                if (Left == null || Right == null)
                {
                    Failed = true; Left = null; Right = null;
                    Vars.Log.LogWarning("Finger tracking: SteamVR has no handle for the skeleton actions. Hands keep their fixed pose.");
                    return false;
                }
                Vars.Log.LogInfo("Finger tracking: skeleton actions created.");
                return true;
            }
            catch (System.Exception E)
            {
                Failed = true; Left = null; Right = null;
                Vars.Log.LogError($"Finger tracking turned off, the skeleton actions failed: {E.Message}");
                return false;
            }
        }

        // Creates one skeleton action from its path that reads summary data only, or returns null if SteamVR gives no handle.
        private static SteamVR_Action_Skeleton Create(string Path)
        {
            // GetAction returns an uninitialized stand-in for actions missing from SteamVR_Actions, so the action is built from its path
            SteamVR_Action_Skeleton A = SteamVR_Action.Create<SteamVR_Action_Skeleton>(Path);
            A.Initialize(true, false);
            if (A.handle == 0) return null;
            A.onlyUpdateSummaryData = true;
            return A;
        }

        // Reads both skeleton actions once per frame.
        private static void UpdateOnce()
        {
            if (UpdatedFrame == Time.frameCount) return;
            UpdatedFrame = Time.frameCount;
            Left.UpdateValueWithoutEvents();
            Right.UpdateValueWithoutEvents();
        }

        // Copies the thumb, index, middle, ring and pinky curls (0 open, 1 closed) of a hand; returns false when the hand has no skeleton data.
        public static bool TryGetCurls(SteamVR_Input_Sources Hand, float[] Curls)
        {
            if (Curls == null || Curls.Length < 5 || !EnsureCreated()) return false;
            SteamVR_Action_Skeleton A = Hand == SteamVR_Input_Sources.LeftHand ? Left
                                      : Hand == SteamVR_Input_Sources.RightHand ? Right : null;
            if (A == null) return false;
            try { UpdateOnce(); }
            catch (System.Exception E)
            {
                Failed = true; Left = null; Right = null;
                Vars.Log.LogError($"Finger tracking turned off, the skeleton update failed: {E.Message}");
                return false;
            }
            if (!A.GetActive() || !A.GetDeviceIsConnected()) return false;
            float[] Source = A.fingerCurls;
            if (Source == null || Source.Length < 5) return false;
            for (int i = 0; i < 5; i++)
                Curls[i] = float.IsNaN(Source[i]) ? 0f : Mathf.Clamp01(Source[i]);

            if (Hand == SteamVR_Input_Sources.LeftHand && !LoggedLeft) { LoggedLeft = true; Vars.Log.LogInfo("Finger tracking: left hand skeleton data received."); }
            else if (Hand == SteamVR_Input_Sources.RightHand && !LoggedRight) { LoggedRight = true; Vars.Log.LogInfo("Finger tracking: right hand skeleton data received."); }
            return true;
        }
    }
}
