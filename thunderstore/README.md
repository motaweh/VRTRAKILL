# VRTRAKILL

VR mod for ULTRAKILL, through SteamVR. Version 1.0.2.

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
| Turn | (R) stick left or right. Up and down do not turn or switch weapons. |
| Jump | (R) Cross |
| Slide / slam | (R) stick click |
| Dash | (L) stick click |
| Shoot | (R) R2 |
| Alternate fire | (R) R1 |
| Punch | (L) L2 |
| Whiplash | (L) L1 |
| Change fist (Feedbacker / Knuckleblaster) | (L) Square |
| Change weapon variation | (L) Triangle |
| Weapon wheel | Hold (R) Circle, aim with the (L) stick, let go to switch |
| Pause | (R) Options, or double tap (R) Circle |
| Menu: point | Point either controller at the menu |
| Menu: select | (R) R2 or (L) L2 selects what that laser points at. (R) Cross selects the highlighted item. |
| Menu: move the highlight | (L) stick |
| Menu: scroll | (R) stick up or down |
| Menu: back | Press (R) Circle once. On the first pause page this closes the pause menu. |

(L) Create is not used. SteamVR uses it for screenshots.

### Meta Quest Touch controllers

| Action | Button |
|---|---|
| Move | (L) stick |
| Turn | (R) stick left or right. Up and down do not turn or switch weapons. |
| Jump | (R) A |
| Slide / slam | (R) stick click |
| Dash | (L) stick click |
| Shoot | (R) trigger |
| Alternate fire | (R) grip |
| Punch | (L) trigger |
| Whiplash | (L) grip |
| Change fist (Feedbacker / Knuckleblaster) | (L) X |
| Change weapon variation | (L) Y |
| Weapon wheel | Hold (R) B, aim with the (L) stick, let go to switch |
| Pause | Double tap (R) B |
| Menu: point | Point either controller at the menu |
| Menu: select | (R) trigger or (L) trigger selects what that laser points at. (R) A selects the highlighted item. |
| Menu: move the highlight | (L) stick |
| Menu: scroll | (R) stick up or down |
| Menu: back | Press (R) B once. On the first pause page this closes the pause menu. |

If the buttons still act like an older version, reset SteamVR to the default binding (SteamVR Settings > Controllers > Manage Controller Bindings > ULTRAKILL > Default).

This layout follows the common VR shooter layout: Meta's controller guidance and ULTRAKILL's own gamepad layout.

On Quest, the (L) menu button opens the SteamVR menu. Games cannot use it.

In menus the select buttons do not jump, shoot or punch, also when the menu closes while you hold them. In a shop, (R) Cross or (R) A on a shop button clicks it; anywhere else it jumps. A single press of (R) Circle or (R) B does nothing in play, so the button only pauses on a double tap. In a menu a double tap goes back once.

To switch weapons, use the weapon wheel.

Maps for Index, Vive and WMR, and launch steps for other connections, are in [docs/HEADSETS.md](https://github.com/motaweh/VRTRAKILL/blob/ultrakill-22957324/docs/HEADSETS.md) on GitHub.

## Settings

Mod settings are in `VRTRAKILL_Config.json`, in the mod's folder under `BepInEx/plugins` in your profile. A mod update replaces this file, so note your changes before you update.

Once per start the mod asks GitHub for the latest VRTRAKILL release and shows a notice in the main menu when a newer one exists. Nothing is downloaded. Set "Auto update" to "Off" in the config file to turn the check off. In a mod manager install, "Download" works like "Notify". Update the mod in the mod manager.

- "Finger tracking": optional cosmetic finger curl on the punch hand, off by default.

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
