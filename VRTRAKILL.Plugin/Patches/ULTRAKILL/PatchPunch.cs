using HarmonyLib;
using Plugin.Systems;
using Plugin.Systems.Arms;
using Plugin.Systems.VRAvatar.Armature;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Plugin.Patches.ULTRAKILL;

[HarmonyPatch(typeof(Punch))]
public sealed class PatchPunch
{
    // Adds the VR arm parts and finger curl for the fist type, keeps its meshes updating off screen and turns off the punch trail.
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Punch.Start))]
    private static void Start(Punch __instance)
    {
        Arm A = null;
        switch (__instance.type)
        {
            case FistType.Standard: A = Arm.FeedbackerPreset(__instance.transform); break;
            case FistType.Heavy: A = Arm.KnuckleblasterPreset(__instance.transform); break;
            case FistType.Spear: default: break;
        }
        VRArmTransformer AT = __instance.gameObject.AddComponent<VRArmTransformer>();
        VRArmControllerBase AC = __instance.gameObject.AddComponent<VRArmController>();
        AT.Arm = A; AC.Arm = A;
        if (Vars.Config.Controllers.FingerTracking && A != null)
            __instance.gameObject.AddComponent<VRFingerCurl>().Setup(A, AC, __instance);

        foreach (SkinnedMeshRenderer SMR in __instance.GetComponentsInChildren<SkinnedMeshRenderer>())
            SMR.updateWhenOffscreen = true;

        __instance.tr.enabled = false;
    }

    // Starts a punch when the arm moves fast enough with punch held and the fist cooldown and stamina allow it, and blocks the game's own punch for this frame.
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Punch.Update))]
    private static void Update(Punch __instance, out bool __state)
    {
        __state = __instance.ready;
        if (MonoSingleton<OptionsManager>.Instance.paused) return;

        if (Vars.NDHC.Speed * 2 >= Vars.Config.MBP.PunchingSpeed
            && MonoSingleton<InputManager>.Instance.InputSource.Punch.IsPressed
            && __instance.ready && !__instance.shopping
            // the game's own gate (Punch.Update); without it stamina went negative
            && __instance.fc.fistCooldown <= 0f
            && MonoSingleton<WeaponCharges>.Instance is WeaponCharges Charges && Charges != null && Charges.punchStamina >= 1f
            && __instance.fc.activated
            && !GameStateManager.Instance.PlayerInputLocked)
        {
            __instance.heldAction = MonoSingleton<InputManager>.Instance.InputSource.Punch.Action;
            __instance.PunchStart();
        }
        __state = __instance.ready;
        __instance.ready = false; // blocks the game's own punch this frame; UpdateEnd restores it
    }
    // Restores the punch ready state after Update.
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Punch.Update))]
    private static void UpdateEnd(Punch __instance, bool __state)
    {
        __instance.ready = __state;
    }
    // Makes ActiveFrame use the arm hand position, the punch direction and the arm hand rotation in place of the camera, also at the cannonball, chainsaw and nail parries.
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Punch.ActiveFrame))]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
    {
        // this.cc.GetDefaultPos() -> Vars.NDHC.transform.position
        IEnumerable<CodeInstruction> sequence = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(i => i.opcode == OpCodes.Ldarg_0),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && i.operand is FieldInfo f && f.FieldType == typeof(CameraController)),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && i.operand is MethodInfo m && m == AccessTools.Method(typeof(CameraController), nameof(CameraController.GetDefaultPos))))
                .Repeat(matcher =>
                {
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Vars), "NDHC")) { labels = matcher.Labels });
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Component), nameof(Component.transform))) { labels = matcher.Labels });
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Transform), nameof(Transform.position))) { labels = matcher.Labels });
                }, Vars.NoMatch(original, "cc.GetDefaultPos()"))
                .InstructionEnumeration();
        // this.camObj.transform.forward -> Vars.punchDirection
        sequence = new CodeMatcher(sequence)
                .MatchForward(false,
                    new CodeMatch(i => i.opcode == OpCodes.Ldarg_0),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && i.operand is FieldInfo f && f.Name == "camObj"),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && i.operand is MethodInfo m && m == AccessTools.PropertyGetter(typeof(GameObject), nameof(GameObject.transform))),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && i.operand is MethodInfo m && m == AccessTools.PropertyGetter(typeof(Transform), nameof(Transform.forward))))
                .Repeat(matcher =>
                {
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(Vars), nameof(Vars.punchDirection))) { labels = matcher.Labels });
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop) { labels = matcher.Labels });
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop) { labels = matcher.Labels });
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop) { labels = matcher.Labels });
                }, Vars.NoMatch(original, "camObj.transform.forward"))
                .InstructionEnumeration();
        // this.camObj.transform.rotation -> Vars.NDHC.transform.rotation
        sequence = new CodeMatcher(sequence)
                .MatchForward(false,
                    new CodeMatch(i => i.opcode == OpCodes.Ldarg_0),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && i.operand is FieldInfo f && f.Name == "camObj"),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && i.operand is MethodInfo m && m == AccessTools.PropertyGetter(typeof(GameObject), nameof(GameObject.transform))),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && i.operand is MethodInfo m && m == AccessTools.PropertyGetter(typeof(Transform), nameof(Transform.rotation))))
                .Repeat(matcher =>
                {
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Vars), "NDHC")) { labels = matcher.Labels });
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Component), nameof(Component.transform))) { labels = matcher.Labels });
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Transform), nameof(Transform.rotation))) { labels = matcher.Labels });
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop) { labels = matcher.Labels });
                }, Vars.NoMatch(original, "camObj.transform.rotation"))
                .InstructionEnumeration();

        // the parry sites read this.cc.transform or CameraController.Instance instead of camObj (Punch.cs: cannonball, chainsaw, nail)
        MethodInfo GetInstance = AccessTools.PropertyGetter(typeof(CameraController), nameof(CameraController.Instance));
        MethodInfo GetTransform = AccessTools.PropertyGetter(typeof(Component), nameof(Component.transform));
        MethodInfo GetForward = AccessTools.PropertyGetter(typeof(Transform), nameof(Transform.forward));
        MethodInfo GetRotation = AccessTools.PropertyGetter(typeof(Transform), nameof(Transform.rotation));
        MethodInfo GetPosition = AccessTools.PropertyGetter(typeof(Transform), nameof(Transform.position));
        MethodInfo GetNDHC = AccessTools.PropertyGetter(typeof(Vars), "NDHC");
        FieldInfo PunchDirection = AccessTools.Field(typeof(Vars), nameof(Vars.punchDirection));
        CodeMatch LoadCC = new CodeMatch(i => i.opcode == OpCodes.Ldfld && i.operand is FieldInfo f && f.FieldType == typeof(CameraController));
        CodeMatch CallInstance = new CodeMatch(i => i.opcode == OpCodes.Call && i.operand is MethodInfo m && m == GetInstance);
        // this.cc.transform.forward -> Vars.punchDirection
        sequence = new CodeMatcher(sequence)
                .MatchForward(false, new CodeMatch(OpCodes.Ldarg_0), LoadCC, new CodeMatch(OpCodes.Callvirt, GetTransform), new CodeMatch(OpCodes.Callvirt, GetForward))
                .Repeat(matcher =>
                {
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldsfld, PunchDirection) { labels = matcher.Labels });
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop) { labels = matcher.Labels });
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop) { labels = matcher.Labels });
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop) { labels = matcher.Labels });
                }, Vars.NoMatch(original, "cc.transform.forward"))
                .InstructionEnumeration();
        // this.cc.transform.rotation -> Vars.NDHC.transform.rotation
        sequence = new CodeMatcher(sequence)
                .MatchForward(false, new CodeMatch(OpCodes.Ldarg_0), LoadCC, new CodeMatch(OpCodes.Callvirt, GetTransform), new CodeMatch(OpCodes.Callvirt, GetRotation))
                .Repeat(matcher =>
                {
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call, GetNDHC) { labels = matcher.Labels });
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Callvirt, GetTransform) { labels = matcher.Labels });
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Callvirt, GetRotation) { labels = matcher.Labels });
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop) { labels = matcher.Labels });
                }, Vars.NoMatch(original, "cc.transform.rotation"))
                .InstructionEnumeration();
        // CameraController.Instance.transform.position -> Vars.NDHC.transform.position
        sequence = new CodeMatcher(sequence)
                .MatchForward(false, CallInstance, new CodeMatch(OpCodes.Callvirt, GetTransform), new CodeMatch(OpCodes.Callvirt, GetPosition))
                .Repeat(matcher => matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call, GetNDHC) { labels = matcher.Labels }), Vars.NoMatch(original, "Instance.transform.position"))
                .InstructionEnumeration();
        // CameraController.Instance.transform.forward -> Vars.punchDirection
        sequence = new CodeMatcher(sequence)
                .MatchForward(false, CallInstance, new CodeMatch(OpCodes.Callvirt, GetTransform), new CodeMatch(OpCodes.Callvirt, GetForward))
                .Repeat(matcher =>
                {
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldsfld, PunchDirection) { labels = matcher.Labels });
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop) { labels = matcher.Labels });
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop) { labels = matcher.Labels });
                }, Vars.NoMatch(original, "Instance.transform.forward"))
                .InstructionEnumeration();
        // CameraController.Instance.GetDefaultPos() -> Vars.NDHC.transform.position
        sequence = new CodeMatcher(sequence)
                .MatchForward(false, CallInstance, new CodeMatch(i => i.opcode == OpCodes.Callvirt && i.operand is MethodInfo m && m == AccessTools.Method(typeof(CameraController), nameof(CameraController.GetDefaultPos))))
                .Repeat(matcher =>
                {
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call, GetNDHC) { labels = matcher.Labels });
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Callvirt, GetTransform) { labels = matcher.Labels });
                    matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Callvirt, GetPosition));
                }, Vars.NoMatch(original, "Instance.GetDefaultPos()"))
                .InstructionEnumeration();
        return sequence;
    }

    // Fires the Knuckleblaster blast wave from the arm hand while punch is held.
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Punch.BlastCheck))]
    private static bool BlastCheck(Punch __instance)
    {
        if (MonoSingleton<InputManager>.Instance.InputSource.Punch.IsPressed)
        {
            __instance.holdingInput = false;
            __instance.anim.SetTrigger("PunchBlast");
            Vector3 direction = Vars.NonDominantHand.transform.forward - Vars.NonDominantHand.transform.right;
            Vector3 position = Vars.NonDominantHand.transform.position + direction * 2f;
            if (Physics.Raycast(Vars.NonDominantHand.transform.position, direction, out var hitInfo,
                                2f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies))) position = hitInfo.point - direction * 0.1f;

            Object.Instantiate(__instance.blastWave, position, Vars.NonDominantHand.transform.rotation).GetComponentInChildren<Explosion>().playerProjectileForceDirection = direction.normalized;
        }
        return false;
    }
    // Returns the parry target along the arm hand, or toward the auto aim target.
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Punch.GetParryLookTarget))]
    private static bool GetParryLookTarget(ref Vector3 __result)
    {
        Vector3 vector = Vars.NonDominantHand.transform.forward;

        if ((bool)MonoSingleton<CameraFrustumTargeter>.Instance && (bool)MonoSingleton<CameraFrustumTargeter>.Instance.CurrentTarget && MonoSingleton<CameraFrustumTargeter>.Instance.IsAutoAimed)
        {
            vector = MonoSingleton<CameraFrustumTargeter>.Instance.CurrentTargetAimPosition - MonoSingleton<CameraController>.Instance.transform.position;
        }
        if (Physics.Raycast(Vars.NDHC.transform.position, vector, out var hitInfo, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.Enemies), QueryTriggerInteraction.Ignore))
            __result = hitInfo.point;
        else
            __result = Vars.NDHC.transform.position + vector * 1000f;
        return false;
    }
}
