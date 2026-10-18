# VRTRAKILL

VR mod for ULTRAKILL, through SteamVR.

This version runs on the current ULTRAKILL on Steam: build 22957324 (Unity 2022.3.29f1). You do not need to downgrade the game. Older VRTRAKILL releases only worked on the version before ULTRA_REVAMP, and the Fraud fork needed a specific depot. This one does not.

It was built and tested on build 22957324 only. If Steam updates the game, the mod can break until it gets an update.

## Download

Get the latest release from the [Releases page](../../releases). There are two zips:

- `VRTRAKILL-1.0.0-full.zip`: the mod plus BepInEx 5.4.23.5 (x64). Use this one if you have never installed BepInEx.
- `VRTRAKILL-1.0.0-modonly.zip`: the mod only, for players who already have BepInEx 5.4.23.5.

The zip root is the ULTRAKILL game folder. It contains `BepInEx/`, `ULTRAKILL_Data/` and (in the full zip) `winhttp.dll`, `doorstop_config.ini`, `.doorstop_version` and `changelog.txt`.

## Requirements

- ULTRAKILL on Steam, build 22957324.
- SteamVR. The mod uses OpenVR only, so every headset goes through SteamVR. The active OpenXR runtime does not matter.
- A headset from the list below.

## Install

1. Close ULTRAKILL.
2. In Steam, right-click ULTRAKILL, then Manage > Browse local files. This opens the folder that has `ULTRAKILL.exe`.
3. Extract the zip into that folder.
4. If Windows asks to replace or merge files, say yes.
5. Start SteamVR and connect your headset.
6. Start ULTRAKILL from Steam.

On the first start, `BepInEx/patchers/VRPatcher.dll` edits `ULTRAKILL_Data/globalgamemanagers` to turn on VR. It keeps the original as `globalgamemanagers.vrtrakill.bak`. When a game update replaces that file, the patcher sees the new file and patches it again on the next start.

If VR does not start, read `BepInEx/LogOutput.log`.

To update the mod, extract the new zip over the old install.

### Upgrading from an older VRTRAKILL

Older versions copied files this version does not use. You can delete these from `ULTRAKILL_Data`:

- `Plugins/x86_64/OVRPlugin.dll`
- `Plugins/x86_64/AudioPluginOculusSpatializer.dll`
- `Plugins/x86_64/UnityOpenXR.dll`
- `Plugins/x86_64/openxr_loader.dll`
- `UnitySubsystems/UnityOpenXR/` (the folder)
- `Managed/Unity.XR.OpenXR*.dll`

If you downgraded the game for an older release, switch it back to the current version first.

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

Launch steps for each headset and the full button map are in [docs/HEADSETS.md](docs/HEADSETS.md). On Quest, jump is (R) A; on PS VR2, jump is (R) cross. On both, change weapon variation (and select in menus) is the left stick click. You can change any binding in SteamVR > Settings > Controllers > Manage Controller Bindings > ULTRAKILL.

Mod settings are in `BepInEx/plugins/VRTRAKILL/VRTRAKILL_Config.json` (turning, deadzone, snap turn, crosshair, punching and more).

## Known issues

- Only PS VR2 has been played. The other bindings are checked on paper only.
- Vive wand: next / previous weapon is not bound. Use the weapon wheel.
- The weapon wheel button holds the game's "Previous Variation" key (Q). On Index a short press changes to the previous variation. Bind a key to "Last Used Weapon" in the game's controls to get quick swap on a short press.
- Left-handed mode is broken.
- Rumble during play is not confirmed on any controller.
- Finger tracking does nothing. Hands use fixed poses.
- After an update that changes a default binding, SteamVR can keep the old one. Select the default binding again in the binding screen.
- Portals seen in a mirror do not render correctly.
- Portals can flicker for a moment when you go through them.
- Whiplash is unreliable through portals.
- Oil stains do not show, and blood stains are missing in most levels.
- The desktop view (T key, off by default) skips the game's post-processing, and portals look wrong in it.
- 4-S and Clash mode have not been played in VR yet. The pause menu does not show in the headset while you are in them.
- The VR body ("Enable VRAvatar" in the config file) is off by default and has not been tested.
- The shop weapon and enemy previews and the late HUD conversion are not yet confirmed in the headset.

## Uninstall

1. Close ULTRAKILL.
2. If you used the full zip, delete `winhttp.dll`, `doorstop_config.ini`, `.doorstop_version`, `changelog.txt` and the `BepInEx` folder from the game folder. If you have other BepInEx mods, delete only `BepInEx/plugins/VRTRAKILL/` and the three files in `BepInEx/patchers/` from this mod (`VRPatcher.dll`, `AssetsTools.NET.dll`, `classdata.tpk`).
3. Delete the files the zip added under `ULTRAKILL_Data`. They are the same paths as in the zip: the SteamVR and Unity XR DLLs in `Managed/` and `Plugins/x86_64/`, the `StreamingAssets/SteamVR/` folder and the `UnitySubsystems/XRSDKOpenVR/` folder. Also delete `globalgamemanagers.vrtrakill.bak` and `globalgamemanagers.vrtrakill.sha256`.
4. In Steam, right-click ULTRAKILL > Properties > Installed Files > Verify integrity of game files. This restores the original `globalgamemanagers`.

Steam's verify step does not remove extra files, so do step 3 as well.

## Building from source

You need the .NET SDK pinned in `global.json` (7.0.400 or a later 7.0 feature band) and ULTRAKILL build 22957324 with BepInEx 5.4.23.5 installed in it. The game's DLLs are not in this repo. The build reads Assembly-CSharp, the Unity DLLs and the BepInEx core from your game folder, and publicizes Assembly-CSharp at build time.

The game folder comes from the `ULTRAKILLDir` property. The default is `C:\Program Files (x86)\Steam\steamapps\common\ULTRAKILL`. Set it if your game is somewhere else, either as an environment variable or on the command line:

```
dotnet build VRTRAKILL.sln -c Release -p:ULTRAKILLDir="E:\SteamLibrary\steamapps\common\ULTRAKILL"
```

To make the release zips, run:

```
pwsh tools/package.ps1 -ULTRAKILLDir "E:\SteamLibrary\steamapps\common\ULTRAKILL"
```

It builds Release, downloads the BepInEx 5.4.23.5 x64 zip from the BepInEx GitHub release, checks its SHA256, and writes `VRTRAKILL-1.0.0-full.zip`, `VRTRAKILL-1.0.0-modonly.zip` and `SHA256SUMS.txt` to `artifacts/`.

Everything the mod installs besides its three DLLs is in `ULTRAKILL_COPYTOROOT`, laid out like the game folder.

The Unity project for `vrtrakillassetbundle` is not in this repo. It is `_unityProject.7z` in the `ultra_revamp` branch of the [original repo](https://github.com/whateverusername0/VRTRAKILL/tree/ultra_revamp).

## Credits

- whateverusername0 created VRTRAKILL ([original repo](https://github.com/whateverusername0/VRTRAKILL)). The original README put development on indefinite hold and pointed players to forks for newer game versions. This is one of those forks.
- Squaresweets (Matthew Collier) made the [VRTRAKILL_FRAUD](https://github.com/Squaresweets/VRTRAKILL_FRAUD) fork. This version builds on it.
- Guillaume-Lechevallier: the VRPatcher, portal overlay and OpenVR start-up code are ported from the [Guillaume-Lechevallier VRTRAKILL_FRAUD](https://github.com/Guillaume-Lechevallier/VRTRAKILL_FRAUD) fork. So are the weapon wheel stick fix, the desktop view cameras, the 4-S and Clash mode camera, the late HUD and menu conversion, and the opt-in VR body loading.
- Graze, Rokic, Party Wumpus and NavaShield contributed to the original mod.
- Raicuparta: the globalgamemanagers patcher approach comes from Two Forks VR (MIT).

Updated for build 22957324 by Motaweh.

## License

AGPL-3.0. See [LICENSE.md](LICENSE.md). Modified by Motaweh, 2026.

Third-party files (BepInEx, UnityDoorstop, SteamVR plugin, OpenVR, Unity XR packages, AssetsTools.NET, Newtonsoft.Json and others) keep their own licenses. See [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).

This is a fan project. It is not made by or connected to New Blood. You need your own copy of ULTRAKILL.
