# Headsets

VRTRAKILL runs through **SteamVR (OpenVR)** only. Every headset must reach the game through SteamVR. The OpenXR runtime that is set as active on the PC (Meta, SteamVR or VDXR) does not matter, because the mod does not use OpenXR.

## Supported headsets

| Headset | Connection | SteamVR controller type | Default binding |
|---|---|---|---|
| PlayStation VR2 | PlayStation VR2 App + SteamVR | `playstation_vr2_sense` | `bindings_playstation_vr2_sense.json` |
| Meta Quest 2, 3, 3S, Pro | Quest Link (cable) or Air Link | `oculus_touch` (SteamVR `oculus` driver) | `bindings_oculus_touch.json` |
| Meta Quest 2, 3, 3S, Pro | Steam Link | `oculus_touch` (SteamVR `vrlink` driver) | `bindings_oculus_touch.json` |
| Meta Quest 2, 3, 3S, Pro | Virtual Desktop (SteamVR mode) | `oculus_touch`, or `knuckles` with "Emulate Index controllers" on | `bindings_oculus_touch.json` / `bindings_knuckles.json` |
| Valve Index | SteamVR | `knuckles` | `bindings_knuckles.json` |
| HTC Vive, WMR | SteamVR | `vive_controller`, `holographic_controller` | community bindings, not tested |

Hand tracking is not supported. Use controllers.

## How to launch

### PlayStation VR2
1. Connect the PC adapter and start the **PlayStation VR2 App**.
2. Start **SteamVR**.
3. Start ULTRAKILL from Steam.

### Quest with Quest Link or Air Link
1. Start the **Meta Quest Link** app on the PC.
2. In the headset, turn on Link or Air Link.
3. Start **SteamVR** (from Steam on the desktop, or from the Link desktop). Keep it running.
4. Start ULTRAKILL from Steam.

The game uses OpenVR, so it always goes through SteamVR, even if Meta is the active OpenXR runtime.

### Quest with Virtual Desktop
1. Start the **Virtual Desktop Streamer** on the PC and connect from the headset.
2. Start **SteamVR** from the Virtual Desktop Games tab, or from Steam.
3. Start ULTRAKILL from Steam.

VDXR (Virtual Desktop's OpenXR runtime) is not used. "Emulate Index controllers" is optional. When it is on, the Index button map applies.

### Quest with Steam Link
1. Install **Steam Link** on the Quest and connect it to the PC.
2. SteamVR starts with the connection.
3. Start ULTRAKILL from Steam.

### Valve Index
1. Start **SteamVR**.
2. Start ULTRAKILL from Steam.

### Refresh rate
The mod does not set or assume a refresh rate. Set 72, 80, 90 or 120 Hz in the Link, Virtual Desktop or Steam Link settings, or in SteamVR for PS VR2 and Index.

## Button maps

`(R)` is the right controller, `(L)` is the left controller. "Double" is a fast double press. "Held" is a long press. Change any of these in **SteamVR > Settings > Controllers > Manage Controller Bindings > ULTRAKILL**.

| Action | Quest Touch | PS VR2 Sense | Index |
|---|---|---|---|
| Move | (L) stick | (L) stick | (L) thumbstick |
| Jump | (L) stick click | (L) stick click | (L) thumbstick click |
| Turn | (R) stick | (R) stick | (R) thumbstick |
| Slide / slam | (R) stick click | (R) stick click | (R) thumbstick click |
| Dash | (L) Y | (L) triangle | (L) B |
| Shoot | (R) trigger | (R) R2 | (R) trigger |
| Alternate fire | (R) grip | (R) R1 | (R) grip |
| Punch | (L) trigger | (L) L2 | (L) trigger |
| Whiplash | (L) X | (L) square | (L) A |
| Change hand | (L) grip | (L) L1 | (L) grip |
| Change weapon variation | (R) A | (R) cross | (R) A |
| Next / previous weapon | (R) stick | (R) stick | (R) thumbstick |
| Weapon wheel | (R) B held | (R) circle held | (R) B held |
| Weapon wheel scroll | (L) stick | (L) stick | (L) thumbstick |
| Pause / back | (R) B double | (R) Options, or (R) circle double | (R) B double |

In menus, point with either controller and press its trigger to click. The left stick moves the selection and the "Change weapon variation" button selects it.

## Upgrading an older install

Earlier versions copied files that the mod does not use. You can delete them from `ULTRAKILL_Data`:

- `Plugins/x86_64/OVRPlugin.dll`
- `Plugins/x86_64/AudioPluginOculusSpatializer.dll`
- `Plugins/x86_64/UnityOpenXR.dll`
- `Plugins/x86_64/openxr_loader.dll`
- `UnitySubsystems/UnityOpenXR/` (the folder)
- `Managed/Unity.XR.OpenXR*.dll`

If SteamVR keeps an old binding, open the binding screen above and select the default binding again.
