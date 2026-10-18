using HarmonyLib;
using Plugin.Systems;
using Plugin.Systems.Controllers;
using UnityEngine;
using Valve.VR;

namespace Plugin.Patches.Controllers;

[HarmonyPatch(typeof(NewMovement))]
internal sealed class PatchControllerAdder
{
    // Adds the left and right VR controllers, their models and the guns and arms systems to the player.
    [HarmonyPostfix]
    [HarmonyPatch(nameof(NewMovement.Start))]
    public static void AddHands(NewMovement __instance)
    {
        __instance.gameObject.SetActive(false);

        GameObject LHGO = CreateController("Left Controller");

        VRControllersSystem LCon = LHGO.AddComponent<VRControllersSystem>();
        LCon.RenderModelOffsetPos = new Vector3(.055f, -.1f, -.1f);
        LCon.RenderModelOffsetEulerAngles = new Vector3(75, 0, 0);
        LCon.RenderModelOffsetScale = new Vector3(.65f, .65f, .65f);
        LCon.source = SteamVR_Input_Sources.LeftHand;

        GameObject RHGO = CreateController("Right Controller");

        VRControllersSystem RCon = RHGO.AddComponent<VRControllersSystem>();
        RCon.RenderModelOffsetPos = new Vector3(-.015f, -.105f, -.15f);
        RCon.RenderModelOffsetEulerAngles = new Vector3(75, 0, 0);
        RCon.RenderModelOffsetScale = new Vector3(-.65f, .65f, .65f);
        RCon.source = SteamVR_Input_Sources.RightHand;

        if (Vars.Config.Controllers.DrawControllers)
        {
            GameObject LHMGO = CreateControllerModel(SteamVR_Input_Sources.LeftHand, out GameObject _);
            LHMGO.transform.parent = LHGO.transform;
            LCon.RenderModel = LHMGO;

            GameObject RHMGO = CreateControllerModel(SteamVR_Input_Sources.RightHand, out GameObject _);
            RHMGO.transform.parent = RHGO.transform;
            RCon.RenderModel = RHMGO;
        }

        if (Vars.Config.Controllers.LeftHanded)
        {
            LHGO.AddComponent<VRGunsSystem>();
            RHGO.AddComponent<VRArmsSystem>();
        }
        else
        {
            RHGO.AddComponent<VRGunsSystem>();
            LHGO.AddComponent<VRArmsSystem>();
        }

        __instance.gameObject.SetActive(true);
    }

    // Creates an empty controller object on the IgnoreRaycast layer.
    private static GameObject CreateController(string Name)
    {
        return new GameObject(Name) { layer = (int)Layers.IgnoreRaycast };
    }
    // Creates the controller model for a hand, plus the sandbox model for the dominant hand.
    private static GameObject CreateControllerModel(SteamVR_Input_Sources Source, out GameObject SandboxRM, string Name = "Model")
    {
        GameObject GO = new GameObject(Name) { layer = (int)Layers.IgnoreRaycast };
        SandboxRM = null;

        Transform T;
        if (Source == SteamVR_Input_Sources.LeftHand)
        {
            if (Vars.Config.Controllers.LeftHanded)
            {
                T = Object.Instantiate(Assets.Controller_D).transform;
                SandboxRM = Object.Instantiate(Assets.Controller_D_Sandbox);
            }
            else T = Object.Instantiate(Assets.Controller_ND).transform;
            T.parent = GO.transform;
            T.localPosition = Vector3.zero;
        }
        else if (Source == SteamVR_Input_Sources.RightHand)
        {
            if (Vars.Config.Controllers.LeftHanded)
                T = Object.Instantiate(Assets.Controller_ND).transform;
            else
            {
                T = Object.Instantiate(Assets.Controller_D).transform;
                SandboxRM = Object.Instantiate(Assets.Controller_D_Sandbox);
            }
            T.parent = GO.transform;
            T.localPosition = Vector3.zero;
        }
        else throw new System.NotImplementedException();

        if (SandboxRM != null)
        {
            SandboxRM.transform.parent = GO.transform;
            SandboxRM.transform.localPosition = Vector3.zero;
        }

        return GO;
    }
}
