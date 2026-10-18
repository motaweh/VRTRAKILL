using UnityEngine;

namespace Plugin.Systems.VRPlayer
{
    public class VRKeybindsController : MonoSingleton<VRKeybindsController>
    {
        public static bool WasDVActive { get; set; }

        // Does nothing.
        public void Start() { }

        // Toggles the desktop view or the avatar size adjustment when their keys are pressed.
        public void Update()
        {
            if (UnityEngine.Input.GetKeyDown((KeyCode)Prefs.ConfigMaster.ToggleDesktopView))
            {
                SubtitleController.Instance.DisplaySubtitle("VR: Toggling desktop view");
                ToggleDesktopView();
            }
            if (UnityEngine.Input.GetKeyDown((KeyCode)Prefs.ConfigMaster.ToggleAvatarSizeAdj))
            {
                try
                {
                    if ((bool)(VRAvatar.AvatarSizeAdjustor.Instance?.enabled))
                        VRAvatar.AvatarSizeAdjustor.Instance.enabled = false;
                    else if ((bool)!VRAvatar.AvatarSizeAdjustor.Instance?.enabled)
                        VRAvatar.AvatarSizeAdjustor.Instance.enabled = true;
                }
                catch (System.NullReferenceException) { SubtitleController.Instance.DisplaySubtitle("Unable to toggle avatar size adjustment!"); }
            }
        }

        // Shows or hides the desktop world and UI cameras together and remembers the choice for later scenes.
        public void ToggleDesktopView()
        {
            if (Vars.DesktopCamera == null || Vars.DesktopUICamera == null) return;

            bool on = !Vars.DesktopCamera.gameObject.activeSelf;
            Vars.DesktopCamera.gameObject.SetActive(on);
            Vars.DesktopUICamera.gameObject.SetActive(on);
            VRCamera.Patches.CameraConverterP.DesktopViewOn = on;
        }
    }
}
