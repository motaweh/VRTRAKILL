using HarmonyLib;
using Plugin.Systems;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace Plugin.Patches.ULTRAKILL;

[HarmonyPatch(typeof(StainVoxelManager))] internal class PatchStains
{
}