using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using VRTRAKILL.Updates;

namespace Plugin.Systems.Updates
{
    // The parts of a GitHub "latest release" response that the update check uses.
    internal sealed class ReleaseInfo
    {
        public enum Verdict { Newer, NotNewer, Prerelease, Invalid }

        public string Tag { get; private set; }
        public ReleaseVersion Version { get; private set; }
        public string PageUrl { get; private set; }
        // asset name to download URL and size in bytes
        public Dictionary<string, KeyValuePair<string, long>> Assets { get; } = new Dictionary<string, KeyValuePair<string, long>>(StringComparer.Ordinal);

        // Parses the response JSON and compares it with the installed version; the reason says why it is not Newer.
        public static Verdict Evaluate(string json, string installedVersion, out ReleaseInfo release, out string reason)
        {
            release = null;
            JObject root;
            try { root = JObject.Parse(json); }
            catch (Exception e) { reason = "the response is not JSON (" + e.GetType().Name + ")"; return Verdict.Invalid; }

            string tag = (string)root["tag_name"];
            ReleaseVersion version = ReleaseVersion.Parse(tag);
            if (version == null) { reason = "the release tag '" + tag + "' is not a version"; return Verdict.Invalid; }

            release = new ReleaseInfo { Tag = tag, Version = version, PageUrl = (string)root["html_url"] ?? UpdateLayout.ReleasesPageUrl };
            if (root["assets"] is JArray assets)
                foreach (JToken asset in assets)
                {
                    string name = (string)asset["name"], url = (string)asset["browser_download_url"];
                    if (name != null && url != null && !release.Assets.ContainsKey(name))
                        release.Assets.Add(name, new KeyValuePair<string, long>(url, asset["size"]?.Value<long>() ?? 0));
                }

            // /releases/latest skips prereleases and drafts; checking the flags and the tag also covers a mislabelled release
            if ((bool?)root["prerelease"] == true || (bool?)root["draft"] == true || version.IsPrerelease)
            { reason = "the latest release " + tag + " is a prerelease"; return Verdict.Prerelease; }

            ReleaseVersion installed = ReleaseVersion.Parse(installedVersion);
            if (installed != null && version.CompareTo(installed) <= 0)
            { reason = "the latest release " + version + " is not newer than " + installedVersion; return Verdict.NotNewer; }

            reason = null;
            return Verdict.Newer;
        }
    }
}
