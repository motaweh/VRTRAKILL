using HarmonyLib;
using Plugin.Systems;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Plugin.Patches.ULTRAKILL;

[HarmonyPatch]
internal class CameraControllerTranspiler
{
    // Lists the weapon methods whose camera transform reads go to the gun hand.
    static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Chainsaw), nameof(Chainsaw.Update));

        yield return AccessTools.Method(typeof(Nailgun), nameof(Nailgun.Shoot));
        yield return AccessTools.Method(typeof(Nailgun), nameof(Nailgun.ShootMagnet));
        yield return AccessTools.Method(typeof(Nailgun), nameof(Nailgun.SuperSaw));

        yield return AccessTools.Method(typeof(Railcannon), nameof(Railcannon.Shoot));

        yield return AccessTools.Method(typeof(RocketLauncher), nameof(RocketLauncher.Shoot));
        yield return AccessTools.Method(typeof(RocketLauncher), nameof(RocketLauncher.ShootCannonball));
        yield return AccessTools.Method(typeof(RocketLauncher), nameof(RocketLauncher.ShootNapalm));

        yield return AccessTools.Method(typeof(Shotgun), nameof(Shotgun.Shoot));
        yield return AccessTools.Method(typeof(Shotgun), nameof(Shotgun.Update));
        yield return AccessTools.Method(typeof(Shotgun), nameof(Shotgun.ShootSinks));
        yield return AccessTools.Method(typeof(Shotgun), nameof(Shotgun.ShootSaw), new[] { typeof(bool) }); // ShootSaw() only wraps ShootSaw(bool)

        yield return AccessTools.Method(typeof(ShotgunHammer), nameof(ShotgunHammer.ImpactRoutine));
        // ImpactRoutine is an iterator: its body, with the camera forward read, is the state machine's MoveNext
        yield return AccessTools.EnumeratorMoveNext(AccessTools.Method(typeof(ShotgunHammer), nameof(ShotgunHammer.ImpactRoutine)));
        yield return AccessTools.Method(typeof(ShotgunHammer), nameof(ShotgunHammer.HitNade));
        yield return AccessTools.Method(typeof(ShotgunHammer), nameof(ShotgunHammer.ImpactEffects));
        yield return AccessTools.Method(typeof(ShotgunHammer), nameof(ShotgunHammer.ThrowNade));
        yield return AccessTools.Method(typeof(ShotgunHammer), nameof(ShotgunHammer.ShootSaw), new[] { typeof(bool) }); // ShootSaw() only wraps ShootSaw(bool)
        yield return AccessTools.Method(typeof(ShotgunHammer), nameof(ShotgunHammer.Update));

        yield return AccessTools.Method(typeof(Vacuum), nameof(Vacuum.SuckObjects));
        yield return AccessTools.Method(typeof(Vacuum), nameof(Vacuum.UpdateStuckObject));

        yield return AccessTools.Method(typeof(Washer), nameof(Washer.Update));

        yield return AccessTools.Method(typeof(Revolver), nameof(Revolver.Shoot));
    }
    // Replaces camera transform reads in the target methods, direct or through a dup of the kept CameraController.Instance, with the dominant hand transform.
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
    {
        // this.cc, CameraController.Instance, Camera.main or this.cam/camObj, then .transform -> the dominant hand's transform
        CodeMatch matchGettingCameraController = new CodeMatch(i =>
            (i.opcode == OpCodes.Ldfld && i.operand is FieldInfo f && f.FieldType == typeof(CameraController))
         || (i.opcode == OpCodes.Call && i.operand is MethodInfo f2 && f2 == AccessTools.PropertyGetter(typeof(CameraController), nameof(CameraController.Instance)))
         || (i.opcode == OpCodes.Call && i.operand is MethodInfo f3 && f3 == AccessTools.PropertyGetter(typeof(Camera), nameof(Camera.main)))
         || (i.opcode == OpCodes.Ldfld && i.operand is FieldInfo f4 && (f4.Name.ToLower() == "cam" || f4.Name.ToLower() == "camobj")));
        CodeMatch matchGetTransform = new CodeMatch(i => (i.opcode == OpCodes.Callvirt || i.opcode == OpCodes.Call) && i.operand is MethodInfo m && m.Name == "get_transform");
        int found = 0;
        List<CodeInstruction> codes = new CodeMatcher(instructions)
                .MatchForward(false, matchGettingCameraController, new CodeMatch(i => i.opcode == OpCodes.Callvirt && i.operand is MethodInfo m && m.Name == "get_transform"))
                .Repeat(matcher =>
                {
                    found++;
                    if (matcher.Instruction.opcode == OpCodes.Ldfld) // the replaced ldfld leaves 'this' on the stack
                        matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Pop));

                    CodeInstruction newInstruction = new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Vars), "DominantHand"));
                    newInstruction.labels.AddRange(matcher.Instruction.labels);
                    matcher.SetInstructionAndAdvance(newInstruction);
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(GameObject), "get_transform")));
                })
                .InstructionEnumeration().ToList();
        MethodInfo getInstance = AccessTools.PropertyGetter(typeof(CameraController), nameof(CameraController.Instance));
        bool keepsInstance = codes.Where((c, k) => k + 1 < codes.Count && c.opcode == OpCodes.Call && c.operand is MethodInfo g && g == getInstance && codes[k + 1].opcode == OpCodes.Dup).Any();
        if (keepsInstance)
            codes = new CodeMatcher(codes)
                .MatchForward(false, new CodeMatch(OpCodes.Dup), matchGetTransform)
                .Repeat(matcher =>
                {
                    found++;
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Vars), "DominantHand")) { labels = matcher.Labels });
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(GameObject), "get_transform")));
                })
                .InstructionEnumeration().ToList();
        if (found == 0 && original?.Name != nameof(ShotgunHammer.ImpactRoutine)) // the iterator stub has none; its MoveNext does
            Vars.NoMatch(original, "camera transform")(null);
        return codes;
    }
}
