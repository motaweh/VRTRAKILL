# Changelog

## 1.0.1

- First Thunderstore release.
- VRPatcher now copies the VR game files (SteamVR and Unity XR DLLs, the OpenVR native plugins, the SteamVR bindings and the OpenVR subsystem manifest) into `ULTRAKILL_Data` when they are missing or different. A mod manager cannot put files there, so the mod now works from an r2modman or Gale profile.
- The zip install works as before. When the files in `ULTRAKILL_Data` already match, VRPatcher changes nothing.

## 1.0.0

- Runs on build 22957324. Portal, whiplash, shotgun saw and punch patches are updated for the new game code.
- VRPatcher works with Unity 2022.3 and patches globalgamemanagers again after a game update.
- OpenVR start-up retries if SteamVR is slow to start.
- Menus work with the controller laser and the left stick.
- New default bindings for PS VR2 Sense. Fixed bindings for Touch, Index, Vive wands and WMR.
- Rebinds from the game's controls menu reach the mod again.
- No longer ships Oculus or OpenXR runtime files. The mod uses OpenVR only.
