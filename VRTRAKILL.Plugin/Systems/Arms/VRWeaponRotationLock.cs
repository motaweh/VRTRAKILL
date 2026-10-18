using VRTRAKILL.Utilities;
using UnityEngine;

namespace Plugin.Systems.Arms;

// For things that are "technically or not" weapons (revolver, fishing rod, etc.) and in need of a rotation locking
internal class VRWeaponRotationLock : VRArmControllerBase
{
    private Revolver Revolver;
    // Sets the target to the gun hand, or to the arm hand when left handed, and finds the revolver if this is one.
    public override void Start()
    {
        base.Start();
        Revolver = GetComponent<Revolver>();
        if (Vars.Config.Controllers.LeftHanded) Target = Vars.NonDominantHand.transform;
        else Target = Vars.DominantHand.transform;
    }
    // Turns the hand with the target; on the revolver, only while it spins.
    public override void LateUpdate()
    {
        if (Revolver != null)
        {
            if (Revolver.anim.GetBool("Spinning"))
            { Arm.Hand.Root.rotation = Target.rotation * Quaternion.Euler(OffsetRot); return; }
            return;
        }
        Arm.Hand.Root.rotation = Target.rotation * Quaternion.Euler(OffsetRot);
    }
}
