using UnityEngine;

namespace Plugin.Systems.UI
{
    // Moves the crosshair ahead of the gun, or ahead of the view in menus.
    internal class CrosshairController : MonoBehaviour
    {
        // Distance between the object and the crosshair
        public float Length => Vars.Config.CBS.CrosshairDistance;

        // A magic number which gives us the most accurate center
        Vector3 Offset = new Vector3(-.2f, -2.75f, 0);
        Transform Target; // Where crosshair will point from
        GameObject LastWeapon; bool AimFromWeapon; // the GetComponent results for the current weapon

        // Places the crosshair ahead of the view while frozen without controller UI, else ahead of the current gun.
        public void LateUpdate()
        {
            if ((Vars.IsPlayerFrozen || Vars.IsPlayerUsingShop) && !Vars.Config.UIInteraction.ControllerBased)
                transform.position = Vars.MainCamera.transform.position + (Vars.MainCamera.transform.forward * Length * .25f) + Offset;
            else
            {
                if (Vars.Registered<GunControl>() is GunControl Guns)
                {
                    GameObject Weapon = Guns.currentWeapon;
                    if (Weapon != LastWeapon)
                    {
                        LastWeapon = Weapon;
                        AimFromWeapon = Weapon != null && (Weapon.GetComponent<RocketLauncher>() || Weapon.GetComponent<ShotgunHammer>());
                    }
                    Target = AimFromWeapon ? Weapon.transform : Guns.transform;
                }
                else Target = Vars.DominantHand.transform;

                transform.position = Target.position + (Target.forward * Length);
                transform.rotation = Target.rotation;
            }
        }
    }
}
