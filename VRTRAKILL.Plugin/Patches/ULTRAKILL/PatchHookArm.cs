using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Plugin.Systems;
using Plugin.Systems.Arms;
using Plugin.Systems.VRAvatar.Armature;

namespace Plugin.Patches.ULTRAKILL;

[HarmonyPatch(typeof(HookArm))]
internal sealed class PatchHookArm
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(HookArm.Start))]
    static void Start(HookArm __instance)
    {
        Arm A = Arm.WhiplashPreset(__instance.transform);
        VRArmTransformer AR = __instance.gameObject.AddComponent<VRArmTransformer>();
        VRArmController AC = __instance.gameObject.AddComponent<VRArmController>();
        AR.Arm = A; AC.Arm = A;
    }

    // native Update stays (portal traversal, line renderer); only the throw origin moves from the arm to the VR hand
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(HookArm.Update))]
    static IEnumerable<CodeInstruction> Update(IEnumerable<CodeInstruction> instructions)
    {
        MethodInfo getTransform = AccessTools.PropertyGetter(typeof(Component), nameof(Component.transform));
        IEnumerable<CodeInstruction> codes = new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(i => i.opcode == OpCodes.Call && i.operand is MethodInfo m && m == getTransform))
            .Repeat(matcher => matcher
                .SetAndAdvance(OpCodes.Call, AccessTools.PropertyGetter(typeof(Vars), nameof(Vars.NonDominantHand)))
                .SetAndAdvance(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(GameObject), nameof(GameObject.transform))))
            .InstructionEnumeration();
        return SkipHandOffsets(codes);
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(HookArm.StopThrow))]
    static IEnumerable<CodeInstruction> StopThrow(IEnumerable<CodeInstruction> instructions)
        => SkipHandOffsets(instructions);

    // the FOV based hand.localPosition offsets fight the VR arm rig, so drop the assignments
    static IEnumerable<CodeInstruction> SkipHandOffsets(IEnumerable<CodeInstruction> instructions)
    {
        MethodInfo setLocalPosition = AccessTools.PropertySetter(typeof(Transform), nameof(Transform.localPosition));
        foreach (CodeInstruction instruction in instructions)
        {
            if (instruction.Calls(setLocalPosition))
            {
                instruction.opcode = OpCodes.Pop; instruction.operand = null;
                yield return instruction;
                yield return new CodeInstruction(OpCodes.Pop);
            }
            else yield return instruction;
        }
    }
}
