# Third-party runtime files

Files in `ULTRAKILL_COPYTOROOT` that come from other projects and were added to resolve references from the shipped SteamVR and Unity XR DLLs. License texts are in `BepInEx/plugins/VRTRAKILL/licenses/`.

| File (under `ULTRAKILL_Data/Managed/`) | Assembly identity | Source | Version | SHA256 | License |
|---|---|---|---|---|---|
| `Valve.Newtonsoft.Json.dll` | `Valve.Newtonsoft.Json, Version=9.0.0.0, Culture=neutral, PublicKeyToken=null` | SteamVR Unity plugin release 2.8.0, `steamvr_2_8_0.unitypackage` (SHA256 `d52baff690d0fa6c29e5299ede2594e8443d7f7c989d948f77890a1985aa9191`), path `Assets/SteamVR/Input/Plugins/JSON.NET/`. https://github.com/ValveSoftware/steamvr_unity_plugin/releases/tag/2.8.0 | file version 9.0.1.19813 (same bytes in plugin releases 2.7.3 to 2.8.5) | `5c0d896b41de72178ace5e3fce9f7b74977b3504be3727fa3b495166eccd3eba` | MIT (James Newton-King; SaladLab) |
| `UnityEngine.SpatialTracking.dll` | `UnityEngine.SpatialTracking, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null` | Unity build output of `com.unity.xr.legacyinputhelpers` 2.1.10 (https://download.packages.unity.com/com.unity.xr.legacyinputhelpers/-/com.unity.xr.legacyinputhelpers-2.1.10.tgz, registry sha1 `4716850f3b0c9a398c68cd272d279e68245c49a7`). Same build as the shipped `UnityEngine.XR.LegacyInputHelpers.dll`: both embed `Library/PackageCache/com.unity.xr.legacyinputhelpers@2.1.10` source paths. Taken from `lib/` (commit 5e6c13b). | 2.1.10 | `4af70f6ad34bd4378142a9f32264cb78131842918f40894f12554473ced8af40` | Unity Companion License |

Which shipped code needs them:

- `SteamVR.dll` (plugin 2.8.0): `Valve.Newtonsoft.Json` in `SteamVR_Input_ActionFile.Open/Save`, `SteamVR_Input.HasFileInMemoryBeenModified`, `SteamVR_Input.CreateEmptyActionsFile` and `SteamVR.GetManifestFile` (editor-only paths); `UnityEngine.SpatialTracking.TrackedPoseDriver` in `SteamVR_CameraHelper.Start` and `InteractionSystem.Player.Awake`.
- `Unity.XR.CoreUtils.dll`: `TrackedPoseDriver` in `XROrigin.Awake`.
- `UnityEngine.XR.LegacyInputHelpers.dll`: `ArmModel` derives from `BasePoseProvider`.

The mod does not call any of these today. `[CameraRig]` in the asset bundle has `SteamVR_CameraHelper`, but the code that instantiates it is commented out (`Systems/Assets.cs`).
