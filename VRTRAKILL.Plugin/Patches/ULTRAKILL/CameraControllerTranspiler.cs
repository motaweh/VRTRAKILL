using HarmonyLib;
using Plugin.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Plugin.Patches.ULTRAKILL;

[HarmonyPatch]
internal class CameraControllerTranspiler
{
    static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Chainsaw), nameof(Chainsaw.Update));

        yield return AccessTools.Method(typeof(Nailgun), nameof(Nailgun.Shoot));
        yield return AccessTools.Method(typeof(Nailgun), nameof(Nailgun.ShootMagnet));
        yield return AccessTools.Method(typeof(Nailgun), nameof(Nailgun.SuperSaw));
        yield return AccessTools.Method(typeof(Nailgun), nameof(Nailgun.ShootZapper));

        yield return AccessTools.Method(typeof(Railcannon), nameof(Railcannon.Shoot));

        yield return AccessTools.Method(typeof(RocketLauncher), nameof(RocketLauncher.Shoot));
        yield return AccessTools.Method(typeof(RocketLauncher), nameof(RocketLauncher.ShootCannonball));
        yield return AccessTools.Method(typeof(RocketLauncher), nameof(RocketLauncher.ShootNapalm));

        yield return AccessTools.Method(typeof(Shotgun), nameof(Shotgun.Shoot));
        yield return AccessTools.Method(typeof(Shotgun), nameof(Shotgun.Update));
        yield return AccessTools.Method(typeof(Shotgun), nameof(Shotgun.ShootSinks));
        yield return AccessTools.Method(typeof(Shotgun), nameof(Shotgun.ShootSaw), new[] { typeof(bool) }); // ShootSaw() only wraps ShootSaw(bool)

        yield return AccessTools.Method(typeof(ShotgunHammer), nameof(ShotgunHammer.ImpactRoutine));
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
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // this.cc, CameraController.Instance, Camera.main or this.cam/camObj, then .transform -> the dominant hand's transform
        CodeMatch matchGettingCameraController = new CodeMatch(i =>
            (i.opcode == OpCodes.Ldfld && i.operand is FieldInfo f && f.FieldType == typeof(CameraController))
         || (i.opcode == OpCodes.Call && i.operand is MethodInfo f2 && f2 == AccessTools.PropertyGetter(typeof(CameraController), nameof(CameraController.Instance)))
         || (i.opcode == OpCodes.Call && i.operand is MethodInfo f3 && f3 == AccessTools.PropertyGetter(typeof(Camera), nameof(Camera.main)))
         || (i.opcode == OpCodes.Ldfld && i.operand is FieldInfo f4 && (f4.Name.ToLower() == "cam" || f4.Name.ToLower() == "camobj")));
        IEnumerable<CodeInstruction> sequence = new CodeMatcher(instructions)
                .MatchForward(false,
                    matchGettingCameraController,
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && i.operand is MethodInfo m && m.Name == "get_transform"))
                .Repeat(matcher =>
                {
                    if (matcher.Instruction.opcode == OpCodes.Ldfld) // the replaced ldfld leaves 'this' on the stack
                        matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Pop));

                    CodeInstruction newInstruction = new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Vars), "DominantHand"));
                    newInstruction.labels.AddRange(matcher.Instruction.labels);
                    matcher.SetInstructionAndAdvance(newInstruction);
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(GameObject), "get_transform")));
                })
                .InstructionEnumeration();
        return sequence;
    }
}
