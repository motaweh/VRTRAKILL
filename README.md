# VRTRAKILL

VR mod for ULTRAKILL, through SteamVR.

This version runs on the current ULTRAKILL on Steam: build 22957324 (Unity 2022.3.29f1). You do not need to downgrade the game. Older VRTRAKILL releases only worked on the version before ULTRA_REVAMP, and the Fraud fork needed a specific depot. This one does not.

It was built and tested on build 22957324 only. If Steam updates the game, the mod can break until it gets an update.

## Download

Get the latest release from the [Releases page](../../releases). There are two zips:

- `VRTRAKILL-1.0.1-full.zip`: the mod plus BepInEx 5.4.23.5 (x64). Use this one if you have never installed BepInEx.
- `VRTRAKILL-1.0.1-modonly.zip`: the mod only, for players who already have BepInEx 5.4.23.5.

The release also has `SHA256SUMS.txt` and its signature `SHA256SUMS.txt.sig`, which the in-game updater checks.

For r2modman or Gale, use the Thunderstore package `VRTRAKILL-1.0.1.zip` instead. Its install steps are in [thunderstore/README.md](thunderstore/README.md).

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

To update the mod, or to upgrade from an older VRTRAKILL, extract the new zip over the old install. Files from older versions can stay; this version starts VR through OpenVR only and does not load them.

If you downgraded the game for an older release, switch it back to the current version in Steam first (right-click ULTRAKILL > Properties > Betas, and select None).

## Setting up your headset

### PlayStation VR2

1. You need the PlayStation VR2 PC adapter, a PC with Windows 10 or 11 (64-bit), a graphics card with a DisplayPort 1.4 output, a free USB 3.0 port, and Bluetooth 4.0 or later for the PS VR2 Sense controllers.
2. Connect the PC adapter to the PC: the DisplayPort cable directly to the graphics card, and the USB cable directly to the PC (no hub or extension cable).
3. Install the free "PlayStation VR2 App" from the Steam store.
4. Run its setup. It connects the adapter, pairs the Sense controllers over Bluetooth and sets up your play area.
5. SteamVR starts with the headset. If it does not, start SteamVR.
6. Start ULTRAKILL.

### Meta Quest

1. Install the Meta Horizon Link app on the PC. Go to https://www.meta.com/quest/setup/, find Quest Link (PC), click Download software and run the installer. Sign in with your Meta account.
2. Connect the headset to the PC with a good USB-C cable that supports USB 3.
3. In the headset, open Quick Settings, then Link, and accept.
4. Start SteamVR on the PC.
5. Start ULTRAKILL.

Air Link and Virtual Desktop (in SteamVR mode) also connect a Quest over Wi-Fi, but the Link steps above are the documented path. The wireless options have not been tested with this mod.

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

Maps for Index, Vive and WMR, and launch steps for other connections, are in [docs/HEADSETS.md](docs/HEADSETS.md).

Mod settings are in `BepInEx/plugins/VRTRAKILL/VRTRAKILL_Config.json` (turning, deadzone, snap turn, crosshair, punching and more).

## Changes in 1.0.1

- Controllers no longer keep vibrating after a slide, whiplash or nailgun shot.
- Turning with the right stick no longer switches weapons.
- One flick of the right stick up or down switches exactly one weapon.
- A held punch no longer fires on its own while you walk or use the weapon wheel.
- Punches use stamina and cooldown like in the normal game.
- Jump moved to Cross on PS VR2 and A on Quest. Change weapon variation and menu select moved to the left stick click.
- Held buttons are released when the SteamVR menu opens.
- Jackhammer impact, coin toss, dual wield and parry fixes.
- The SteamVR NoData errors at startup are gone.
- A broken or empty config file no longer disables the mod.
- Turning off "Draw controller models" no longer breaks the hands.
- The weapon wheel uses its own stick and you stop walking while it is open.
- Shop enemy and weapon previews render in the headset.
- 4-S and Clash mode show the platformer camera in the headset.
- HUD and menus that appear after a level loads are converted for VR, so the HUD stays visible.
- The desktop view toggle (T key) works.
- Faster start: VRPatcher no longer makes BepInEx rewrite Assembly-CSharp on every start.
- "Enable Controller Rumble" and "Enable Crosshair" in the config file now work.
- New option "Finger tracking" (experimental, off by default): the punch arm fingers follow the SteamVR finger data.
- New option "Auto update". "Notify" (default) shows a notice in the main menu when a new release is out. "Download" (opt-in) downloads a signed update and installs it at the next start. "Off" turns the check off.
- New option "Game blood and oil stains (experimental)", off by default.
- Thunderstore package for r2modman and Gale. VRPatcher copies the VR game files into `ULTRAKILL_Data` at start-up, so the mod works from a mod manager profile. The zip install works as before.

The full list is in [CHANGELOG.md](CHANGELOG.md).

## Updates

Once per game start, a few seconds after launch, the mod asks GitHub for the latest VRTRAKILL release. It sends one plain GET request to `https://api.github.com/repos/motaweh/VRTRAKILL/releases/latest` with the header `User-Agent: VRTRAKILL/<your version>`. It sends no personal data, no game data and no ID. If you are offline or GitHub does not answer within 10 seconds, the check is skipped until the next start. Prereleases are ignored.

The `"Auto update"` setting in `VRTRAKILL_Config.json` controls what happens:

- `"Notify"` (default): if a newer release exists, the log says so and a short notice shows at the bottom of the main menu for 15 seconds. Nothing is downloaded.
- `"Download"`: the mod also downloads `VRTRAKILL-<version>-modonly.zip`, `SHA256SUMS.txt` and `SHA256SUMS.txt.sig` into `BepInEx/VRTRAKILL_update/staging/`. It checks that the signature file is a valid signature of `SHA256SUMS.txt` made with the VRTRAKILL release key, and that the zip matches its SHA-256 line. If a check fails, the download is deleted. Nothing is installed while the game runs. At the next start, before any plugin loads, VRPatcher checks the files again and installs them. It moves the files it replaces to `BepInEx/VRTRAKILL_update/backup/<old version>/`. If a file cannot be written, it puts the old files back. Your `VRTRAKILL_Config.json` is kept. The result is in `BepInEx/VRTRAKILL_update/last_update.txt` and in `BepInEx/LogOutput.log`. A new `VRPatcher.dll` takes effect from the start after the install.
- `"Off"`: no request is sent.

In a mod manager install (r2modman, Gale), `"Download"` works like `"Notify"`. Update the mod in the mod manager.

An install only writes to `BepInEx/plugins/VRTRAKILL/`, `BepInEx/patchers/` and the mod's folders under `ULTRAKILL_Data`. A zip with any other path is rejected. Files that are not in the new zip are left in place. If an install fails, that version is not downloaded again until you delete `BepInEx/VRTRAKILL_update/failed_<version>.txt`. To uninstall the updater's files, delete `BepInEx/VRTRAKILL_update/`.

## Known issues

- Index, Vive and WMR have not been played. Their bindings are checked against the driver input profiles only.
- Vive wand: next / previous weapon is not bound. Use the weapon wheel.
- The weapon wheel button holds the game's "Previous Variation" key (Q). On Index a short press changes to the previous variation. Bind a key to "Last Used Weapon" in the game's controls to get quick swap on a short press.
- Left-handed mode is broken.
- Finger tracking is off by default and is not confirmed on hardware. It moves the punch arm fingers only.
- After an update that changes a default binding, SteamVR can keep the old one. Select the default binding again in the binding screen.
- Portals seen in a mirror do not render correctly.
- Portals can flicker for a moment when you go through them.
- Whiplash is unreliable through portals.
- Oil stains do not show, and blood stains are missing in most levels. The mod turns off the game's compute shader stain path before each frame. Setting "Game blood and oil stains (experimental)" to true in the config file turns it back on. This is not tested in a headset and may tint the view red, which is probably why it was turned off.
- The desktop view (T key, off by default) skips the game's post-processing, and portals look wrong in it.
- 4-S and Clash mode have not been played in VR yet. The pause menu does not show in the headset while you are in them.
- The VR body ("Enable VRAvatar" in the config file) is off by default and has not been tested.
- The shop weapon and enemy previews and the late HUD conversion are not yet confirmed in the headset.

## Uninstall

1. Close ULTRAKILL.
2. If no other mods use BepInEx, delete the `BepInEx` folder and the doorstop files `winhttp.dll`, `doorstop_config.ini` and `.doorstop_version` from the game folder. If you have other BepInEx mods, keep those and delete only `BepInEx/plugins/VRTRAKILL/` and this mod's three files in `BepInEx/patchers/` (`VRPatcher.dll`, `AssetsTools.NET.dll`, `classdata.tpk`). Also delete `BepInEx/VRTRAKILL_update/` if it exists.
3. In Steam, right-click ULTRAKILL > Properties > Installed Files > Verify integrity of game files. This restores the original `globalgamemanagers`, which turns VR off.

The other files the zip added under `ULTRAKILL_Data` can stay. Only the mod starts OpenVR, so without it they are not used.

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

It builds Release, downloads the BepInEx 5.4.23.5 x64 zip from the BepInEx GitHub release, checks its SHA256, and writes `VRTRAKILL-<version>-full.zip`, `VRTRAKILL-<version>-modonly.zip`, `SHA256SUMS.txt` and `SHA256SUMS.txt.sig` to `artifacts/`. The version is `VRTRAKILLVersion` in `Directory.Build.props`, which also sets the plugin and assembly versions. `tools/sign-release.ps1` makes the signature with the release key, which is kept outside the repo. Use `-NoSign` for a local test build; the in-game updater does not install unsigned zips. `tools/backup-signing-key.ps1` backs up the release key to 1Password and restores it on a new Windows profile (`-Restore`).

To make the Thunderstore package (r2modman, Gale), run:

```
pwsh tools/package-thunderstore.ps1 -ULTRAKILLDir "E:\SteamLibrary\steamapps\common\ULTRAKILL"
```

It builds Release and writes `VRTRAKILL-<version>.zip` to `artifacts/thunderstore/`. The version is `VRTRAKILLVersion` in `Directory.Build.props`, and `version_number` in `thunderstore/manifest.json` must match it. The description and BepInExPack dependency come from `thunderstore/manifest.json`. A mod manager can only install into `BepInEx/`, so the package puts the `ULTRAKILL_Data` files in `BepInEx/patchers/<team>-VRTRAKILL/GameData/`. VRPatcher copies them into the game's `ULTRAKILL_Data` at start-up when they are missing or different. With the zip install that folder does not exist and VRPatcher skips the copy.

Everything the mod installs besides its three DLLs is in `ULTRAKILL_COPYTOROOT`, laid out like the game folder.

The Unity project for `vrtrakillassetbundle` is not in this repo. It is `_unityProject.7z` in the `ultra_revamp` branch of the [original repo](https://github.com/whateverusername0/VRTRAKILL/tree/ultra_revamp).

## Credits

- whateverusername0 created VRTRAKILL ([original repo](https://github.com/whateverusername0/VRTRAKILL)). The original README put development on indefinite hold and pointed players to forks for newer game versions. This is one of those forks.
- Squaresweets (Matthew Collier) made the [VRTRAKILL_FRAUD](https://github.com/Squaresweets/VRTRAKILL_FRAUD) fork. This version builds on it.
- Guillaume-Lechevallier: thank you for your [VRTRAKILL_FRAUD fork](https://github.com/Guillaume-Lechevallier/VRTRAKILL_FRAUD), which did the groundwork for the current game build (the Unity 2022.3 VRPatcher, the OpenVR start-up and the portal overlay). Our weapon wheel, desktop view, 4-S and Clash camera, late HUD conversion and opt-in VR body are based on your work, and your notes pointed us at the shop preview problem.
- Graze, Rokic, Party Wumpus and NavaShield contributed to the original mod.
- Raicuparta: the globalgamemanagers patcher approach comes from Two Forks VR (MIT).

Updated for build 22957324 by Motaweh.

## License

AGPL-3.0. See [LICENSE.md](LICENSE.md). Modified by Motaweh, 2026.

Third-party files (BepInEx, UnityDoorstop, SteamVR plugin, OpenVR, Unity XR packages, AssetsTools.NET, Newtonsoft.Json and others) keep their own licenses. See [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).

This is a fan project. It is not made by or connected to New Blood. You need your own copy of ULTRAKILL.
