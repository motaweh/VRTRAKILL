using Valve.VR;
using System.Collections.Generic;

namespace Plugin.Systems.Input.ControlMessages
{
    internal class MessageContainer
    {
        // Returns the bound input name for punch.
        private static string Punch => SteamVR_Actions._default.Punch.FriendlyBindingName();
        // Returns the bound input name for slide.
        private static string Slide => SteamVR_Actions._default.Slide.FriendlyBindingName();
        // Returns the bound input name for jump.
        private static string Jump => SteamVR_Actions._default.Jump.FriendlyBindingName();
        // Returns the bound input name for dash.
        private static string Dash => SteamVR_Actions._default.Dash.FriendlyBindingName();
        // Returns the bound input name for alt shoot.
        private static string Grip => SteamVR_Actions._default.AltShoot.FriendlyBindingName();
        // Returns the bound input name for swap hand.
        private static string SwapArm => SteamVR_Actions._default.SwapHand.FriendlyBindingName();
        // Returns the bound input name for whiplash.
        private static string Whiplash => SteamVR_Actions._default.Whiplash.FriendlyBindingName();


        // Returns the VR punch tutorial text.
        private static string Tutorial_Punch
            => $"Hold '<color=orange>{Punch}</color>' and fling your arm to <color=orange>PUNCH</color>";
        // Returns the VR slide tutorial text.
        private static string Tutorial_Slide
            => $"Hold '<color=orange>{Slide}</color>' to <color=orange>SLIDE</color>.";
        // Returns the VR jump and dash tutorial text.
        private static string Tutorial_JumpDash
            => $"Press '<color=orange>{Jump}</color>' to <color=orange>JUMP</color>\r\n" +
               $"Press '<color=#00DFFF>{Dash}</color>' to <color=#00DFFF>DASH</color>. (Consumes <color=#00DFFF>STAMINA</color>)\r\n" +
               $"Can be performed in air.";
        // Returns the VR ground slam tutorial text.
        private static string Tutorial_Slam
            => $"Press '<color=orange>{Slide}</color>' in the air to <color=orange>GROUND SLAM</color>.\r\n" +
               $"Hold for <color=orange>SHOCKWAVE</color>. (Consumes <color=#00DFFF>STAMINA</color>)";
        // Returns the Prelude ground slam hint text.
        private static string Tutorial_PreludeSlam =>
            $"<color=orange>GROUND SLAM</color> deals damage on direct hit.";

        // Returns the VR revolver alt fire tutorial text.
        private static string Tutorial_Revolver
            => $"<color=cyan>REVOLVER</color>: Hold <color=orange>{Grip}</color> to charge a <color=orange>PIERCING</color> shot.";
        // Returns the VR shotgun alt fire tutorial text.
        private static string Tutorial_Shotgun
            => $"<color=cyan>SHOTGUN</color>: Press '<color=orange>{Grip}</color>' to fire an explosive. Hold to charge distance.";
        // Returns the VR nailgun alt fire tutorial text.
        private static string Tutorial_Nailgun
            => $"<color=cyan>NAILGUN</color>: Use <color=orange>{Grip}</color> to fire a <color=orange>NAIL MAGNET</color>. Can be attached to environment to create traps.";

        // Returns the VR arm swap tutorial text.
        private static string TutorialKnuckleblaster =>
            $"Cycle through <color=orange>EQUIPPED</color> arms with '<color=orange>{SwapArm}</color>'";
        // Returns the Lust parry reminder text.
        private static string Tutorial_Lust_ArmSwapReminder =>
            $"Only the <color=cyan>FEEDBACKER</color> (<color=cyan>Blue arm</color>) can <color=orange>PARRY PROJECTILES</color>.";
        // Returns the VR whiplash tutorial text.
        private static string Tutorial_Whiplash =>
            $"<color=lime>WHIPLASH</color>: Hold <color=orange>{Whiplash}</color> to throw, release to pull";

        // Returns the VR book close hint text.
        private static string Book_Close =>
            $"<color=orange>Punch</color> to CLOSE";

        public static readonly Dictionary<string, string> HintsToTexts = new Dictionary<string, string>
        {
            { "PUNCH", Tutorial_Punch },
            { "SLIDE", Tutorial_Slide },
            { "DASH", Tutorial_JumpDash },
            { "SHOCKWAVE", Tutorial_Slam },
            { "deals damage on direct hit.", Tutorial_PreludeSlam },

            { "REVOLVER", Tutorial_Revolver },
            { "SHOTGUN", Tutorial_Shotgun },
            { "NAILGUN", Tutorial_Nailgun },

            { "arms with", TutorialKnuckleblaster },
            { "Only the", Tutorial_Lust_ArmSwapReminder },
            { "to throw, release to pull", Tutorial_Whiplash },

            { "to CLOSE", Book_Close },
        };
    }
}
