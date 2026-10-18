# Changelog

## Unreleased

- Menus: Cross on PS VR2 and A on Quest select (the jump button on other controllers). The trigger selects what the laser points at, on either hand. These buttons no longer also jump, shoot or punch in a menu, or when the menu closes while held.
- A small hand turn while you pull the trigger no longer cancels a click on a button in a scrolling list.
- Controls follow the common VR shooter layout (Meta's controller guidance and ULTRAKILL's own gamepad layout). Dash is on the left stick click. Change weapon variation is on Triangle (PS VR2) or Y (Quest). Whiplash is on L1 or the left grip. Change fist (Feedbacker / Knuckleblaster) is on Square or X. The same logic applies to Index, Vive and WMR.
- The right stick only turns. It no longer switches weapons. Use the weapon wheel. In menus the right stick still scrolls.
- One press of Circle (PS VR2) or B (Quest) goes back in menus. In play only a double press pauses. Options still pauses on PS VR2.
- Rockets, cannonballs and napalm now fire from the gun hand.
- After this update, reset SteamVR to the default binding if the buttons still act like before (SteamVR Settings > Controllers > Manage Controller Bindings > ULTRAKILL > Default).

## 1.0.1

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
- New option "Auto update". "Notify" (default) shows a notice in the main menu when a new release is out. "Download" (opt-in) downloads a signed update and installs it at the next start. "Off" turns the check off.
- New option "Game blood and oil stains (experimental)", off by default.
- Thunderstore package for r2modman and Gale. VRPatcher copies the VR game files into `ULTRAKILL_Data` at start-up, so the mod works from a mod manager profile. The zip install works as before.

## 1.0.0

- First release for the current ULTRAKILL on Steam, build 22957324 (Unity 2022.3.29f1). No downgrade needed.
- Portal, whiplash, shotgun saw and punch patches are updated for the new game code.
- Runs through SteamVR (OpenVR) on any SteamVR headset. The mod no longer ships Oculus or OpenXR runtime files.
- OpenVR start-up retries if SteamVR is slow to start.
- Tested on PS VR2. New default bindings for PS VR2 Sense. Fixed bindings for Touch, Index, Vive wands and WMR.
- Menus work with the controller laser and the left stick.
- Rebinds from the game's controls menu reach the mod again.
- VRPatcher works with Unity 2022.3. It turns on VR in globalgamemanagers on the first start and patches it again after a game update.
- Full zip with BepInEx 5.4.23.5, and a mod-only zip.
