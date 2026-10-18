using UnityEngine;

namespace Plugin.Systems.VRAvatar.Armature
{
    public class Hand
    {
        public ArmType Type { get; set; }
        public Transform Root { get; set; }
        public Finger FThumb { get; set; }
        public Finger FIndex { get; set; }
        public Finger FMiddle { get; set; }
        public Finger FRing { get; set; }
        public Finger FPinky { get; set; }

        // Maps the finger bones of a game Feedbacker hand.
        public static Hand FeedbackerPreset(Transform Base)
        {
            Hand H = new Hand
            {
                Type = ArmType.Feedbacker,
                Root = Base
            };
            H.FIndex = new Finger(H.Root.GetChild(0));
            H.FPinky = new Finger(H.Root.GetChild(1));
            H.FMiddle = new Finger(H.Root.GetChild(2));
            H.FRing = new Finger(H.Root.GetChild(3));
            H.FThumb = new Finger(H.Root.GetChild(0), true);
            return H;
        }
        // Maps the finger bones of a game Knuckleblaster hand.
        public static Hand KnuckleblasterPreset(Transform Base)
        {
            Hand H = new Hand
            {
                Type = ArmType.Knuckleblaster,
                Root = Base
            };
            H.FIndex = new Finger(H.Root.GetChild(4), true);
            H.FPinky = new Finger(H.Root.GetChild(5), true);
            H.FThumb = new Finger(H.Root.GetChild(6), true);
            return H;
        }
        // Maps the finger bones of an avatar Feedbacker hand.
        public static Hand MRFeedbackerPreset(Transform Base)
        {
            Hand H = new Hand
            {
                Type = ArmType.Feedbacker,
                Root = Base
            };
            H.FThumb = new Finger(H.Root.GetChild(0), true);
            H.FIndex = new Finger(H.Root.GetChild(1));
            H.FMiddle = new Finger(H.Root.GetChild(2));
            H.FRing = new Finger(H.Root.GetChild(3));
            H.FPinky = new Finger(H.Root.GetChild(4));
            return H;
        }
        // Maps the finger bones of an avatar Knuckleblaster hand.
        public static Hand MRKnuckleblasterPreset(Transform Base)
        {
            Hand H = new Hand
            {
                Type = ArmType.Knuckleblaster,
                Root = Base
            };
            H.FIndex = new Finger(H.Root.GetChild(0), true);
            H.FPinky = new Finger(H.Root.GetChild(1), true);
            H.FThumb = new Finger(H.Root.GetChild(2), true);
            return H;
        }
    }
}
