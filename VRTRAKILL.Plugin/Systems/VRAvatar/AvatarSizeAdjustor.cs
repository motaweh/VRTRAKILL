using UnityEngine;

namespace Plugin.Systems.VRAvatar
{
    [RequireComponent(typeof(VRigController))]
    internal class AvatarSizeAdjustor : MonoBehaviour
    {
        // Returns the single instance; hand-rolled so this class can use OnEnable.
        private static AvatarSizeAdjustor _Instance; public static AvatarSizeAdjustor Instance { get { return _Instance; } }
        // Returns the avatar rig.
        public MetaRig Rig => VRigController.Instance.Rig;

        private readonly float ScalePercentage = .05f;

        // Keeps one instance and destroys the game object of any duplicate.
        public void Awake()
        {
            if (_Instance != null && _Instance != this) Destroy(this.gameObject);
            else _Instance = this;
        }

        // Shows a subtitle that avatar calibration is on.
        public void OnEnable()
        {
            SubtitleController.Instance.DisplaySubtitle("Avatar calibration is now enabled." +
                                                        "Press PrimaryFire to increase avatar's size," +
                                                        "press SecondaryFire to decrease.");
        }
        private bool IsHoldingKey = false;
        // Grows the player by one step on primary fire and shrinks it on secondary fire, once per press.
        public void Update()
        {
            if (InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame && !IsHoldingKey)
            { IsHoldingKey = true; ChangeSize(ScalePercentage, NewMovement.Instance.transform); }
            if (InputManager.Instance.InputSource.Fire2.WasPerformedThisFrame && !IsHoldingKey)
            { IsHoldingKey = true; ChangeSize(ScalePercentage * -1, NewMovement.Instance.transform); }
            if (InputManager.Instance.InputSource.Fire1.WasCanceledThisFrame
            || InputManager.Instance.InputSource.Fire2.WasCanceledThisFrame
            && IsHoldingKey) IsHoldingKey = false;
        }
        // Shows a subtitle that avatar calibration is off.
        public void OnDisable()
        {
            SubtitleController.Instance.DisplaySubtitle("Avatar calibration is now disabled.");
        }

        // Adds the scale step to each axis of the target scale.
        private void ChangeSize(float ScalePercentage, Transform Target)
        {
            Target.localScale = new Vector3(Target.localScale.x + ScalePercentage,
                                            Target.localScale.y + ScalePercentage,
                                            Target.localScale.z + ScalePercentage);
        }
    }
}
