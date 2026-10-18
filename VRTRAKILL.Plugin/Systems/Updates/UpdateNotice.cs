using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using VRTRAKILL.Utilities;

namespace Plugin.Systems.Updates
{
    // A small text strip at the bottom of the main menu, shown for a few seconds; it takes no input.
    internal static class UpdateNotice
    {
        private const float ShowSeconds = 15f;
        private const float MaxWaitSeconds = 600f;

        // Waits for the main menu, shows the text there for 15 seconds, then removes it; gives up after 10 minutes.
        public static IEnumerator ShowOnMainMenu(string text)
        {
            for (float waited = 0; !IsMainMenuReady(); waited += 1f)
            {
                if (waited > MaxWaitSeconds) yield break;
                yield return new WaitForSecondsRealtime(1f);
            }
            // the menu canvases convert in the frames after the scene loads; add the notice after them
            yield return new WaitForSecondsRealtime(2f);

            GameObject notice = null;
            try { notice = Create(text); }
            catch (Exception e) { Vars.Log.LogWarning("Could not show the update notice: " + e.Message); }
            if (notice == null) yield break;

            yield return new WaitForSecondsRealtime(ShowSeconds);
            if (notice != null) UnityEngine.Object.Destroy(notice);
        }

        // Returns true when the main menu is open and the VR camera exists; GetInstance avoids the one-shot Find of Instance.
        private static bool IsMainMenuReady()
        {
            try
            {
                return MonoSingleton.GetInstance(typeof(OptionsManager)) is OptionsManager O && O != null && O.mainMenu
                    && Vars.MainCamera != null;
            }
            catch (Exception) { return false; }
        }

        // Builds the notice canvas and converts it to a VR canvas like the game menus.
        private static GameObject Create(string text)
        {
            GameObject root = new GameObject("VRTRAKILL Update Notice");
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30000;
            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            GameObject panel = new GameObject("Panel", typeof(RectTransform));
            panel.transform.SetParent(root.transform, false);
            RectTransform panelRect = (RectTransform)panel.transform;
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(.5f, 0);
            panelRect.pivot = new Vector2(.5f, 0);
            panelRect.anchoredPosition = new Vector2(0, 40);
            panelRect.sizeDelta = new Vector2(1300, 70);
            Image background = panel.AddComponent<Image>();
            background.color = new Color(0, 0, 0, .8f);
            background.raycastTarget = false;

            GameObject label = new GameObject("Text", typeof(RectTransform));
            label.transform.SetParent(panel.transform, false);
            RectTransform labelRect = (RectTransform)label.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(20, 0);
            labelRect.offsetMax = new Vector2(-20, 0);
            Text t = label.AddComponent<Text>();
            t.font = FindFont();
            t.fontSize = 30;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.color = Color.white;
            t.raycastTarget = false;
            t.text = text;

            UI.UIConverter.ConvertCanvas(canvas);
            // the UI camera only draws the UI layer, and objects made in code start on Default
            root.RecursiveChangeLayer((int)Layers.UI);
            return root;
        }

        // Returns a font the game already uses, or Unity's built-in runtime font.
        private static Font FindFont()
        {
            foreach (Text existing in Resources.FindObjectsOfTypeAll<Text>())
                if (existing != null && existing.font != null) return existing.font;
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }
}
