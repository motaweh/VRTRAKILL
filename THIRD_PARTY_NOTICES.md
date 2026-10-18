# Third-party notices

VRTRAKILL itself is AGPL-3.0 (see `LICENSE.md`). The release zips also contain files from other projects. They keep their own licenses. The full license texts ship in the zips under `BepInEx/plugins/VRTRAKILL/licenses/`, and the same files are in this repo under `ULTRAKILL_COPYTOROOT/BepInEx/plugins/VRTRAKILL/licenses/`.

No ULTRAKILL game files are in this repo or in the zips. The build reads them from your own game install.

## VRTRAKILL code

The mod code is AGPL-3.0 and includes work from these earlier versions, all under the same license:

| Author | Repository | What this version uses |
|---|---|---|
| whateverusername0 | https://github.com/whateverusername0/VRTRAKILL | the original mod and its asset bundle |
| Squaresweets (Matthew Collier) | https://github.com/Squaresweets/VRTRAKILL_FRAUD | the fork this version builds on |
| Guillaume-Lechevallier | https://github.com/Guillaume-Lechevallier/VRTRAKILL_FRAUD | the Unity 2022.3 VRPatcher, OpenVR start-up and portal overlay code, and the basis of the weapon wheel, desktop view, 4-S and Clash camera, late HUD conversion and opt-in VR body |

## Loader (full zip only)

| Files | Project | License | Source |
|---|---|---|---|
| `BepInEx/core/BepInEx*.dll`, `changelog.txt` | BepInEx 5.4.23.5, Copyright (c) 2018 Bepis | MIT (`BepInEx_LICENSE.txt`, from tag v5.4.23.5) | https://github.com/BepInEx/BepInEx/tree/v5.4.23.5 |
| `winhttp.dll`, `doorstop_config.ini`, `.doorstop_version` | UnityDoorstop 4.5.0 (bundled with BepInEx) | LGPL-2.1 (`UnityDoorstop_LICENSE.txt`) | https://github.com/NeighTools/UnityDoorstop/tree/v4.5.0 |
| `BepInEx/core/0Harmony*.dll`, `HarmonyXInterop.dll` | HarmonyX, Copyright (c) 2020 BepInEx | MIT (`HarmonyX_LICENSE.txt`) | https://github.com/BepInEx/HarmonyX |
| `BepInEx/core/MonoMod.*.dll` | MonoMod, Copyright (c) 2015 - 2020 0x0ade | MIT (`MonoMod_LICENSE.txt`) | https://github.com/MonoMod/MonoMod |
| `BepInEx/core/Mono.Cecil*.dll` | Mono.Cecil, Copyright (c) 2008 - 2015 Jb Evain, Copyright (c) 2008 - 2011 Novell, Inc. | MIT (`Mono.Cecil_LICENSE.txt`) | https://github.com/jbevain/cecil |

The BepInEx files are taken unchanged from the official `BepInEx_win_x64_5.4.23.5.zip` release asset. `tools/package.ps1` checks its SHA256 before use.

UnityDoorstop is LGPL-2.1. Its source for the shipped version is at the link above. `winhttp.dll` is the unmodified binary from the BepInEx release.

## Mod payload (both zips)

| Files | Project | License | Source |
|---|---|---|---|
| `BepInEx/patchers/AssetsTools.NET.dll` | AssetsTools.NET 3.0.5 (lib/net40 from nuget.org), Copyright (c) 2020 nesrak1 | MIT (`AssetsTools.NET_LICENSE.txt`) | https://github.com/nesrak1/AssetsTools.NET |
| `BepInEx/patchers/classdata.tpk` | Class database from the UABEA v8 release, Copyright (c) 2021 nesrak1 | MIT (`UABEA_LICENSE.txt`) | https://github.com/nesrak1/UABEA |
| `BepInEx/plugins/VRTRAKILL/SteamVR.dll`, `SteamVR_Actions.dll` and the copies in `ULTRAKILL_Data/Managed/`, `ULTRAKILL_Data/StreamingAssets/SteamVR/` | SteamVR Unity Plugin 2.8.0, Copyright (c) Valve Corporation | BSD-3-Clause (`SteamVR_Unity_Plugin_LICENSE.txt`) | https://github.com/ValveSoftware/steamvr_unity_plugin |
| `ULTRAKILL_Data/Plugins/x86_64/openvr_api.dll` | OpenVR SDK, Copyright (c) 2015, Valve Corporation | BSD-3-Clause (`OpenVR_LICENSE.txt`) | https://github.com/ValveSoftware/openvr |
| `ULTRAKILL_Data/Managed/Unity.XR.OpenVR.dll`, `ULTRAKILL_Data/Plugins/x86_64/XRSDKOpenVR.dll`, `ULTRAKILL_Data/UnitySubsystems/XRSDKOpenVR/` | OpenVR Unity XR Plugin, Copyright (c) 2020, Valve Software | BSD-3-Clause (`Unity-XR-Plugin_OpenVR_LICENSE.txt`) | https://github.com/ValveSoftware/unity-xr-plugin |
| `ULTRAKILL_Data/Managed/Unity.XR.Management.dll`, `Unity.XR.CoreUtils.dll`, `UnityEngine.XR.LegacyInputHelpers.dll`, `UnityEngine.SpatialTracking.dll` | Unity packages, Copyright Unity Technologies ApS | Unity Companion License (`UnityEngine.SpatialTracking_LICENSE.md`) | https://unity.com/legal/licenses/unity-companion-license |
| `BepInEx/plugins/VRTRAKILL/Newtonsoft.Json.dll` | Json.NET, Copyright (c) 2007 James Newton-King | MIT (`Newtonsoft.Json_LICENSE.txt`) | https://github.com/JamesNK/Newtonsoft.Json |
| `ULTRAKILL_Data/Managed/Valve.Newtonsoft.Json.dll` | Json.NET for Unity as shipped in the SteamVR Unity Plugin (James Newton-King; SaladLab) | MIT (`Valve.Newtonsoft.Json_LICENSE_*.txt`) | see below |

`VRPatcher` (`BepInEx/patchers/VRPatcher.dll`) started from the patcher in Raicuparta's Two Forks VR, Copyright (c) 2020 Ricardo Lopes, MIT (`TwoForksVR_LICENSE.txt`, https://github.com/Raicuparta/two-forks-vr).

`vrtrakillassetbundle` (arm and weapon models) was built by the original VRTRAKILL author from the Unity project in the `ultra_revamp` branch of https://github.com/whateverusername0/VRTRAKILL. Some of it is based on ULTRAKILL art. ULTRAKILL is published by New Blood Interactive. This project is not connected to New Blood.

## Provenance of two Managed DLLs

These two were added to resolve references from the shipped SteamVR and Unity XR DLLs.

| File (under `ULTRAKILL_Data/Managed/`) | Assembly identity | Source | Version | SHA256 | License |
|---|---|---|---|---|---|
| `Valve.Newtonsoft.Json.dll` | `Valve.Newtonsoft.Json, Version=9.0.0.0, Culture=neutral, PublicKeyToken=null` | SteamVR Unity plugin release 2.8.0, `steamvr_2_8_0.unitypackage` (SHA256 `d52baff690d0fa6c29e5299ede2594e8443d7f7c989d948f77890a1985aa9191`), path `Assets/SteamVR/Input/Plugins/JSON.NET/`. https://github.com/ValveSoftware/steamvr_unity_plugin/releases/tag/2.8.0 | file version 9.0.1.19813 (same bytes in plugin releases 2.7.3 to 2.8.5) | `5c0d896b41de72178ace5e3fce9f7b74977b3504be3727fa3b495166eccd3eba` | MIT (James Newton-King; SaladLab) |
| `UnityEngine.SpatialTracking.dll` | `UnityEngine.SpatialTracking, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null` | Unity build output of `com.unity.xr.legacyinputhelpers` 2.1.10 (https://download.packages.unity.com/com.unity.xr.legacyinputhelpers/-/com.unity.xr.legacyinputhelpers-2.1.10.tgz, registry sha1 `4716850f3b0c9a398c68cd272d279e68245c49a7`). Same build as the shipped `UnityEngine.XR.LegacyInputHelpers.dll`: both embed `Library/PackageCache/com.unity.xr.legacyinputhelpers@2.1.10` source paths. From commit 5e6c13b of the Squaresweets fork. | 2.1.10 | `4af70f6ad34bd4378142a9f32264cb78131842918f40894f12554473ced8af40` | Unity Companion License |

Which shipped code needs them:

- `SteamVR.dll` (plugin 2.8.0): `Valve.Newtonsoft.Json` in `SteamVR_Input_ActionFile.Open/Save`, `SteamVR_Input.HasFileInMemoryBeenModified`, `SteamVR_Input.CreateEmptyActionsFile` and `SteamVR.GetManifestFile` (editor-only paths); `UnityEngine.SpatialTracking.TrackedPoseDriver` in `SteamVR_CameraHelper.Start` and `InteractionSystem.Player.Awake`.
- `Unity.XR.CoreUtils.dll`: `TrackedPoseDriver` in `XROrigin.Awake`.
- `UnityEngine.XR.LegacyInputHelpers.dll`: `ArmModel` derives from `BasePoseProvider`.

The mod does not call any of these today. `[CameraRig]` in the asset bundle has `SteamVR_CameraHelper`, but the mod never loads it (`Assets.CAMERARIG` in `Systems/Assets.cs` is never set).
