using HarmonyLib;
using Plugin.Systems.Input;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using ULTRAKILL.Cheats;
using UnityEngine;

namespace Plugin.Systems.Movement.Patches;
[HarmonyPatch]
internal class MovementTranspiler
{
    // Lists the movement methods that read the move input.
    static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(PlatformerMovement), nameof(PlatformerMovement.Update));
        yield return AccessTools.Method(typeof(ClimbStep), nameof(ClimbStep.FixedUpdate));
        yield return AccessTools.Method(typeof(Flight), nameof(Flight.Update));
        yield return AccessTools.Method(typeof(Noclip), nameof(Noclip.UpdateTick));
        yield return AccessTools.Method(typeof(Grenade), nameof(Grenade.LateUpdate));
        yield return AccessTools.Method(typeof(NewMovement), nameof(NewMovement.Update));
        yield return AccessTools.Method(typeof(NewMovement), nameof(NewMovement.Dodge));
    }
    // Replaces move input reads in the target methods with the VR movement stick.
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && i.operand is FieldInfo f && f == AccessTools.Field(typeof(PlayerInput), "Move")),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && i.operand is MethodInfo m && m.Name == "ReadValue"))
                .Repeat(matcher =>
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MovementTranspiler), nameof(GetReplacementMove))))
                    .RemoveInstruction())
                .InstructionEnumeration();
    }
    // Returns the VR movement stick axis in place of the move input, or zero while the weapon wheel uses the same stick.
    static Vector2 GetReplacementMove(PlayerInput input) => Vars.IsWeaponWheelPresent ? Vector2.zero : InputVars.MoveVector;
}
