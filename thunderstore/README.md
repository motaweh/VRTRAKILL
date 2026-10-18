# VRTRAKILL

VR mod for ULTRAKILL, through SteamVR.

This version runs on the current ULTRAKILL on Steam: build 22957324 (Unity 2022.3.29f1). You do not need to downgrade the game. It was built and tested on build 22957324 only. If Steam updates the game, the mod can break until it gets an update.

## Requirements

- ULTRAKILL on Steam, build 22957324.
- SteamVR. The mod uses OpenVR only, so every headset goes through SteamVR.
- A mod manager: r2modman, Gale or the Thunderstore Mod Manager. It installs BepInExPack for you.

## Install

1. Install VRTRAKILL with your mod manager.
2. Start SteamVR and connect your headset.
3. Start the game with "Start modded".

A mod manager can only put files in the BepInEx folder of its profile. VR also needs files in the game's own `ULTRAKILL_Data` folder. On each start, `VRPatcher` copies these files from the mod into `ULTRAKILL_Data` if they are missing or different:

- `Managed/`: the SteamVR and Unity XR DLLs
- `Plugins/x86_64/`: `XRSDKOpenVR.dll` and `openvr_api.dll`
- `StreamingAssets/SteamVR/`: the SteamVR actions and default bindings
- `UnitySubsystems/XRSDKOpenVR/`: the OpenVR subsystem manifest

It also edits `ULTRAKILL_Data/globalgamemanagers` to turn on VR, and keeps the original as `globalgamemanagers.vrtrakill.bak`.

The copy runs before Unity reads these files, so VR should start on the first launch. VRPatcher checks this and writes the result to `BepInEx/LogOutput.log` in your profile. If the log says `RESTART NEEDED`, close the game and start it again.

If VR does not start, open `BepInEx/LogOutput.log` in your profile (mod manager > Settings > Browse profile folder).

## Headsets

| Headset | Status |
|---|---|
| PlayStation VR2 (PC adapter) | Tested |
| Meta Quest 2, 3, 3S, Pro (Link, Air Link, Steam Link, or Virtual Desktop in SteamVR mode) | Configured, not tested |
| Valve Index | Configured, not tested |
| HTC Vive, Vive Pro | Configured, not tested |
| Windows Mixed Reality | Configured, not tested |
| HTC Vive Cosmos controllers | Not supported |
| Logitech VR Ink stylus | Not supported |
| Hand tracking, OpenXR without SteamVR | Not supported |

"Configured" means the default binding covers every game action and was checked against the SteamVR driver's input profile. Nobody has played with it yet. Reports from Quest, Index, Vive and WMR players are welcome.

You can change any binding in SteamVR > Settings > Controllers > Manage Controller Bindings > ULTRAKILL.

Mod settings are in `VRTRAKILL_Config.json`, in the mod's folder under `BepInEx/plugins` in your profile. A mod update replaces this file, so note your changes before you update.

## Known issues

- Only PS VR2 has been played. The other bindings are checked on paper only.
- Left-handed mode is broken.
- Rumble during play is not confirmed on any controller.
- Finger tracking does nothing. Hands use fixed poses.
- Portals seen in a mirror do not render correctly.
- Whiplash is unreliable through portals.
- A future ULTRAKILL update can break the mod.

The full list is on the [GitHub page](https://github.com/motaweh/VRTRAKILL).

## Uninstall

1. Remove VRTRAKILL in your mod manager.
2. The mod manager does not remove the files that VRPatcher copied into the game folder. In Steam, right-click ULTRAKILL > Manage > Browse local files, then delete these from `ULTRAKILL_Data`:
   - `Managed/`: `SteamVR.dll`, `SteamVR_Actions.dll`, `Unity.XR.CoreUtils.dll`, `Unity.XR.Management.dll`, `Unity.XR.OpenVR.dll`, `UnityEngine.SpatialTracking.dll`, `UnityEngine.XR.LegacyInputHelpers.dll`, `Valve.Newtonsoft.Json.dll`
   - `Plugins/x86_64/`: `XRSDKOpenVR.dll`, `openvr_api.dll`
   - the `StreamingAssets/SteamVR/` folder and the `UnitySubsystems/XRSDKOpenVR/` folder
   - `globalgamemanagers.vrtrakill.bak` and `globalgamemanagers.vrtrakill.sha256`
3. In Steam, right-click ULTRAKILL > Properties > Installed Files > Verify integrity of game files. This restores the original `globalgamemanagers`.

Steam's verify step does not remove extra files, so do step 2 as well. "Start vanilla" in the mod manager does not undo these changes.

## Credits

- whateverusername0 created VRTRAKILL ([original repo](https://github.com/whateverusername0/VRTRAKILL)). The original README put development on indefinite hold and pointed players to forks for newer game versions. This is one of those forks.
- Squaresweets (Matthew Collier) made the [VRTRAKILL_FRAUD](https://github.com/Squaresweets/VRTRAKILL_FRAUD) fork. This version builds on it.
- Guillaume-Lechevallier: the VRPatcher, portal overlay and OpenVR start-up code are ported from the [Guillaume-Lechevallier VRTRAKILL_FRAUD](https://github.com/Guillaume-Lechevallier/VRTRAKILL_FRAUD) fork. So are the weapon wheel stick fix, the desktop view cameras, the 4-S and Clash mode camera, the late HUD and menu conversion, and the opt-in VR body loading.
- Graze, Rokic, Party Wumpus and NavaShield contributed to the original mod.
- Raicuparta: the globalgamemanagers patcher approach comes from Two Forks VR (MIT).

Updated for build 22957324 by Motaweh.

## License

AGPL-3.0. The source code is on [GitHub](https://github.com/motaweh/VRTRAKILL). `LICENSE.md` and `THIRD_PARTY_NOTICES.md` are in the mod's plugin folder. Third-party files (SteamVR plugin, OpenVR, Unity XR packages, AssetsTools.NET, Newtonsoft.Json and others) keep their own licenses.

This is a fan project. It is not made by or connected to New Blood. You need your own copy of ULTRAKILL.
