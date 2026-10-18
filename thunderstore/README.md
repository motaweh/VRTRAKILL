# VRTRAKILL

VR mod for ULTRAKILL, through SteamVR. Version 1.0.1.

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
| Meta Quest 2, 3, 3S, Pro (through SteamVR) | Tested |
| Valve Index | Configured, not tested |
| HTC Vive, Vive Pro | Configured, not tested |
| Windows Mixed Reality | Configured, not tested |
| HTC Vive Cosmos controllers | Not supported |
| Logitech VR Ink stylus | Not supported |
| Hand tracking, OpenXR without SteamVR | Not supported |

"Tested" means it was played on hardware through SteamVR. "Configured" means the default binding covers every game action and was checked against the SteamVR driver's input profile. Nobody has played with it yet. Reports from Index, Vive and WMR players are welcome.

## Controls

(R) is the right controller, (L) is the left controller. You can change any binding in SteamVR > Settings > Controllers > Manage Controller Bindings > ULTRAKILL.

### PS VR2 Sense controllers

| Action | Button |
|---|---|
| Move | (L) stick |
| Turn | (R) stick left or right |
| Jump | (R) Cross |
| Slide / slam | (R) stick click |
| Dash | (L) Triangle |
| Shoot | (R) R2 |
| Alternate fire | (R) R1 |
| Punch | (L) L2 |
| Whiplash | (L) Square |
| Change hand | (L) L1 |
| Change weapon variation | (L) stick click |
| Next / previous weapon | (R) stick flick up or down |
| Weapon wheel | Hold (R) Circle, aim with the (L) stick, let go to switch |
| Pause / back | (R) Options, or double tap (R) Circle |

### Meta Quest Touch controllers

| Action | Button |
|---|---|
| Move | (L) stick |
| Turn | (R) stick left or right |
| Jump | (R) A |
| Slide / slam | (R) stick click |
| Dash | (L) Y |
| Shoot | (R) trigger |
| Alternate fire | (R) grip |
| Punch | (L) trigger |
| Whiplash | (L) X |
| Change hand | (L) grip |
| Change weapon variation | (L) stick click |
| Next / previous weapon | (R) stick flick up or down |
| Weapon wheel | Hold (R) B, aim with the (L) stick, let go to switch |
| Pause / back | Double tap (R) B |

On Quest, the (L) menu button opens the SteamVR menu. Games cannot use it.

In menus, point with either controller and press its trigger to click. The (L) stick moves the selection and the left stick click selects it.

One flick of the (R) stick up or down switches one weapon. Return the stick to the center before the next flick.

Maps for Index, Vive and WMR, and launch steps for other connections, are in [docs/HEADSETS.md](https://github.com/motaweh/VRTRAKILL/blob/ultrakill-22957324/docs/HEADSETS.md) on GitHub.

## Settings

Mod settings are in `VRTRAKILL_Config.json`, in the mod's folder under `BepInEx/plugins` in your profile. A mod update replaces this file, so note your changes before you update.

Once per start the mod asks GitHub for the latest VRTRAKILL release and shows a notice in the main menu when a newer one exists. Nothing is downloaded. Set "Auto update" to "Off" in the config file to turn the check off. In a mod manager install, "Download" works like "Notify". Update the mod in the mod manager.

"Finger tracking" is experimental and off by default. It is not tested on a headset yet. When it is on, the punch arm fingers follow the SteamVR finger data.

## Known issues

- Index, Vive and WMR have not been played. Their bindings are checked against the driver input profiles only.
- Left-handed mode is broken.
- Finger tracking is experimental and off by default. It is not tested on a headset yet. It moves the punch arm fingers only.
- After an update that changes a default binding, SteamVR can keep the old one. Select the default binding again in the binding screen.
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
- Guillaume-Lechevallier: thank you for your [VRTRAKILL_FRAUD fork](https://github.com/Guillaume-Lechevallier/VRTRAKILL_FRAUD), which did the groundwork for the current game build (the Unity 2022.3 VRPatcher, the OpenVR start-up and the portal overlay). Our weapon wheel, desktop view, 4-S and Clash camera, late HUD conversion and opt-in VR body are based on your work, and your notes pointed us at the shop preview problem.
- Graze, Rokic, Party Wumpus and NavaShield contributed to the original mod.
- Raicuparta: the globalgamemanagers patcher approach comes from Two Forks VR (MIT).

Updated for build 22957324 by Motaweh.

## License

AGPL-3.0. The source code is on [GitHub](https://github.com/motaweh/VRTRAKILL). `LICENSE.md` and `THIRD_PARTY_NOTICES.md` are in the mod's plugin folder. Third-party files (SteamVR plugin, OpenVR, Unity XR packages, AssetsTools.NET, Newtonsoft.Json and others) keep their own licenses.

This is a fan project. It is not made by or connected to New Blood. You need your own copy of ULTRAKILL.
