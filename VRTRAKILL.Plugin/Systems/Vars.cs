using UnityEngine;

namespace Plugin.Systems
{
    public enum Layers
    {
        Default = 0,
        TransparentFX = 1,
        IgnoreRaycast = 2,
        Water = 4,
        UI = 5,
        Environment = 8,
        Gib = 9, Limb = 10, BigCorpse = 11,
        EnemyTrigger = 12, AlwaysOnTop = 13,
        Projectile = 14,
        Invincible = 15, Invisible = 16,
        BrokenGlass = 17,
        PlayerOnly = 18,
        VirtualScreen = 19,
        GroundCheck = 20,
        EnemyWall = 21,
        Item = 22,
        Explosion = 23,
        Outdoors = 24,
        Armor = 26,
        GibLit = 27,
        VirtualRender = 28,
        SandboxGrabba = 29
    }
    public static class Vars
    {
        // Returns the loaded config.
        public static Prefs.NewConfig Config => Prefs.ConfigJSON.GetConfig().Config;
        // Returns the plugin logger.
        public static BepInEx.Logging.ManualLogSource Log => Plugin.Log;

        #region Checks
        // Returns true in the main menu.
        public static bool IsMainMenu
            => OptionsManager.Instance?.mainMenu ?? false;
        // Returns true when the game is paused; GetInstance avoids spending the one-shot Find that Instance does.
        public static bool IsPaused
            => MonoSingleton.GetInstance(typeof(OptionsManager)) is OptionsManager O && O != null && O.paused;
        // Returns true while the weapon wheel is open.
        public static bool IsWeaponWheelPresent
        => WeaponWheel.Instance?.isActiveAndEnabled ?? false;
        // Returns true while the player is in a shop.
        public static bool IsPlayerUsingShop
            => FistControl.Instance?.shopping ?? false;

        // Returns true while player movement or the camera is not active.
        public static bool IsPlayerFrozen
            => (!NewMovement.Instance?.activated ?? false) || (!NewMovement.Instance?.enabled ?? false)
            || (!CameraController.Instance?.activated ?? false);
        #endregion

        #region Cameras
        private static Camera _MainCamera;
        public static Vector3 punchDirection;

        // Returns the camera under "Main Camera", found once and then cached.
        public static Camera MainCamera
        {
            get
            {
                if (_MainCamera == null)
                {
                    _MainCamera = GameObject.Find("Main Camera")?.gameObject.GetComponentInChildren<Camera>();
                    return _MainCamera;
                }
                else return _MainCamera;
            }
        }
        // Returns the VR UI camera.
        public static Camera UICamera
            => UI.UIConverter.UICamera;
        // Returns the desktop world camera.
        public static Camera DesktopCamera
            => VRCamera.Patches.CameraConverterP.DesktopWorldCam;
        // Returns the desktop UI camera.
        public static Camera DesktopUICamera
            => VRCamera.Patches.CameraConverterP.DesktopUICam;
        #endregion

        #region Controllers
        // Returns the gun offset of the arms controller (non-dominant hand).
        public static GameObject NonDominantHand
            => Controllers.VRArmsSystem.Instance.GunOffset;
        // Returns the arms controller system.
        public static Controllers.VRArmsSystem NDHC
            => Controllers.VRArmsSystem.Instance;
        // Returns the gun offset of the guns controller (dominant hand).
        public static GameObject DominantHand
            => Controllers.VRGunsSystem.Instance.GunOffset;
        // Returns the guns controller system.
        public static Controllers.VRGunsSystem DHC
            => Controllers.VRGunsSystem.Instance;
        #endregion
    }
}
