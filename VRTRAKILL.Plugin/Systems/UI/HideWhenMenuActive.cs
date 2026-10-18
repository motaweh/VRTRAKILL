using UnityEngine;

namespace Plugin.Systems.UI
{
    internal class HideWhenMenuActive : MonoBehaviour
    {
        // Returns true while the player is frozen.
        private bool ShouldHide => Vars.IsPlayerFrozen;

        // Hides the canvas while the player is frozen.
        public void Update()
        {
            if (ShouldHide) gameObject.GetComponent<Canvas>().enabled = false;
            else gameObject.GetComponent<Canvas>().enabled = true;
        }
    }
}
