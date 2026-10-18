using UnityEngine;

namespace Plugin.Systems.VRAvatar.Armature
{
    public class Finger
    {
        public Transform Root { get; set; }
        // Returns the middle bone of the finger.
        public Transform Bridge => Root.GetChild(0);
        public Transform Tip { get; }

        // Maps a finger from its root bone; the thumb tip is the bone after the root.
        public Finger(Transform RooT, bool IsThumb = false)
        {
            Root = RooT;
            if (IsThumb) Tip = Root.GetChild(0);
            else Tip = Bridge.GetChild(0);
        }
    }
}
