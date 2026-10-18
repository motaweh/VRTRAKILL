using System;
using System.IO;
using System.Text.RegularExpressions;

namespace VRTRAKILL.Updates
{
    // The update modes from the "Auto update" config setting.
    internal enum UpdateMode { Off, Notify, Download }

    // File names, folders and path rules shared by the update check (plugin) and the update install (VRPatcher).
    internal static class UpdateLayout
    {
        public const string LatestReleaseUrl = "https://api.github.com/repos/motaweh/VRTRAKILL/releases/latest";
        public const string ReleasesPageUrl = "https://github.com/motaweh/VRTRAKILL/releases";
        public const string SumsName = "SHA256SUMS.txt";
        public const string SignatureName = "SHA256SUMS.txt.sig";
        public const string ReadyName = "ready.txt";
        public const string ConfigRelativePath = "BepInEx/plugins/VRTRAKILL/VRTRAKILL_Config.json";

        // the zip root is the game folder; an update may only write below these folders
        private static readonly string[] AllowedPrefixes =
        {
            "BepInEx/plugins/VRTRAKILL/",
            "BepInEx/patchers/",
            "ULTRAKILL_Data/Managed/",
            "ULTRAKILL_Data/Plugins/x86_64/",
            "ULTRAKILL_Data/StreamingAssets/SteamVR/",
            "ULTRAKILL_Data/UnitySubsystems/",
        };

        // Returns the mod-only zip name for a version.
        public static string ZipName(string version) => "VRTRAKILL-" + version + "-modonly.zip";

        // Returns BepInEx/VRTRAKILL_update.
        public static string UpdateRoot(string bepInExRoot) => Path.Combine(bepInExRoot, "VRTRAKILL_update");
        // Returns the folder that holds a downloaded, verified update.
        public static string StagingDir(string bepInExRoot) => Path.Combine(UpdateRoot(bepInExRoot), "staging");
        // Returns the folder that holds the files of an install in progress.
        public static string ExtractDir(string bepInExRoot) => Path.Combine(UpdateRoot(bepInExRoot), "extract");
        // Returns the folder that keeps the files an update replaced.
        public static string BackupDir(string bepInExRoot, string oldVersion) => Path.Combine(Path.Combine(UpdateRoot(bepInExRoot), "backup"), oldVersion);
        // Returns the file that records the last install result.
        public static string ResultPath(string bepInExRoot) => Path.Combine(UpdateRoot(bepInExRoot), "last_update.txt");
        // Returns the file that marks a version whose install failed, so it is not downloaded again.
        public static string FailedMarkerPath(string bepInExRoot, string version) => Path.Combine(UpdateRoot(bepInExRoot), "failed_" + version + ".txt");

        // Returns the zip entry as a safe game-relative path with '/' separators, or null with a reason if it is not allowed.
        public static string NormalizeEntry(string entryName, out string reason)
        {
            reason = null;
            if (string.IsNullOrEmpty(entryName)) { reason = "empty name"; return null; }
            string p = entryName.Replace('\\', '/');
            if (p.StartsWith("/") || p.IndexOf(':') >= 0) { reason = "absolute path or drive"; return null; }
            foreach (char c in Path.GetInvalidPathChars()) if (p.IndexOf(c) >= 0) { reason = "invalid character"; return null; }
            string[] segments = p.Split('/');
            for (int i = 0; i < segments.Length; i++)
            {
                string segment = segments[i];
                if (segment == "." || segment == "..") { reason = "relative segment"; return null; }
                // only a directory entry ("dir/") may end with an empty segment
                if (segment.Length == 0 && i != segments.Length - 1) { reason = "empty segment"; return null; }
                if (segment.TrimEnd(' ', '.') != segment) { reason = "segment ends with a dot or space"; return null; }
            }
            foreach (string prefix in AllowedPrefixes)
                if (p.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && p.Length > prefix.Length)
                    return p;
            reason = "outside the mod folders";
            return null;
        }

        // Returns the full path of a game-relative path, or throws if it would leave the game folder.
        public static string ResolveUnder(string gameRoot, string relativePath)
        {
            string root = Path.GetFullPath(gameRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string full = Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
            if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Path leaves the game folder: " + relativePath);
            return full;
        }

        // Reads the "Auto update" value from the config JSON text; a missing or unknown value means Notify.
        public static UpdateMode ParseMode(string value)
        {
            if (string.Equals(value, "Off", StringComparison.OrdinalIgnoreCase)) return UpdateMode.Off;
            if (string.Equals(value, "Download", StringComparison.OrdinalIgnoreCase)) return UpdateMode.Download;
            return UpdateMode.Notify;
        }

        // Reads the "Auto update" mode from the config file text without a JSON library (VRPatcher has none).
        public static UpdateMode ReadModeFromConfigText(string json)
        {
            if (json == null) return UpdateMode.Notify;
            Match m = Regex.Match(json, "\"Auto update\"\\s*:\\s*\"([^\"]*)\"");
            return m.Success ? ParseMode(m.Groups[1].Value) : UpdateMode.Notify;
        }
    }
}
