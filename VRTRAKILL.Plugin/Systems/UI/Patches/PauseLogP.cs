using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;
using Valve.VR;
using Plugin.Systems.Input;

namespace Plugin.Systems.UI.Patches
{
    // records what opened and closed the pause menu; the game's own logs are filtered out, so a headset test showed nothing
    [HarmonyPatch]
    internal class PauseLogP
    {
        // Logs the pause with its caller and the input state.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(OptionsManager), nameof(OptionsManager.Pause))]
        static void LogPause(OptionsManager __instance)
        {
            if (__instance.mainMenu) return;
            Vars.Log.LogMessage("Pause menu opened" + Describe());
        }
        // Logs the unpause with its caller and the input state.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(OptionsManager), nameof(OptionsManager.UnPause))]
        static void LogUnPause(OptionsManager __instance)
        {
            if (__instance.mainMenu || !__instance.paused) return;
            Vars.Log.LogMessage("Pause menu closed" + Describe());
        }
        // Returns the frame, the calling game methods and which pause, submit and click inputs are held.
        static string Describe()
        {
            // frame 0 is this method and frame 1 the patch; the Harmony wrapper frames carry the original method name
            System.Diagnostics.StackFrame[] Frames = new System.Diagnostics.StackTrace(2, false).GetFrames() ?? new System.Diagnostics.StackFrame[0];
            System.Text.StringBuilder Callers = new System.Text.StringBuilder();
            for (int i = 0, n = 0; i < Frames.Length && n < 4; i++)
            {
                System.Reflection.MethodBase M = Frames[i].GetMethod();
                if (M == null) continue;
                if (n++ > 0) Callers.Append(" < ");
                Callers.Append(M.DeclaringType != null ? M.DeclaringType.Name + "." + M.Name : M.Name);
            }
            return $" (frame {Time.frameCount}, t {Time.realtimeSinceStartup:F2}s) by {Callers};"
                + $" escape action {SVRActionsManager.Escape}, keyboard escape {Keyboard.current?.escapeKey.isPressed ?? false},"
                + $" gamepad start {Gamepad.current?.startButton.isPressed ?? false} ({Gamepad.current?.displayName ?? "no gamepad"}),"
                + $" variation action {SVRActionsManager.ChangeVariation}, shoot {SteamVR_Actions._default.Shoot.GetState(SteamVR_Input_Sources.Any)},"
                + $" punch {SteamVR_Actions._default.Punch.GetState(SteamVR_Input_Sources.Any)}, selected {SelectedName()}";
        }
        // Returns the name of the selected UI object, or none.
        static string SelectedName()
        {
            UnityEngine.EventSystems.EventSystem ES = UnityEngine.EventSystems.EventSystem.current;
            return ES != null && ES.currentSelectedGameObject != null ? ES.currentSelectedGameObject.name : "none";
        }
    }
}
