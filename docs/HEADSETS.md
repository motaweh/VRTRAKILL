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

### Finger tracking

Finger tracking is off by default. Set "Finger tracking" to true under "Controller Settings" in `VRTRAKILL_Config.json` to turn it on. Not yet confirmed on hardware.

- Only the punch arm (Feedbacker and Knuckleblaster) follows the finger curls, and only while it idles. A punch, a held item and the shop use the game's own animation.
- Weapon grips, the whiplash and the sandbox arm keep their fixed poses.
- A controller with no skeleton data, or with the skeleton not bound, keeps the fixed pose.
- The log shows "Finger tracking: left hand skeleton data received." when data arrives.

| Controller | Skeleton input in driver profile | Skeleton bound in default binding |
|---|---|---|
| PS VR2 Sense controllers | yes | yes |
| Index | yes | yes |
| Meta Quest Touch controllers | yes | yes |
| Vive wand | yes | yes |
| WMR | not checked (driver profile not available) | no |

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
| Jump | (R) A | (R) cross | (L) thumbstick click | (L) trackpad click up | (L) stick click |
| Turn | (R) stick left / right | (R) stick left / right | (R) thumbstick left / right | (R) trackpad touch left / right | (R) stick left / right |
| Slide / slam | (R) stick click | (R) stick click | (R) thumbstick click | (R) trackpad click down | (R) stick click |
| Dash | (L) Y | (L) triangle | (L) B | (L) trackpad click down, left or right | (L) trackpad click up |
| Shoot | (R) trigger | (R) R2 | (R) trigger | (R) trigger | (R) trigger |
| Alternate fire | (R) grip | (R) R1 | (R) grip | (R) grip | (R) grip |
| Punch | (L) trigger | (L) L2 | (L) trigger | (L) trigger | (L) trigger |
| Whiplash | (L) X | (L) square | (L) A | (L) menu | (L) trackpad click left |
| Change hand | (L) grip | (L) L1 | (L) grip | (L) grip | (L) grip |
| Change weapon variation | (L) stick click | (L) stick click | (R) A | (R) trackpad click up | (R) trackpad click down |
| Next / previous weapon | (R) stick flick up / down | (R) stick flick up / down | (R) thumbstick flick up / down | not bound | (R) stick flick up / down |
| Weapon wheel | (R) B held | (R) circle held | (R) B held | (R) menu held | (R) menu held |
| Weapon wheel scroll | (L) stick | (L) stick | (L) thumbstick | (L) trackpad touch | (L) stick |
| Pause / back | (R) B double | (R) Options, or (R) circle double | (R) B double | (R) menu double | (L) menu, or (R) menu double |

In menus, point with either controller and press its trigger to click. The left stick moves the selection and the "Change weapon variation" button selects it (left stick click on Quest and PS VR2, (R) A on Index).

One flick of the turn stick up or down switches one weapon. Return the stick to the center before the next flick. A push that is more up or down than left or right does not turn.

While the weapon wheel is open, the weapon wheel scroll stick points at a weapon and does not move the player. Let go of the wheel button to switch.

## Known issues

- PS VR2 and Quest are tested on hardware. Index, Vive and WMR bindings are checked against driver input profiles only.
- Quest: the (L) menu button opens the SteamVR menu. Games cannot use it, so pause is a double tap of (R) B.
- Vive wand: next / previous weapon is not bound. The right trackpad already turns, and a scroll there would also fire on every up or down click. Use the weapon wheel.
- WMR: the binding was checked against Valve's WMR example binding and ALVR's WMR controller profile, not against Microsoft's own driver profile.
- The weapon wheel button presses the key the game uses for "Last Used Weapon". The game has no keyboard key for it by default, so the mod presses the "Previous Variation" key (Q) instead, which opens the wheel when held. On Index a short press therefore changes to the previous variation. Bind a keyboard key to "Last Used Weapon" in the game's controls to get quick swap on a short press.
- Left-handed mode is marked as broken in the config file ("Left handed (BROKEN)").
- After an update that changes a default binding, SteamVR can keep the old one. Select the default binding again in the binding screen.
- Issues in the game itself (portals, stains, 4-S, desktop view) are listed in the [README](../README.md#known-issues).

## Upgrading an older install

Extract the new zip over the old install. Files from older versions can stay.

If SteamVR keeps an old binding, open the binding screen above and select the default binding again.
