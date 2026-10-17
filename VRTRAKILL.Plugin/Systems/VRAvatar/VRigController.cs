using Plugin.Systems.Controllers;
using VRTRAKILL.Utilities;
using Plugin.Util.Libraries.EZhex1991.EZSoftBone;
using UnityEngine;
using Valve.VR;

namespace Plugin.Systems.VRAvatar
{
    // the NewMovement of the player's avatar
    internal class VRigController : MonoBehaviour
    {
        // hand-rolled singleton so this class can use Awake
        private static VRigController _Instance; public static VRigController Instance { get { return _Instance; } }

        public MetaRig Rig; private EZSoftBone WingBone;
        public Vector3 HeadOffsetPosition = new Vector3(0, 0, 0),
                       HeadOffsetAngles = new Vector3(-90, 0, 0);
        public Animator Anim;

        private IKChain AddIK(GameObject GO, Transform Target, int ChainLen = 2, Transform Pole = null)
        {
            IKChain IK = GO.AddComponent<IKChain>();
            IK.Target = Target; IK.ChainLength = ChainLen; IK.Pole = Pole;
            return IK;
        }

        public void Awake()
        {
            if (_Instance != null && _Instance != this) Destroy(this.gameObject);
            else _Instance = this;
        }

        public void Start()
        {
            Rig = Rig ?? MetaRig.CreateVCustomPreset(null, "VR Avatar");
            VRTRAKILL.Utilities.Unity.RecursiveChangeLayer(Rig.GameObjectT.gameObject, (int)Layers.AlwaysOnTop);

            Rig.GameObjectT.localPosition = Vector3.zero;
            Rig.GameObjectT.localRotation = Quaternion.Euler(Vector3.zero);

            Rig.Root.localScale *= 3;
            Rig.Root.GetChild(0).localPosition = new Vector3(0, -.015f, -.001f);


            if (Vars.Config.VRBody.EnableArmsIK)
            {
                Armature.Arm[] LArms =
                {
                    Rig._LFeedbacker, Rig._LKnuckleblaster,
                    Rig._LWhiplash, Rig._LSandboxer,
                };
                foreach (Armature.Arm Arm in LArms)
                    AddIK(Arm.Hand.Root.gameObject, VRArmsSystem.Instance.CC.ArmOffset.transform, Pole: Rig.Arm_IKPole_Left);

                Armature.Arm[] RArms =
                {
                    Rig._RFeedbacker, Rig._RKnuckleblaster,
                    Rig._RWhiplash, Rig._RSandboxer,
                };
                foreach (Armature.Arm Arm in RArms)
                    AddIK(Arm.Hand.Root.gameObject, VRGunsSystem.Instance.CC.ArmOffset.transform, Pole: Rig.Arm_IKPole_Right);
            }

            if (Vars.Config.VRBody.EnableLegsIK)
            {
                Anim = Rig.GameObjectT.GetComponent<Animator>();
            }

            #region Dynamic wings

            WingBone = Rig.Chest.GetChild(4).gameObject.AddComponent<EZSoftBone>();
            WingBone.m_RootBones.Add(WingBone.transform);
            WingBone.startDepth = 1;
            WingBone.collisionLayers = 1 << (int)Layers.Environment;
            WingBone.deltaTimeMode = EZSoftBone.DeltaTimeMode.Constant;
            WingBone.constantDeltaTime = 5;
            WingBone.Awake();
            WingBone.material.damping = 1;
            WingBone.material.stiffness = .8f;
            WingBone.material.resistance = 0;
            WingBone.material.slackness = 0;

            #endregion

            // neck chain IK
            AddIK(Rig.NeckEnd.gameObject, Rig.Head.GetChild(0).GetChild(0), 2);

            this.gameObject.SetActive(false);
            var ASC = gameObject.AddComponent<AvatarSizeAdjustor>();
            ASC.enabled = false;
            this.gameObject.SetActive(true);
        }

        public void LateUpdate()
        {
            if (Rig == null) return;

            HandleBodyTransform();
            HandleHead();
            HandleArms();
            HandleWings();
            if (Vars.Config.VRBody.EnableLegsIK)
            {
                HandleAnimations();
                HandlePelvisRotation();
            }
            MakeArmCannonsFun();
        }

        private void HandleBodyTransform()
        {
            // Smooth body's position and rotation towards the camera
            Rig.Root.position = Vector3.Lerp(Rig.Root.position, Vars.MainCamera.transform.position, Time.deltaTime * 10f);
            Quaternion Rotation = Quaternion.Lerp(Rig.Abdomen.rotation, Vars.MainCamera.transform.rotation, Time.deltaTime * 2.5f);
            Rig.Root.rotation = Quaternion.Euler(0, Rotation.eulerAngles.y, 0);
        }
        private void HandleHead()
        {
            Rig.Head.GetChild(0).position = Vars.MainCamera.transform.position;
            Rig.Head.GetChild(0).eulerAngles = Vars.MainCamera.transform.eulerAngles;
        }
        private void HandleArms()
        {
            // This method decides which arm to render

            // Reset everything to feedbacker in the main menu
            if (Vars.IsMainMenu)
            {
                Rig.FeedbackerA.GameObject.gameObject.SetActive(true);
                Rig.FeedbackerB.GameObject.gameObject.SetActive(true);
                Rig.Knuckleblaster.GameObject.gameObject.SetActive(false);
                Rig.Whiplash.GameObject.gameObject.SetActive(false);
                Rig.Sandboxer.GameObject.gameObject.SetActive(false);
                return;
            }

            // Sandbox arm
            if (GunControl.Instance != null
            && GunControl.Instance.currentWeapon != null
            && GunControl.Instance.currentWeapon.HasComponent<Sandbox.Arm.SandboxArm>())
            {
                Rig.FeedbackerB.GameObject.gameObject.SetActive(false);
                Rig.Sandboxer.GameObject.gameObject.SetActive(true);
            }
            else
            {
                Rig.FeedbackerB.GameObject.gameObject.SetActive(true);
                Rig.Sandboxer.GameObject.gameObject.SetActive(false);
            }

            // Set activearm to use with whiplash (for convenience)
            Armature.Arm ActiveArm = null;
            switch (FistControl.Instance.currentPunch.type)
            {
                case FistType.Standard:
                default:
                    ActiveArm = Rig.FeedbackerA;
                    Rig.FeedbackerA.GameObject.gameObject.SetActive(true);
                    Rig.Knuckleblaster.GameObject.gameObject.SetActive(false);
                    break;

                case FistType.Heavy:
                    ActiveArm = Rig.Knuckleblaster;
                    Rig.FeedbackerA.GameObject.gameObject.SetActive(false);
                    Rig.Knuckleblaster.GameObject.gameObject.SetActive(true);
                    break;

                case FistType.Spear: break;
            }

            // Whiplash behavior
            if (HookArm.Instance != null && HookArm.Instance.enabled
            && HookArm.Instance.model.activeSelf && !Vars.Config.MBP.CameraWhiplash)
            {
                ActiveArm.GameObject.gameObject.SetActive(false);
                Rig.Whiplash.GameObject.gameObject.SetActive(true);
            }
            else
            {
                ActiveArm.GameObject.gameObject.SetActive(true);
                Rig.Whiplash.GameObject.gameObject.SetActive(false);
            }
        }

        private void HandleWings()
        {
            // stiffer wings while moving; tune these values in the Unity editor
            if (NewMovement.Instance.rb.velocity.magnitude > .1f)
            {
                WingBone.material.damping = .2f;
                WingBone.material.stiffness = .65f;
                WingBone.material.resistance = 0;
                WingBone.material.slackness = 0;
            }
            else
            {
                WingBone.material.damping = .2f;
                WingBone.material.stiffness = .1f;
                WingBone.material.resistance = .9f;
                WingBone.material.slackness = .1f;
            }
        }

        private void HandleAnimations()
        {
            // triggers in the air
            if (!NewMovement.Instance.gc.onGround) Anim.SetBool("Jumping", true);
            else Anim.SetBool("Jumping", false);

            // triggers when a player is either sliding or pulling himself towards something heavy (e.g. hookpoint, maurice)
            if (NewMovement.Instance.sliding || (HookArm.Instance?.state == HookState.Pulling && (bool)!HookArm.Instance?.lightTarget))
            { Anim.SetBool("Jumping", false); Anim.SetBool("Sliding", true); }
            else Anim.SetBool("Sliding", false);
        }
        private void HandlePelvisRotation()
        {
            // turn the pelvis toward the movement, since foot placement does not handle sideways steps
            Vector3 Direction = new Vector3(-NewMovement.Instance.rb.velocity.z, 0, NewMovement.Instance.rb.velocity.x);
            Quaternion Rotation = Quaternion.Euler(0, Quaternion.LookRotation(Direction, Vector3.up).eulerAngles.y + 90, 0);
            if (Input.InputVars.MoveVector.y < 0) Rotation = Quaternion.Euler(0, -Rotation.eulerAngles.y - 180, 0);
            Rig.Pelvis.rotation = Quaternion.Lerp(Rig.Pelvis.rotation, Rotation, Time.deltaTime * 5);
        }

        private bool macf = false;
        private void MakeArmCannonsFun()
        {
            if (!GunControl.Instance || !GunControl.Instance.currentWeapon) return;
            if (GunControl.Instance.currentWeapon.HasComponent<RocketLauncher>())
            {
                macf = true;
            }
            else if (GunControl.Instance.currentWeapon.HasComponent<ShotgunHammer>())
            {
                macf = true;
            }
            else if (macf)
            {
                macf = false;
            }
        }
    }
}
