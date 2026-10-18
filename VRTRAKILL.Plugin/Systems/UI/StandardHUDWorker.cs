using UnityEngine;

namespace Plugin.Systems.UI
{
    internal class StandardHUDWorker : MonoBehaviour
    {
        public GameObject StandardHUD;

        // Shows the standard HUD only when the HUD type setting is 1.
        public void Update()
        {
            if (StandardHUD == null) return;

            if (MonoSingleton<PrefsManager>.Instance.GetInt("hudType") != 1) StandardHUD.SetActive(false);
            else StandardHUD.SetActive(true);
        }
    }
}
