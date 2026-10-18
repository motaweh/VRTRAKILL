using HarmonyLib;
using Plugin.Systems;
using UnityEngine;

namespace Plugin.Patches.ULTRAKILL;

[HarmonyPatch(typeof(Revolver))]
internal class PatchRevolver
{
    // Replaces ThrowCoin to throw coins from the arm hand, the gun hand or the camera, based on the aim settings.
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Revolver.ThrowCoin))]
    private static bool ThrowCoin(Revolver __instance)
    {
        if (Vars.Config.EnableMBP) __instance.camObj = Vars.NonDominantHand;
        else if (Vars.Config.EnableCBA) __instance.camObj = Vars.DominantHand;
        else __instance.camObj = Camera.main.gameObject;

        // rest of the game's ThrowCoin (build 22957324), spawning from the camObj chosen above
        if (__instance.punch == null || !__instance.punch.gameObject.activeInHierarchy)
            __instance.punch = MonoSingleton<FistControl>.Instance.currentPunch;
        if (__instance.punch)
            __instance.punch.CoinFlip();
        MonoSingleton<PlayerAnimations>.Instance?.CoinToss();
        // the game drops the coin 0.5 below the eye; from a hand it spawns at the hand, as before
        Vector3 Drop = Vars.Config.EnableMBP || Vars.Config.EnableCBA ? Vector3.zero : __instance.camObj.transform.up * -0.5f;
        Coin component = Object.Instantiate(__instance.coin, __instance.camObj.transform.position + Drop, __instance.camObj.transform.rotation).GetComponent<Coin>();
        component.sourceWeapon = __instance.gc.currentWeapon;
        // the coin falls along the player's custom gravity (inverted gravity areas)
        Vector3 vector = Vector3.down;
        if (__instance.nmov.rb.TryGetCustomGravity(out Vector3 gravity) && __instance.nmov.rb.GetCustomGravityMode())
            vector = gravity;
        component.rb.SetCustomGravity(vector.normalized * Physics.gravity.magnitude);
        component.rb.SetCustomGravityMode(useCustomGravity: true);
        MonoSingleton<RumbleManager>.Instance.SetVibration(RumbleProperties.CoinToss);
        component.rb.AddForce(__instance.camObj.transform.forward * 20f + -vector.normalized * 15f + MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity(trueVelocity: true), ForceMode.VelocityChange);
        __instance.pierceCharge = 0f;
        __instance.pierceReady = false;

        return false;
    }
    // Moves the revolver muzzle to the VR hand before firing.
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Revolver.Shoot))]
    private static void Shoot(Revolver __instance)
    {
        __instance.gunBarrel = Vars.DominantHand; // fire from the VR hand
    }
}
