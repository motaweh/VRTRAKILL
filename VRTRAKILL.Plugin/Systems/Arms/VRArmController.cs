using VRTRAKILL.Utilities;
using UnityEngine;

namespace Plugin.Systems.Arms;

internal class VRArmController : VRArmControllerBase
{
    public Transform RawTarget { get; set; }
    private bool IsHookArm;
    // Picks the hand the arm follows: the gun hand for left handed or sandbox arms, else the arm hand.
    public override void Start()
    {
        base.Start();
        IsHookArm = gameObject.HasComponent<HookArm>();

        if (Vars.Config.Controllers.LeftHanded || gameObject.HasComponent<Sandbox.Arm.SandboxArm>())
        { Target = Vars.DominantHand.transform; RawTarget = Vars.NDHC.transform; }
        else { Target = Vars.NonDominantHand.transform; RawTarget = Vars.DHC.transform; }
    }

    // Moves the arm to its target hand and turns the hand with it.
    public override void LateUpdate()
    {
        if (Vars.IsMainMenu || Arm == null) return;

        if (!IsHookArm) base.LateUpdate();
        Arm.GameObject.position = Target.position;
        Arm.GameObject.rotation = Target.rotation;

        Arm.Root.localPosition = OffsetPos;
        Arm.Hand.Root.rotation = Target.rotation * Quaternion.Euler(OffsetRot);

        if (Arm.Type == VRAvatar.Armature.ArmType.Feedbacker)
            Arm.Hand.Root.rotation *= Quaternion.Euler(0, 90, 0);
    }
}
