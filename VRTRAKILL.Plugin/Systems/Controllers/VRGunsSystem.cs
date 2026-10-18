using VRTRAKILL.Utilities;
using UnityEngine;

namespace Plugin.Systems.Controllers
{
    public class VRGunsSystem : MonoSingleton<VRGunsSystem>
    {
        public VRControllersSystem CC;
        public GameObject GunOffset;
        public GameObject RM, SandboxHandRM;

        public Vector3 ArmIKOffset = new Vector3(.05f, .0525f, -.1765f);

        // Gets the controller and its gun offset.
        public void Awake()
        {
            CC = gameObject.GetComponent<VRControllersSystem>();
            GunOffset = CC.GunOffset;
        }
        // Finds the controller and sandbox hand models and hides them, if the controller has a model.
        public void Start()
        {
            if (CC.RenderModel == null || CC.RenderModel.transform.childCount < 2) return;
            RM = CC.RenderModel.transform.GetChild(0).gameObject;
            SandboxHandRM = CC.RenderModel.transform.GetChild(1).gameObject;
            RM.SetActive(false);
            SandboxHandRM.SetActive(false);
        }

        // Sets the arm IK offset and toggles the controller models.
        public void Update()
        {
            CC.ArmOffset.transform.localPosition = ArmIKOffset;
            SetControllers();
        }

        // Shows the controller or sandbox hand model while no gun is active, and hides both while one is.
        private void SetControllers()
        {
            GunControl Guns = GunControl.Instance;
            if (Guns == null || !Guns.activated || Guns.noWeapons)
            {
                if (GunControl.Instance != null
                && GunControl.Instance.currentWeapon != null
                && GunControl.Instance.currentWeapon.HasComponent<Sandbox.Arm.SandboxArm>()
                && !Vars.IsMainMenu)
                {
                    RM?.SetActive(false);
                    SandboxHandRM?.SetActive(true);
                }
                else
                {
                    RM?.SetActive(true);
                    SandboxHandRM?.SetActive(false);
                }
            }
            else
            {
                RM?.SetActive(false);
                SandboxHandRM?.SetActive(false);
            }
        }
    }
}
