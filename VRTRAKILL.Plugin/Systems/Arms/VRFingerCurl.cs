using UnityEngine;
using Valve.VR;
using Plugin.Systems.Controllers;
using Plugin.Systems.Input;
using Plugin.Systems.VRAvatar.Armature;

namespace Plugin.Systems.Arms;

// Curls the punch arm finger bones from the SteamVR skeleton finger curls while the fist is in its Idle state.
internal class VRFingerCurl : MonoBehaviour
{
    private struct JointSpec
    {
        public string Name; public int Finger; public Vector3 Axis; public float MaxAngle;
        // Stores a bone name, its SteamVR finger (0 thumb to 4 pinky), its curl axis in the bone frame and its curl angle at full curl.
        public JointSpec(string N, int F, float X, float Y, float Z, float A) { Name = N; Finger = F; Axis = new Vector3(X, Y, Z); MaxAngle = A; }
    }

    // Axis and angle of the rotation from the game's Idle pose to its fist, per bone, read from the punch arm animations:
    // Feedbacker from the Idle and Jab clips, Knuckleblaster from the Idle clip and the fist pose stored in the prefab.
    private static readonly JointSpec[] Specs =
    {
        new JointSpec("Thumb_01", 0, -.850f, .319f, .415f, 56), new JointSpec("Thumb_02", 0, .094f, .241f, -.967f, 62), new JointSpec("Thumb_03", 0, .249f, .138f, -.962f, 61),
        new JointSpec("Index_01", 1, -.107f, -.110f, -.989f, 81), new JointSpec("Index_02", 1, -.999f, .014f, .001f, 81), new JointSpec("Index_03", 1, -.999f, .015f, .003f, 77),
        new JointSpec("Middle_01", 2, -.992f, -.096f, .078f, 75), new JointSpec("Middle_02", 2, -1, .006f, 0, 80), new JointSpec("Middle_03", 2, -1, .005f, 0, 84),
        new JointSpec("Ring_01", 3, -.999f, -.027f, .021f, 68), new JointSpec("Ring_02", 3, -1, .008f, 0, 80), new JointSpec("Ring_03", 3, -.999f, .008f, 0, 74),
        new JointSpec("Pinky_01", 4, -.998f, .029f, -.058f, 64), new JointSpec("Pinky_02", 4, -1, .003f, 0, 86), new JointSpec("Pinky_03", 4, -1, .004f, .001f, 92),

        new JointSpec("thumb.1", 0, -.924f, -.157f, .352f, 42), new JointSpec("thumb.2", 0, 0, -.001f, 1, 68),
        new JointSpec("index.1", 1, 0, -.001f, 1, 109), new JointSpec("index.2", 1, -.001f, 0, .999f, 68),
        new JointSpec("pinkie.1", 4, -.001f, 0, 1, 107), new JointSpec("pinkie.2", 4, -.001f, 0, 1, 76),
    };

    private static readonly int IdleState = Animator.StringToHash("Idle");
    private const float CurlRate = 20f, FadeTime = .1f;

    private VRArmControllerBase Controller;
    private Punch Punch;
    private SteamVR_Input_Sources Source = SteamVR_Input_Sources.Any;

    private Transform[] Bones;
    private Vector3[] Axes;
    private float[] MaxAngles;
    private int[] Fingers;
    private Quaternion[] Rest, Written;
    private int Count;
    private bool Writing;
    private float Weight;
    private readonly float[] Raw = new float[5], Smoothed = new float[5];

    // Stores the arm controller and punch, and finds the finger bones of the joint table under the arm's hand bone.
    public void Setup(Arm A, VRArmControllerBase C, Punch P)
    {
        Controller = C; Punch = P;
        if (A == null || A.Hand == null || A.Hand.Root == null) { enabled = false; return; }

        Bones = new Transform[Specs.Length]; Axes = new Vector3[Specs.Length];
        MaxAngles = new float[Specs.Length]; Fingers = new int[Specs.Length];
        Rest = new Quaternion[Specs.Length]; Written = new Quaternion[Specs.Length];
        for (int i = 0; i < Specs.Length; i++)
        {
            Transform B = FindDeep(A.Hand.Root, Specs[i].Name);
            if (B == null) continue;
            Bones[Count] = B; Axes[Count] = Specs[i].Axis.normalized;
            MaxAngles[Count] = Specs[i].MaxAngle; Fingers[Count] = Specs[i].Finger;
            Rest[Count] = B.localRotation; Written[Count] = B.localRotation;
            Count++;
        }
        if (Count == 0) { enabled = false; Vars.Log.LogWarning($"Finger tracking: no known finger bones under {A.Hand.Root.name}."); }
    }

    // Returns the first transform under Root with the given name, searching depth first.
    private static Transform FindDeep(Transform Root, string Name)
    {
        for (int i = 0; i < Root.childCount; i++)
        {
            Transform Child = Root.GetChild(i);
            if (Child.name == Name) return Child;
            Transform Found = FindDeep(Child, Name);
            if (Found != null) return Found;
        }
        return null;
    }

    // Finds the hand the arm follows from the controller above the arm target; returns false until the target is set.
    private bool ResolveSource()
    {
        if (Source != SteamVR_Input_Sources.Any) return true;
        if (Controller == null || Controller.Target == null) return false;
        VRControllersSystem C = Controller.Target.GetComponentInParent<VRControllersSystem>();
        if (C == null) return false;
        Source = C.source;
        return true;
    }

    // Returns true while the fist idles: not punching, holding an item or shopping, with the base layer in Idle.
    private bool CanAnimate()
    {
        if (Punch == null || Punch.anim == null || !Punch.anim.isActiveAndEnabled) return false;
        if (!Punch.ready || Punch.holding || Punch.shopping) return false;
        return !Punch.anim.IsInTransition(0) && Punch.anim.GetCurrentAnimatorStateInfo(0).shortNameHash == IdleState;
    }

    // Rotates each finger bone from its animated pose by its smoothed curl, and fades back to the game pose when tracking stops.
    private void LateUpdate()
    {
        bool Track = ResolveSource() && CanAnimate() && SkeletonInput.TryGetCurls(Source, Raw);
        float Delta = Time.deltaTime;
        Weight = Mathf.MoveTowards(Weight, Track ? 1f : 0f, Delta / FadeTime);
        if (Track)
        {
            float K = 1f - Mathf.Exp(-CurlRate * Delta);
            for (int i = 0; i < 5; i++) Smoothed[i] = Mathf.Lerp(Smoothed[i], Raw[i], K);
        }

        if (Weight <= 0f)
        {
            if (Writing) RestorePose();
            return;
        }
        for (int i = 0; i < Count; i++)
        {
            Transform B = Bones[i];
            if (B == null) continue;
            Quaternion Current = B.localRotation;
            // the animator writes its bones every frame; a value equal to the last write means nothing animates this bone
            if (!Writing || Current != Written[i]) Rest[i] = Current;
            Quaternion Q = Rest[i] * Quaternion.AngleAxis(Smoothed[Fingers[i]] * MaxAngles[i] * Weight, Axes[i]);
            B.localRotation = Q;
            Written[i] = Q;
        }
        Writing = true;
    }

    // Puts back the pose from before the curl on bones that nothing else has written since.
    private void RestorePose()
    {
        for (int i = 0; i < Count; i++)
            if (Bones[i] != null && Bones[i].localRotation == Written[i]) Bones[i].localRotation = Rest[i];
        Writing = false;
    }
}
