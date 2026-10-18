using UnityEngine;
using Valve.VR;
using WindowsInput;
using WindowsInput.Native;
using Plugin.Prefs;
namespace Plugin.Systems.Input
{
    internal static class SVRActionsManager
    {
        // Returns a new Windows input simulator for keyboard and mouse events.
        private static InputSimulator InpSim => new InputSimulator();

        public static bool
            Jump = false,
            Dash = false,
            Slide = false;

        public static bool
            RHPrimaryFire = false,
            RHAltFire = false,
            ChangeVariation = false,
            OpenWeaponWheel = false,
            NextWeapon = false,
            PrevWeapon = false;

        public static bool
            Punch = false,
            SwapHand = false,
            Whiplash = false;

        public static bool
            Slot0 = false,
            Slot1 = false, Slot2 = false, Slot3 = false,
            Slot4 = false, Slot5 = false, Slot6 = false,
            Slot7 = false, Slot8 = false, Slot9 = false;

        public static bool Escape = false;

        // Returns the input origin name of the action, with Left or Right for the active device.
        public static string FriendlyBindingName(this ISteamVR_Action_In SVRAI)
        {
            string Direction;
            switch (SVRAI.activeDevice)
            {
                case SteamVR_Input_Sources.LeftHand:
                case SteamVR_Input_Sources.LeftFoot:
                case SteamVR_Input_Sources.LeftShoulder:
                    Direction = "Left"; break;
                case SteamVR_Input_Sources.RightHand:
                case SteamVR_Input_Sources.RightFoot:
                case SteamVR_Input_Sources.RightShoulder:
                    Direction = "Right"; break;
                default: Direction = string.Empty; break;
            }
            // GetLocalizedOriginPart's params array is emitted as a SteamVR-scoped EVRInputStringBits typeref that fails to load
            if (string.IsNullOrEmpty(Direction))
                return SVRAI.GetLocalizedOrigin(SteamVR_Input_Sources.Any);
            else return $"{Direction} {SVRAI.GetLocalizedOrigin(SteamVR_Input_Sources.Any)}";
        }

        // Presses or releases the given key and the given mouse button.
        private static void TriggerKey(bool Started, bool Ended, VirtualKeyCode? KeyCode = null, MouseButton? Button = null)
        {
            if (KeyCode != null) TriggerKey((VirtualKeyCode)KeyCode, Started, Ended);
            if (Button != null) TriggerKey((MouseButton)Button, Started, Ended);
        }
        // Presses the key when the action starts and releases it when the action ends.
        private static void TriggerKey(VirtualKeyCode KeyCode, bool Started, bool Ended)
        {
            if (Started) InpSim.Keyboard.KeyDown(KeyCode);
            else if (Ended) InpSim.Keyboard.KeyUp(KeyCode);
        }
        // Presses or releases a mouse button; 3 is XBUTTON1, 4 is XBUTTON2 and 10 scrolls up with no release.
        private static void TriggerKey(MouseButton Button, bool Started, bool Ended)
        {
            if (Started)
                switch (Button)
                {
                    case MouseButton.LeftButton:
                        InpSim.Mouse.LeftButtonDown(); break;
                    case MouseButton.RightButton:
                        InpSim.Mouse.RightButtonDown(); break;
                    case MouseButton.MiddleButton:
                        InpSim.Mouse.MiddleButtonDown(); break;
                    case (MouseButton)3:
                        InpSim.Mouse.XButtonDown(1); break;
                    case (MouseButton)4:
                        InpSim.Mouse.XButtonDown(2); break;
                    case (MouseButton)10:
                        MouseScroll(1); break;
                }
            else if (Ended)
                switch (Button)
                {
                    case MouseButton.LeftButton:
                        InpSim.Mouse.LeftButtonUp(); break;
                    case MouseButton.RightButton:
                        InpSim.Mouse.RightButtonUp(); break;
                    case MouseButton.MiddleButton:
                        InpSim.Mouse.MiddleButtonUp(); break;
                    case (MouseButton)3:
                        InpSim.Mouse.XButtonUp(1); break;
                    case (MouseButton)4:
                        InpSim.Mouse.XButtonUp(2); break;
                }
        }
        // Scrolls the mouse wheel by the given amount.
        private static void MouseScroll(int Amount)
        {
            InpSim.Mouse.VerticalScroll(Amount);
        }

        // Adds the SteamVR action listeners that drive movement, guns, arms and key presses.
        public static void Init()
        {
            // the first per-hand read (UIInteraction, the pose trackers) adds a source and updates it at once, mid-frame,
            // which logs NoData; added here, in SteamVR_Input's startup frame, their first update waits for the input update
            SteamVR_Actions._default.Shoot.ForceAddSourceToUpdateList(SteamVR_Input_Sources.LeftHand);
            SteamVR_Actions._default.Shoot.ForceAddSourceToUpdateList(SteamVR_Input_Sources.RightHand);
            SteamVR_Actions._default.Punch.ForceAddSourceToUpdateList(SteamVR_Input_Sources.LeftHand);
            SteamVR_Actions._default.Punch.ForceAddSourceToUpdateList(SteamVR_Input_Sources.RightHand);
            SteamVR_Actions._default.LeftPose.ForceAddSourceToUpdateList(SteamVR_Input_Sources.LeftHand);
            SteamVR_Actions._default.RightPose.ForceAddSourceToUpdateList(SteamVR_Input_Sources.RightHand);

            // Movement
            Listen(SteamVR_Actions._default.Movement, MovementH);
            Listen(SteamVR_Actions._default.Turn, TurnH);
            Listen(SteamVR_Actions._default.Jump, JumpH);
            Listen(SteamVR_Actions._default.Slide, SlideH);
            Listen(SteamVR_Actions._default.Dash, DashH);

            // Arms
            Listen(SteamVR_Actions._default.Punch, PunchH);
            Listen(SteamVR_Actions._default.SwapHand, SwapHandH);
            Listen(SteamVR_Actions._default.Whiplash, WhiplashH);

            // Guns
            Listen(SteamVR_Actions._default.Shoot, RHShootH);
            Listen(SteamVR_Actions._default.AltShoot, RHAltShootH);
            Listen(SteamVR_Actions._default.IterateWeapon, IterateWeaponH);
            Listen(SteamVR_Actions._default.ChangeWeaponVariation, ChangeWeaponVariationH);
            Listen(SteamVR_Actions._default.NextWeapon, NextWeaponH);
            Listen(SteamVR_Actions._default.PrevWeapon, PrevWeaponH);

            // Weapon quick switch, open weapon wheel
            Listen(SteamVR_Actions._default.OpenWeaponWheel, OpenWeaponWheelH);
            Listen(SteamVR_Actions._default.WeaponWheelScroll, WeaponWheelScrollH);

            // Slots
            Listen(SteamVR_Actions._default.Slot0, Slot0H);
            Listen(SteamVR_Actions._default.Slot1, Slot1H);
            Listen(SteamVR_Actions._default.Slot2, Slot2H);
            Listen(SteamVR_Actions._default.Slot3, Slot3H);
            Listen(SteamVR_Actions._default.Slot4, Slot4H);
            Listen(SteamVR_Actions._default.Slot5, Slot5H);
            Listen(SteamVR_Actions._default.Slot6, Slot6H);
            Listen(SteamVR_Actions._default.Slot7, Slot7H);
            Listen(SteamVR_Actions._default.Slot8, Slot8H);
            Listen(SteamVR_Actions._default.Slot9, Slot9H);

            // Go back, pause, etc.
            Listen(SteamVR_Actions._default.Escape, EscapeH);
        }

        // Adds the update listener, and releases the bind when the action goes inactive (SteamVR dashboard, lost controller), since onUpdate then stops.
        private static void Listen(SteamVR_Action_Boolean Action, SteamVR_Action_Boolean.UpdateHandler Handler)
        {
            Action.AddOnUpdateListener(Handler, SteamVR_Input_Sources.Any);
            Action.AddOnActiveChangeListener((FromAction, FromSource, Active) => { if (!Active) Handler(FromAction, FromSource, false); }, SteamVR_Input_Sources.Any);
        }
        // Adds the update listener, and centers the stick when the action goes inactive, since onUpdate then stops.
        private static void Listen(SteamVR_Action_Vector2 Action, SteamVR_Action_Vector2.UpdateHandler Handler)
        {
            Action.AddOnUpdateListener(Handler, SteamVR_Input_Sources.Any);
            Action.AddOnActiveChangeListener((FromAction, FromSource, Active) => { if (!Active) Handler(FromAction, FromSource, Vector2.zero, Vector2.zero); }, SteamVR_Input_Sources.Any);
        }

        // Stores the movement stick axis.
        private static void MovementH(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
        { InputVars.MoveVector = axis; }
        // Stores the turn stick axis.
        private static void TurnH(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
        { InputVars.TurnVector = axis; }
        // Presses or releases the jump bind when the jump action changes.
        private static void JumpH(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
        { if (newState != Jump) { Jump = newState; TriggerKey(Jump, !Jump, ConfigMaster.KJump, ConfigMaster.MJump); } }
        // Presses or releases the slide bind when the slide action changes.
        private static void SlideH(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
        { if (newState != Slide) { Slide = newState; TriggerKey(Slide, !Slide, ConfigMaster.KSlide, ConfigMaster.MSlide); } }
        // Presses or releases the dash bind when the dash action changes.
        private static void DashH(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
        { if (newState != Dash) { Dash = newState; TriggerKey(Dash, !Dash, ConfigMaster.KDash, ConfigMaster.MDash); } }

        private static VirtualKeyCode? PunchKey; private static MouseButton? PunchButton;
        // Presses the punch bind, or the shoot bind while the player is frozen or in a shop, and releases the bind it pressed.
        private static void PunchH(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
        {
            if (newState != Punch)
            {
                Punch = newState;
                // the frozen or shop state can change while held; releasing the other bind left this one down
                if (Punch)
                {
                    bool Shoot = Vars.IsPlayerFrozen || Vars.IsPlayerUsingShop;
                    PunchKey = Shoot ? ConfigMaster.KShoot : ConfigMaster.KPunch;
                    PunchButton = Shoot ? ConfigMaster.MShoot : ConfigMaster.MPunch;
                }
                TriggerKey(Punch, !Punch, PunchKey, PunchButton);
            }
        }
        // Presses or releases the change fist bind when the swap hand action changes.
        private static void SwapHandH(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
        { if (newState != SwapHand) { SwapHand = newState; TriggerKey(SwapHand, !SwapHand, ConfigMaster.KSwapHand, ConfigMaster.MSwapHand); } }
        // Presses or releases the whiplash bind when the whiplash action changes.
        private static void WhiplashH(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
        { if (newState != Whiplash) { Whiplash = newState; TriggerKey(Whiplash, !Whiplash, ConfigMaster.KWhiplash, ConfigMaster.MWhiplash); } }

        // Presses or releases the shoot bind when the shoot action changes.
        private static void RHShootH(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
        { if (newState != RHPrimaryFire) { RHPrimaryFire = newState; TriggerKey(RHPrimaryFire, !RHPrimaryFire, ConfigMaster.KShoot, ConfigMaster.MShoot); } }
        // Presses or releases the alt fire bind when the alt shoot action changes.
        private static void RHAltShootH(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
        { if (newState != RHAltFire) { RHAltFire = newState; TriggerKey(RHAltFire, !RHAltFire, ConfigMaster.KAltShoot, ConfigMaster.MAltShoot); } }

        private static bool IterateLatched;
        // Scrolls once per up or down flick in play and every frame while paused, unless the weapon wheel is open.
        private static void IterateWeaponH(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
        {
            if (Vars.IsWeaponWheelPresent) return;
            float Threshold = Vars.Config.Controllers.Deadzone * 1.5f;
            if (Vars.IsPaused)
            {
                // menus scroll naturally: stick up moves the list up
                if (axis.y > Threshold) MouseScroll(1);
                if (axis.y < -Threshold) MouseScroll(-1);
                return;
            }
            // onUpdate fires every frame and the game takes a scroll every 0.1 s, so latch until the stick returns
            if (IterateLatched)
            {
                if (Mathf.Abs(axis.y) < Vars.Config.Controllers.Deadzone) IterateLatched = false;
                return;
            }
            // left and right turn (VRControllerLocations), so only a mostly vertical push switches weapons
            if (Mathf.Abs(axis.y) > Threshold && Mathf.Abs(axis.y) > Mathf.Abs(axis.x))
            {
                IterateLatched = true;
                MouseScroll(axis.y > 0 ? -1 : 1);
            }
        }
        // Presses or releases the change variation bind when the action changes.
        private static void ChangeWeaponVariationH(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
        { if (newState != ChangeVariation) { ChangeVariation = newState; TriggerKey(ChangeVariation, !ChangeVariation, ConfigMaster.KChangeVariation, ConfigMaster.MChangeVariation); } }
        // Scrolls the mouse wheel up once when the next weapon action is pressed.
        private static void NextWeaponH(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
        { if (newState != NextWeapon) { NextWeapon = newState; if (NextWeapon) MouseScroll(1); } }
        // Scrolls the mouse wheel down once when the previous weapon action is pressed.
        private static void PrevWeaponH(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
        { if (newState != PrevWeapon) { PrevWeapon = newState; if (PrevWeapon) MouseScroll(-1); } }

        // Presses or releases the last weapon bind, which opens the weapon wheel while held.
        private static void OpenWeaponWheelH(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
        { if (newState != OpenWeaponWheel) { OpenWeaponWheel = newState; TriggerKey(OpenWeaponWheel, !OpenWeaponWheel, ConfigMaster.KLastWeapon, ConfigMaster.MLastWeapon); } }
        // Stores the weapon wheel stick axis.
        private static void WeaponWheelScrollH(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
        { InputVars.WWVector = axis; }

        // Presses or releases the slot 0 bind when the slot 0 action changes.
        private static void Slot0H(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
        { if (newState != Slot0) { Slot0 = newState; TriggerKey(Slot0, !Slot0, ConfigMaster.KSlot0, ConfigMaster.MSlot0); } }
        // Presses or releases the slot 1 bind when the slot 1 action changes.
        private static void Slot1H(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
        { if (newState != Slot1) { Slot1 = newState; TriggerKey(Slot1, !Slot1, ConfigMaster.KSlot1, ConfigMaster.MSlot1); } }
        // Presses or releases the slot 2 bind when the slot 2 action changes.
        private static void Slot2H(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
        { if (newState != Slot2) { Slot2 = newState; TriggerKey(Slot2, !Slot2, ConfigMaster.KSlot2, ConfigMaster.MSlot2); } }
        // Presses or releases the slot 3 bind when the slot 3 action changes.
        private static void Slot3H(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
        { if (newState != Slot3) { Slot3 = newState; TriggerKey(Slot3, !Slot3, ConfigMaster.KSlot3, ConfigMaster.MSlot3); } }
        // Presses or releases the slot 4 bind when the slot 4 action changes.
        private static void Slot4H(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
        { if (newState != Slot4) { Slot4 = newState; TriggerKey(Slot4, !Slot4, ConfigMaster.KSlot4, ConfigMaster.MSlot4); } }
        // Presses or releases the slot 5 bind when the slot 5 action changes.
        private static void Slot5H(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
        { if (newState != Slot5) { Slot5 = newState; TriggerKey(Slot5, !Slot5, ConfigMaster.KSlot5, ConfigMaster.MSlot5); } }
        // Presses or releases the slot 6 bind when the slot 6 action changes.
        private static void Slot6H(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
        { if (newState != Slot6) { Slot6 = newState; TriggerKey(Slot6, !Slot6, ConfigMaster.KSlot6, ConfigMaster.MSlot6); } }
        // Presses or releases the slot 7 bind when the slot 7 action changes.
        private static void Slot7H(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
        { if (newState != Slot7) { Slot7 = newState; TriggerKey(Slot7, !Slot7, ConfigMaster.KSlot7, ConfigMaster.MSlot7); } }
        // Presses or releases the slot 8 bind when the slot 8 action changes.
        private static void Slot8H(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
        { if (newState != Slot8) { Slot8 = newState; TriggerKey(Slot8, !Slot8, ConfigMaster.KSlot8, ConfigMaster.MSlot8); } }
        // Presses or releases the slot 9 bind when the slot 9 action changes.
        private static void Slot9H(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
        { if (newState != Slot9) { Slot9 = newState; TriggerKey(Slot9, !Slot9, ConfigMaster.KSlot9, ConfigMaster.MSlot9); } }

        // Presses or releases Escape when the escape action changes.
        private static void EscapeH(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
        { if (newState != Escape) { Escape = newState; TriggerKey(Escape, !Escape, VirtualKeyCode.ESCAPE, null); } }
    }
}
