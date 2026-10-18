using UnityEngine;

namespace Plugin.Systems.VRAvatar
{
    // FABRIK solver, see https://www.youtube.com/watch?v=UNoX65PRehA
    internal class IKChain : MonoBehaviour
    {
        // Number of bones in the chain.
        public int ChainLength = 2;

        // Target the chain bends toward.
        public Transform Target;
        public Transform Pole;

        // Solver iterations per update.
        [Header("Solver Parameters")]
        public int Iterations = 10;

        // Distance at which the solver stops.
        public float Delta = 0.001f;

        // Strength of the pull back to the start pose.
        [Range(0, 1)]
        public float SnapBackStrength = 1f;


        protected float[] BonesLength; // target to origin
        protected float CompleteLength;
        protected Transform[] Bones;
        protected Vector3[] Positions;
        protected Vector3[] StartDirectionSucc;
        protected Quaternion[] StartRotationBone;
        protected Quaternion StartRotationTarget;
        protected Transform Root;


        // Sets up the chain on start.
        public void Start() => Init();

        // Finds the chain bones and root, creates a target if none is set, and stores the bone lengths and start rotations.
        public void Init()
        {
            Bones = new Transform[ChainLength + 1];
            Positions = new Vector3[ChainLength + 1];
            BonesLength = new float[ChainLength];
            StartDirectionSucc = new Vector3[ChainLength + 1];
            StartRotationBone = new Quaternion[ChainLength + 1];

            Root = transform;
            for (var i = 0; i <= ChainLength; i++)
            {
                if (Root == null)
                    throw new UnityException("The chain value is longer than the ancestor chain!");
                Root = Root.parent;
            }

            if (Target == null)
            {
                Target = new GameObject(gameObject.name + " Target").transform;
                SetPositionRootSpace(Target, GetPositionRootSpace(transform));
            }
            StartRotationTarget = GetRotationRootSpace(Target);

            var current = transform;
            CompleteLength = 0;
            for (var i = Bones.Length - 1; i >= 0; i--)
            {
                Bones[i] = current;
                StartRotationBone[i] = GetRotationRootSpace(current);

                if (i == Bones.Length - 1)
                {
                    StartDirectionSucc[i] = GetPositionRootSpace(Target) - GetPositionRootSpace(current);
                }
                else
                {
                    StartDirectionSucc[i] = GetPositionRootSpace(Bones[i + 1]) - GetPositionRootSpace(current);
                    BonesLength[i] = StartDirectionSucc[i].magnitude;
                    CompleteLength += BonesLength[i];
                }

                current = current.parent;
            }
        }

        // Solves the chain each frame.
        public void LateUpdate()
        {
            ResolveIK();
        }

        // Moves the bones toward the target with FABRIK, bends them toward the pole and sets their rotations.
        private void ResolveIK()
        {
            if (Target == null)
                return;

            if (BonesLength.Length != ChainLength)
                Init();

            for (int i = 0; i < Bones.Length; i++)
                Positions[i] = GetPositionRootSpace(Bones[i]);

            var targetPosition = GetPositionRootSpace(Target);
            var targetRotation = GetRotationRootSpace(Target);

            // target out of reach: stretch the chain toward it
            if ((targetPosition - GetPositionRootSpace(Bones[0])).sqrMagnitude >= CompleteLength * CompleteLength)
            {
                var direction = (targetPosition - Positions[0]).normalized;
                for (int i = 1; i < Positions.Length; i++)
                    Positions[i] = Positions[i - 1] + direction * BonesLength[i - 1];
            }
            else
            {
                for (int i = 0; i < Positions.Length - 1; i++)
                    Positions[i + 1] = Vector3.Lerp(Positions[i + 1], Positions[i] + StartDirectionSucc[i], SnapBackStrength);

                for (int iteration = 0; iteration < Iterations; iteration++)
                {
                    // backward pass
                    for (int i = Positions.Length - 1; i > 0; i--)
                    {
                        if (i == Positions.Length - 1)
                            Positions[i] = targetPosition;
                        else
                            Positions[i] = Positions[i + 1] + (Positions[i] - Positions[i + 1]).normalized * BonesLength[i];
                    }

                    // forward pass
                    for (int i = 1; i < Positions.Length; i++)
                        Positions[i] = Positions[i - 1] + (Positions[i] - Positions[i - 1]).normalized * BonesLength[i - 1];

                    if ((Positions[Positions.Length - 1] - targetPosition).sqrMagnitude < Delta * Delta)
                        break;
                }
            }

            // bend toward the pole
            if (Pole != null)
            {
                var polePosition = GetPositionRootSpace(Pole);
                for (int i = 1; i < Positions.Length - 1; i++)
                {
                    var plane = new Plane(Positions[i + 1] - Positions[i - 1], Positions[i - 1]);
                    var projectedPole = plane.ClosestPointOnPlane(polePosition);
                    var projectedBone = plane.ClosestPointOnPlane(Positions[i]);
                    var angle = Vector3.SignedAngle(projectedBone - Positions[i - 1], projectedPole - Positions[i - 1], plane.normal);
                    Positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (Positions[i] - Positions[i - 1]) + Positions[i - 1];
                }
            }

            for (int i = 0; i < Positions.Length; i++)
            {
                if (i == Positions.Length - 1)
                    SetRotationRootSpace(Bones[i], Quaternion.Inverse(targetRotation) * StartRotationTarget * Quaternion.Inverse(StartRotationBone[i]));
                else
                    SetRotationRootSpace(Bones[i], Quaternion.FromToRotation(StartDirectionSucc[i], Positions[i + 1] - Positions[i]) * Quaternion.Inverse(StartRotationBone[i]));
                SetPositionRootSpace(Bones[i], Positions[i]);
            }
        }

        // Returns the position of a transform in root space.
        private Vector3 GetPositionRootSpace(Transform current)
        {
            if (Root == null)
                return current.position;
            else
                return Quaternion.Inverse(Root.rotation) * (current.position - Root.position);
        }

        // Sets the position of a transform from a root space position.
        private void SetPositionRootSpace(Transform current, Vector3 position)
        {
            if (Root == null)
                current.position = position;
            else
                current.position = Root.rotation * position + Root.position;
        }

        // Returns the rotation of a transform in root space.
        private Quaternion GetRotationRootSpace(Transform current)
        {
            // inverse(after) * before => rot: before -> after
            if (Root == null)
                return current.rotation;
            else
                return Quaternion.Inverse(current.rotation) * Root.rotation;
        }

        // Sets the rotation of a transform from a root space rotation.
        private void SetRotationRootSpace(Transform current, Quaternion rotation)
        {
            if (Root == null)
                current.rotation = rotation;
            else
                current.rotation = Root.rotation * rotation;
        }
    }
}
