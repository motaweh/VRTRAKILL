using UnityEngine;

namespace Plugin.Systems.UI
{
    // warns the player when the head is inside level geometry
    internal sealed class GTFOTW : MonoBehaviour
    {
        public Transform DetectorTransform;
        private CanvasGroup CG; private UnityEngine.UI.Text Text;
        private bool ShouldShow;

        private readonly string[] Texts = new string[]
        {
            "GET OUT OF THE WALL.",
            "GET THE OUT OF THE GODDAM WALL.",
            "DON'T MAKE ME REPEAT MYSELF."
        };
        private bool ChangeText = false;

        // Gets the canvas group and text and hides the warning.
        public void OnEnable()
        {
            CG = GetComponent<CanvasGroup>();
            CG.alpha = 0;
            Text = GetComponentInChildren<UnityEngine.UI.Text>();
        }

        // Fades the warning in while the head is inside level geometry and out after it leaves, with a new random text each time.
        public void Update()
        {
            // CheckSphere does not allocate the collider array that OverlapSphere did every frame
            ShouldShow = Physics.CheckSphere(DetectorTransform.position, .1f, 1 << (int)Layers.Environment, QueryTriggerInteraction.Ignore);

            if (ShouldShow)
            {
                if (ChangeText) Text.text = Texts[Random.Range(0, Texts.Length)];
                ChangeText = false;
                if (CG.alpha < 1)
                {
                    CG.alpha += Time.deltaTime;
                    if (CG.alpha >= 1) ShouldShow = false;
                }
            }
            else
            {
                if (CG.alpha > 0)
                {
                    CG.alpha -= Time.deltaTime;
                    if (CG.alpha <= 0) ChangeText = true;
                }
            }
        }
    }
}
