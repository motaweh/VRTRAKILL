using UnityEngine;

namespace Plugin.Systems.VRAvatar.Armature
{
    public enum ArmType
    {
        Feedbacker,
        Knuckleblaster,
        Spear,
        Whiplash,
        Sandboxer
    }

    public class Arm
    {
        public ArmType Type { get; set; }
        public Transform GameObject { get; set; }
        public Transform Root { get; set; }
        public Transform Clavicle { get; set; }
        public Transform UpperArm { get; set; }
        public Transform Forearm { get; set; }

        public Hand Hand { get; set; }

        // Maps the bones of a game Feedbacker arm.
        public static Arm FeedbackerPreset(Transform GameObjectT)
        {
            Arm A = new Arm
            {
                Type = ArmType.Feedbacker,
                GameObject = GameObjectT,
                Root = GameObjectT.GetChild(0).GetChild(0)
            };
            A.Clavicle = A.Root;
            A.UpperArm = A.Clavicle.GetChild(0);
            A.Forearm = A.UpperArm.GetChild(0);

            A.Hand = Hand.FeedbackerPreset(A.Forearm.GetChild(0));
            return A;
        }
        // Maps the bones of a game Knuckleblaster arm.
        public static Arm KnuckleblasterPreset(Transform GameObjectT)
        {
            Arm A = new Arm
            {
                Type = ArmType.Knuckleblaster,
                GameObject = GameObjectT,
                Root = GameObjectT.GetChild(1)
            };
            A.Clavicle = A.Root.GetChild(1);
            A.UpperArm = A.Clavicle.GetChild(0);
            A.Forearm = A.Clavicle.GetChild(1);

            A.Hand = Hand.KnuckleblasterPreset(A.Forearm.GetChild(0));
            return A;
        }
        // Creates a spear arm with no bones.
        public static Arm SpearPreset(Transform T)
        {
            try
            {
                Arm A = new Arm
                {
                    Type = ArmType.Spear,
                    GameObject = T
                };
                return A;
            }
            catch (System.Exception E)
            {
                Vars.Log.LogError($"{E.Message}\n{E.Source}");
                return null;
            }
        }
        // Maps the bones of a game whiplash arm, or returns null on failure.
        public static Arm WhiplashPreset(Transform GameObjectT)
        {
            try
            {
                Arm A = new Arm
                {
                    Type = ArmType.Whiplash,
                    GameObject = GameObjectT,
                    Root = GameObjectT.GetChild(0).GetChild(1)
                };
                A.Clavicle = A.Root.GetChild(1);
                A.UpperArm = A.Clavicle.GetChild(0);
                A.Forearm = A.Clavicle.GetChild(1);

                A.Hand = Hand.FeedbackerPreset(A.Forearm.GetChild(0));
                return A;
            }
            catch (System.Exception E)
            {
                Vars.Log.LogError($"{E.Message}\n{E.Source}");
                return null;
            }
        }
        // Maps the bones of a game sandbox arm, or returns null on failure.
        public static Arm SandboxerPreset(Transform GameObjectT)
        {
            try
            {
                Arm A = new Arm
                {
                    Type = ArmType.Sandboxer,
                    GameObject = GameObjectT,
                    Root = GameObjectT.GetChild(0)
                };
                A.Clavicle = A.Root;
                A.UpperArm = A.Clavicle.GetChild(0);
                A.Forearm = A.UpperArm.GetChild(0);

                A.Hand = Hand.FeedbackerPreset(A.Forearm.GetChild(0));
                return A;
            }
            catch (System.Exception E)
            {
                Vars.Log.LogError($"{E.Message}\n{E.Source}");
                return null;
            }
        }

        // Maps the bones of the avatar Feedbacker arm, or returns null on failure.
        public static Arm MRFeedbackerPreset(Transform GameObjectT)
        {
            try
            {
                Arm A = new Arm
                {
                    Type = ArmType.Feedbacker,
                    GameObject = GameObjectT,
                    Root = GameObjectT.GetChild(1)
                };
                A.Clavicle = A.Root.GetChild(0);
                A.UpperArm = A.Clavicle.GetChild(0);
                A.Forearm = A.UpperArm.GetChild(0);
                A.Hand = Hand.MRFeedbackerPreset(A.Forearm.GetChild(0));
                return A;
            }
            catch (System.Exception E)
            {
                Vars.Log.LogError($"{E.Message}\n{E.Source}");
                return null;
            }
        }
        // Maps the bones of the avatar Knuckleblaster arm.
        public static Arm MRKnuckleblasterPreset(Transform GameObjectT)
        {
            Arm A = new Arm
            {
                Type = ArmType.Knuckleblaster,
                GameObject = GameObjectT,
                Root = GameObjectT.GetChild(1)
            };
            A.Clavicle = A.Root.GetChild(0);
            A.UpperArm = A.Clavicle.GetChild(0);
            A.Forearm = A.UpperArm.GetChild(0);
            A.Hand = Hand.MRKnuckleblasterPreset(A.Forearm.GetChild(0));
            return A;
        }
        // Maps the bones of the avatar whiplash arm.
        public static Arm MRWhiplashPreset(Transform GameObjectT)
        {
            Arm A = new Arm
            {
                Type = ArmType.Whiplash,
                GameObject = GameObjectT,
                Root = GameObjectT.GetChild(1)
            };
            A.Clavicle = A.Root.GetChild(0);
            A.UpperArm = A.Clavicle.GetChild(0);
            A.Forearm = A.UpperArm.GetChild(0);
            A.Hand = Hand.MRFeedbackerPreset(A.Forearm.GetChild(0));
            return A;
        }
        // Creates an avatar spear arm with no bones.
        public static Arm MRSpearPreset(Transform GameObjectT)
        {
            Arm A = new Arm
            {
                Type = ArmType.Spear,
                GameObject = GameObjectT
            };
            return A;
        }
        // Maps the bones of the avatar sandbox arm.
        public static Arm MRSandboxerPreset(Transform GameObjectT)
        {
            Arm A = new Arm
            {
                Type = ArmType.Sandboxer,
                GameObject = GameObjectT,
                Root = GameObjectT.GetChild(1)
            };
            A.Clavicle = A.Root.GetChild(0);
            A.UpperArm = A.Clavicle.GetChild(0);
            A.Forearm = A.UpperArm.GetChild(0);
            A.Hand = Hand.MRFeedbackerPreset(A.Forearm.GetChild(0));
            return A;
        }
    }
}
