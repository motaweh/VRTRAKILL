using UnityEngine;

namespace Plugin.Systems.UI
{
    internal class HideWhenMenuActive : MonoBehaviour
    {
        // Returns true while the player is frozen.
        private bool ShouldHide => Vars.IsPlayerFrozen;

        private Canvas Canvas;
        // Hides the canvas while the player is frozen.
        public void Update()
        {
            if (Canvas == null) Canvas = gameObject.GetComponent<Canvas>();
            if (Canvas != null) Canvas.enabled = !ShouldHide;
        }
    }
}
