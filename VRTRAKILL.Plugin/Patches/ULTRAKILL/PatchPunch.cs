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

        foreach (SkinnedMeshRenderer SMR in __instance.GetComponentsInChildren<SkinnedMeshRenderer>())
            SMR.updateWhenOffscreen = true;

        __instance.tr.enabled = false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Punch.Update))]
    private static void Update(Punch __instance, out bool __state)
    {
        __state = __instance.ready;
        if (MonoSingleton<OptionsManager>.Instance.paused) return;

        if (Vars.NDHC.Speed * 2 >= Vars.Config.MBP.PunchingSpeed
            && MonoSingleton<InputManager>.Instance.InputSource.Punch.IsPressed
            && __instance.ready && !__instance.shopping
            && __instance.fc.activated
            && !GameStateManager.Instance.PlayerInputLocked)
        {
            __instance.heldAction = MonoSingleton<InputManager>.Instance.InputSource.Punch.Action;
            __instance.PunchStart();
        }
        __state = __instance.ready;
        __instance.ready = false; // blocks the game's own punch this frame; UpdateEnd restores it
    }
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Punch.Update))]
    private static void UpdateEnd(Punch __instance, bool __state)
    {
        __instance.ready = __state;
    }
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Punch.ActiveFrame))]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
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
                })
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
                })
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
                })
                .InstructionEnumeration();
        return sequence;
    }

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

        __result = Vars.NDHC.transform.position + vector * 1000f;
        return false;
    }
}
