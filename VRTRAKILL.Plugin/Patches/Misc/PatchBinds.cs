using HarmonyLib;
using Plugin.Systems;

namespace Plugin.Patches.Misc;

[HarmonyPatch]
internal sealed class PatchBinds
{
    // Reloads the simulated keys after the game saves Binds.json, so a rebind works without a scene change.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(InputManager), nameof(InputManager.SaveBindings))]
    static void ReloadBinds()
    {
        try
        {
            Vars.Config.UKBinds = Prefs.UKBindings.GetBinds();
            Prefs.ConfigMaster.Init();
        }
        catch (System.Exception E) { Vars.Log.LogError("Unable to reload Binds.json: " + E); }
    }
}
