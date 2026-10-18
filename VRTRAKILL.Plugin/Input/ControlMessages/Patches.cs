using HarmonyLib;

namespace Plugin.Systems.Input.ControlMessages
{
    [HarmonyPatch]
    internal static class Patches
    {
        // Replaces a HUD hint with its VR control text before it plays.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HudMessage), nameof(HudMessage.PlayMessage))]
        static void PlayMessage(HudMessage __instance, bool hasToBeEnabled = false)
        {
            if (ConvertTextToVR(__instance.message, __instance.message2) == "Error"
            && (!string.IsNullOrEmpty(__instance.message) || !string.IsNullOrEmpty(__instance.message2))) return;
            else __instance.ChangeMessage(ConvertTextToVR(__instance.message, __instance.message2));
        }
        // Replaces bound key text with its VR control text when it is shown.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(TextBinds), nameof(TextBinds.OnEnable))]
        static void TBOnEnable(TextBinds __instance)
        {
            if (ConvertTextToVR(__instance.text1, __instance.text2) == "Error"
            && (!string.IsNullOrEmpty(__instance.text1) || !string.IsNullOrEmpty(__instance.text2))) return;
            else __instance.text.text = ConvertTextToVR(__instance.text1, __instance.text2);
        }
        // Returns the VR text for the first hint key found in the message, or "Error" if none match.
        public static string ConvertTextToVR(string Message, string Message2)
        {
            string FullMessage = Message + Message2;
            foreach (string Key in MessageContainer.HintsToTexts.Keys)
            {
                if (FullMessage.Contains(Key))
                    if (MessageContainer.HintsToTexts.TryGetValue(Key, out string Value) && Value != null)
                        return Value;
            }
            return "Error";
        }
    }
}
