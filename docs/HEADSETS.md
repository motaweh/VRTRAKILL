# Headsets

VRTRAKILL runs through SteamVR (OpenVR) only. Every headset must reach the game through SteamVR. The OpenXR runtime that is set as active on the PC (Meta, SteamVR or VDXR) does not matter, because the mod does not use OpenXR.

## Support status

| Headset / controller | SteamVR controller type | Status | Default binding |
|---|---|---|---|
| PlayStation VR2 / PS VR2 Sense controllers | `playstation_vr2_sense` | Tested on hardware | `bindings_playstation_vr2_sense.json` |
| Meta Quest 2, 3, 3S, Pro / Meta Quest Touch controllers (through SteamVR) | `oculus_touch` | Tested on hardware | `bindings_oculus_touch.json` |
| Valve Index / Index controllers (also Virtual Desktop with "Emulate Index controllers") | `knuckles` | Configured, not tested | `bindings_knuckles.json` |
| HTC Vive, Vive Pro / Vive wands | `vive_controller` | Configured, not tested | `bindings_vive_controller.json` |
| Windows Mixed Reality / motion controllers | `holographic_controller` | Configured, not tested | `bindings_holographic_controller.json` |
| HTC Vive Cosmos controllers | `vive_cosmos_controller` | Not supported | file is an empty stub |
| Logitech VR Ink stylus | `logitech_stylus` | Not supported | file is an empty stub |
| Hand tracking (any headset) | none | Not supported | none |
| OpenXR-only runtimes without SteamVR | none | Not supported | none |

"Configured" means the default binding covers every game action and passes a check against the SteamVR driver's input profile. Nobody has played the game with it.

### What works

- PS VR2 Sense controllers and Meta Quest Touch controllers: played on this game build through SteamVR. Head and hand tracking, the actions in the button map below and the menus with the controller laser are in use.
- Configured controllers: the same actions are bound, so the same features are expected. Change any binding in SteamVR if one does not suit you.

## How to launch

### PlayStation VR2
1. Connect the PC adapter and start the PlayStation VR2 App.
2. Start SteamVR.
3. Start ULTRAKILL from Steam.

### Quest with Link (cable)
1. Install the Meta Horizon Link app from https://www.meta.com/quest/setup/ (Quest Link (PC), Download software) and sign in with your Meta account.
2. Connect the headset with a USB-C cable that supports USB 3.
3. In the headset, open Quick Settings, then Link, and accept.
4. Start SteamVR on the PC. Keep it running.
5. Start ULTRAKILL from Steam.

Air Link (the same steps over Wi-Fi), Virtual Desktop and Steam Link below are not tested with this mod.

The game uses OpenVR, so it always goes through SteamVR, even if Meta is the active OpenXR runtime.

### Quest with Virtual Desktop
1. Start the Virtual Desktop Streamer on the PC and connect from the headset.
2. Start SteamVR from the Virtual Desktop Games tab, or from Steam.
3. Start ULTRAKILL from Steam.

VDXR (Virtual Desktop's OpenXR runtime) is not used. "Emulate Index controllers" is optional. When it is on, the Index button map applies.

### Quest with Steam Link
1. Install Steam Link on the Quest and connect it to the PC.
2. SteamVR starts with the connection.
3. Start ULTRAKILL from Steam.

### Valve Index
1. Start SteamVR.
2. Start ULTRAKILL from Steam.

### Refresh rate
The mod does not set or assume a refresh rate. Set 72, 80, 90 or 120 Hz in the Link, Virtual Desktop or Steam Link settings, or in SteamVR for PS VR2 and Index.

## Button maps

`(R)` is the right controller, `(L)` is the left controller. "Double" is a fast double press. "Held" is a long press. Change any of these in SteamVR > Settings > Controllers > Manage Controller Bindings > ULTRAKILL.

| Action | Quest Touch | PS VR2 Sense | Index | Vive wand | WMR |
|---|---|---|---|---|---|
| Move | (L) stick | (L) stick | (L) thumbstick | (L) trackpad touch | (L) stick |
| Turn | (R) stick left / right | (R) stick left / right | (R) thumbstick left / right | (R) trackpad touch left / right | (R) stick left / right |
| Jump | (R) A | (R) cross | (R) A | (R) trackpad click center or up | (R) trackpad click |
| Slide / slam | (R) stick click | (R) stick click | (R) thumbstick click | (R) trackpad click down | (R) stick click |
| Dash | (L) stick click | (L) stick click | (L) thumbstick click | (L) trackpad click center, down, left or right | (L) stick click |
| Shoot | (R) trigger | (R) R2 | (R) trigger | (R) trigger | (R) trigger |
| Alternate fire | (R) grip | (R) R1 | (R) grip | (R) grip | (R) grip |
| Punch | (L) trigger | (L) L2 | (L) trigger | (L) trigger | (L) trigger |
| Whiplash | (L) grip | (L) L1 | (L) grip | (L) grip | (L) grip |
| Change fist (Feedbacker / Knuckleblaster) | (L) X | (L) square | (L) A | (L) menu | (L) trackpad click down |
| Change weapon variation | (L) Y | (L) triangle | (L) B | (L) trackpad click up | (L) trackpad click up |
| Weapon wheel | (R) B held | (R) circle held | (R) B held | (R) menu held | (R) menu held |
| Weapon wheel scroll | (L) stick | (L) stick | (L) thumbstick | (L) trackpad touch | (L) stick |
| Pause | (R) B double | (R) Options, or (R) circle double | (R) B double | (R) menu double | (L) menu, or (R) menu double |
| Menu select | (R) A, or a trigger | (R) cross, or R2 / L2 | (R) A, or a trigger | (R) trackpad click center or up, or a trigger | (R) trackpad click, or a trigger |
| Menu highlight | (L) stick | (L) stick | (L) thumbstick | (L) trackpad touch | (L) stick |
| Menu scroll | (R) stick up / down | (R) stick up / down | (R) thumbstick up / down | (R) trackpad touch up / down | (R) stick up / down |
| Menu back | (R) B once | (R) circle once | (R) B once | (R) menu once | (R) menu once |

This layout follows the common VR shooter layout: Meta's controller guidance and ULTRAKILL's own gamepad layout. No button switches to the next or previous weapon by default. Use the weapon wheel. PS VR2 Create is not bound, because SteamVR uses it for screenshots.

The jump button jumps in game and selects in menus. In menus, point with either controller: its trigger selects what the laser points at. The left stick moves the highlight and the jump button selects the highlighted item. In menus the jump and trigger buttons do not jump, shoot or punch, also when the menu closes while you hold them. In a shop, the jump button on a shop button clicks it; anywhere else it jumps. "Change weapon variation" only changes the variation.

A single press of the weapon wheel button goes back one menu page. On the first pause page it closes the pause menu, like Escape on a keyboard. In play a single press does nothing, so the button only pauses on a double press. In a menu a double press goes back once. A single press counts after the double press time has passed, so back comes a moment after the press.

A push of the turn stick that is more up or down than left or right does not turn.

While the weapon wheel is open, the weapon wheel scroll stick points at a weapon and does not move the player. Let go of the wheel button to switch.

If the buttons still act like an older version, reset SteamVR to the default binding (SteamVR Settings > Controllers > Manage Controller Bindings > ULTRAKILL > Default).

## Known issues

- PS VR2 and Quest are tested on hardware. Index, Vive and WMR bindings are checked against driver input profiles only.
- Quest: the (L) menu button opens the SteamVR menu. Games cannot use it, so pause is a double tap of (R) B.
- WMR: the binding was checked against Valve's WMR example binding and ALVR's WMR controller profile, not against Microsoft's own driver profile.
- The weapon wheel button presses the key the game uses for "Last Used Weapon". The game has no keyboard key for it by default, so the mod presses the "Previous Variation" key (Q) instead, which opens the wheel when held. Bind a keyboard key to "Last Used Weapon" in the game's controls to get quick swap.
- Left-handed mode is marked as broken in the config file ("Left handed (BROKEN)").
- After an update that changes a default binding, SteamVR can keep the old one. Select the default binding again in the binding screen.
- Issues in the game itself (portals, stains, 4-S, desktop view) are listed in the [README](../README.md#known-issues).

## Upgrading an older install

Extract the new zip over the old install. Files from older versions can stay.

If SteamVR keeps an old binding, open the binding screen above and select the default binding again.
