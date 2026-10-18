using HarmonyLib;

namespace Plugin.Systems.Movement.Patches
{
    [HarmonyPatch(typeof(NewMovement))]
    internal sealed class NewMovementP
    {
        // Scales walk speed, jump power and wall jump power by the movement multiplier before the player starts.
        [HarmonyPrefix]
        [HarmonyPatch(nameof(NewMovement.Start))]
        static void Start(NewMovement __instance)
        {
            __instance.walkSpeed *= Vars.Config.MovementMultiplier;
            __instance.jumpPower *= Vars.Config.MovementMultiplier;
            __instance.wallJumpPower *= Vars.Config.MovementMultiplier;
        }
    }
}
