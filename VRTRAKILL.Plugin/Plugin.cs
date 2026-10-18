using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using Valve.VR;
using Plugin.Systems;
using VRTRAKILL.Utilities;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using Unity.XR.OpenVR;
using System.Collections.Generic;
using System;
using System.Collections;

namespace Plugin
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public sealed partial class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log { get; private set; }
        private static XRGeneralSettings generalSettings;
        private static XRManagerSettings managerSettings;
        private static OpenVRLoader xrLoader;
        private const int VRInitAttempts = 90;
        private const float VRInitRetryDelay = 2f;
        // PreInitialize creates new action objects; a second call orphans the ones already handed out
        private static bool ActionsPreInitialized;

        // Loads the config, applies the patches, starts the scene worker, then starts VR init.
        public void Awake()
        {
            Log = Logger;
            Debug.unityLogger.filterLogType = LogType.Warning;

            // a setup failure must not stop VR init, or the game falls back to SteamVR theater mode;
            // each step has its own try so a bad config or Binds.json does not also skip the patches
            Patches.Controllers.PatchControllerHaptics.WatchQuit();
            try { Prefs.ConfigMaster.Init(); }
            catch (Exception e) { Log.LogError("VRTRAKILL config setup failed: " + e); }
            try { PatchStuff(); }
            catch (Exception e) { Log.LogError("VRTRAKILL patching failed: " + e); }
            try { SceneWorker.Init(this); }
            catch (Exception e) { Log.LogError("VRTRAKILL scene setup failed: " + e); }

            Systems.Updates.UpdateService.Begin(this); // background update check; never throws
            StartCoroutine(InitVRWhenReady());
        }

        // Retries VR init every 2 seconds for up to 3 minutes, as SteamVR may still be starting.
        private IEnumerator InitVRWhenReady()
        {
            for (int attempt = 1; attempt <= VRInitAttempts; attempt++)
            {
                if (InitVRLoader(attempt == 1)) yield break;
                if (attempt == 1) Log.LogWarning("OpenVR is not ready yet, retrying until SteamVR responds.");
                yield return new WaitForSecondsRealtime(VRInitRetryDelay);
            }
            Log.LogError("OpenVR did not respond after three minutes. Keep SteamVR open and restart the game.");
        }

        // Applies the Harmony patches in the mod patch namespaces, and the avatar patches when the avatar is enabled.
        private void PatchStuff()
        {
            System.Collections.Generic.List<string> Namespaces = new System.Collections.Generic.List<string>
            {
                typeof(Patches.Controllers.A).Namespace,
                typeof(Patches.Misc.A).Namespace,
                typeof(Patches.ULTRAKILL.A).Namespace,
                typeof(Patches.Visual.A).Namespace,

                typeof(Systems.VRCamera.Patches.A).Namespace,
                typeof(Systems.UI.Patches.A).Namespace,
                typeof(Systems.Movement.Patches.A).Namespace,
            };
            System.Collections.Generic.List<System.Type> Types = new System.Collections.Generic.List<System.Type>
            {
            };
            // the avatar is opt-in; with it off nothing in its namespace is patched
            if (Vars.Config.EnableVRBody) Namespaces.Add(typeof(Systems.VRAvatar.Patches.A).Namespace);

            new Patcher(new HarmonyLib.Harmony(PluginInfo.PLUGIN_GUID))
            {
                Namespaces = Namespaces.ToArray(),
                Types = Types.ToArray(),
                Log = Vars.Log,
            }.PatchAll();
        }

        // Starts OpenVR through XR Management and SteamVR, then SteamVR input; returns false on failure.
        public static bool InitVRLoader(bool logFailure = true)
        {
            try
            {
                Log.LogMessage("INITILIZING");
                if (!ActionsPreInitialized) { SteamVR_Actions.PreInitialize(); ActionsPreInitialized = true; }

                generalSettings = ScriptableObject.CreateInstance<XRGeneralSettings>();
                managerSettings = ScriptableObject.CreateInstance<XRManagerSettings>();
                xrLoader = ScriptableObject.CreateInstance<OpenVRLoader>();

                var settings = OpenVRSettings.GetSettings();
                if (settings == null) throw new InvalidOperationException("OpenVR settings not found.");
                settings.StereoRenderingMode = OpenVRSettings.StereoRenderingModes.MultiPass;

                generalSettings.Manager = managerSettings;

                // the game has no serialized XR loader asset, so fill the list directly (TrySetLoaders rejects runtime loaders)
                var activeLoaders = managerSettings.activeLoaders as List<XRLoader>;
                if (activeLoaders == null) throw new InvalidOperationException("XR loader list is not writable.");
                activeLoaders.Clear();
                activeLoaders.Add(xrLoader);
                managerSettings.InitializeLoaderSync();

                Log.LogMessage("Active loader: " + managerSettings.activeLoader);

                XRLoader loader = managerSettings.activeLoader;
                if (loader == null)
                {
                    // activeLoader can stay unset for a manager created at runtime; initialize the loader directly
                    if (SteamVR.initializedState != SteamVR.InitializedStates.InitializeSuccess)
                    {
                        try { SteamVR.Initialize(true); }
                        catch (Exception e) { Log.LogDebug("SteamVR init deferred: " + e.Message); }
                    }
                    if (!xrLoader.Initialize())
                        throw new InvalidOperationException("OpenVR loader failed to initialize (SteamVR: " + SteamVR.initializedState + ").");
                    loader = xrLoader;
                }

                if (SteamVR.initializedState != SteamVR.InitializedStates.InitializeSuccess)
                    SteamVR.Initialize(true);
                Log.LogMessage("SteamVR initialized. Active: " + SteamVR.active + " Connected: " + SteamVR.initializedState);

                Log.LogMessage("Initilizing:" + loader.Start());

                var display = loader.GetLoadedSubsystem<XRDisplaySubsystem>();
                if (display == null) throw new InvalidOperationException("OpenVR display subsystem is missing.");
                if (!display.running) display.Start();

                SteamVR_Input.Initialize();
                Systems.Input.SVRActionsManager.Init();
                Log.LogMessage("DONE");
                return true;
            }
            catch (Exception e)
            {
                CleanupVRLoader();
                if (logFailure) Log.LogWarning("OpenVR is not available yet: " + e.GetType().Name + " - " + e.Message);
                return false;
            }
        }

        // Deinitializes and destroys the XR loader objects so the next attempt starts clean.
        private static void CleanupVRLoader()
        {
            try
            {
                if (managerSettings != null && managerSettings.activeLoader != null)
                    managerSettings.DeinitializeLoader();
            }
            catch (Exception e) { Log?.LogDebug("OpenVR cleanup deferred: " + e.Message); }

            if (xrLoader != null) Destroy(xrLoader);
            if (managerSettings != null) Destroy(managerSettings);
            if (generalSettings != null) Destroy(generalSettings);

            xrLoader = null;
            managerSettings = null;
            generalSettings = null;
        }
    }
}
