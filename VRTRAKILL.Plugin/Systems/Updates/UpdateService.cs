using System;
using System.Collections;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using VRTRAKILL.Updates;

namespace Plugin.Systems.Updates
{
    // Once per game start: asks GitHub for the latest release, shows a notice when it is newer and, in Download mode, stages it for VRPatcher.
    internal static class UpdateService
    {
        private const int CheckTimeoutSeconds = 10;
        private const int DownloadTimeoutSeconds = 300;
        private const long MaxZipBytes = 256L * 1024 * 1024;
        private const float StartDelaySeconds = 5f;
        private static bool Started;

        // Starts the background update check unless it already ran or "Auto update" is Off; never throws.
        public static void Begin(MonoBehaviour host)
        {
            try
            {
                if (Started) return;
                Started = true;
                UpdateMode mode = UpdateLayout.ParseMode(Vars.Config.AutoUpdate);
                if (mode == UpdateMode.Off) { Vars.Log.LogInfo("Update check is off (\"Auto update\": \"Off\")."); return; }
                host.StartCoroutine(Run(host, mode));
            }
            catch (Exception e) { Vars.Log.LogWarning("Update check not started: " + e.Message); }
        }

        // Runs the check after a short delay so it never competes with startup, then notifies or downloads.
        private static IEnumerator Run(MonoBehaviour host, UpdateMode mode)
        {
            yield return new WaitForSecondsRealtime(StartDelaySeconds);
            string installed = PluginInfo.PLUGIN_VERSION;

            string json = null, error = null;
            long status = 0;
            using (UnityWebRequest request = UnityWebRequest.Get(UpdateLayout.LatestReleaseUrl))
            {
                request.timeout = CheckTimeoutSeconds;
                request.SetRequestHeader("User-Agent", "VRTRAKILL/" + installed);
                request.SetRequestHeader("Accept", "application/vnd.github+json");
                yield return request.SendWebRequest();
                status = request.responseCode;
                if (request.result == UnityWebRequest.Result.Success) json = request.downloadHandler.text;
                else error = request.error;
            }
            if (json == null && status == 404)
            {
                // /releases/latest leaves out prereleases, so this is the answer while only prereleases exist
                Vars.Log.LogInfo("No update: GitHub lists no stable VRTRAKILL release yet.");
                yield break;
            }
            if (json == null)
            {
                // offline, blocked or rate limited: skip quietly until the next game start
                Vars.Log.LogInfo("Update check skipped: " + error + (status > 0 ? " (HTTP " + status + ")" : "") + ".");
                yield break;
            }

            ReleaseInfo release = null;
            string reason = null;
            ReleaseInfo.Verdict verdict;
            try { verdict = ReleaseInfo.Evaluate(json, installed, out release, out reason); }
            catch (Exception e) { verdict = ReleaseInfo.Verdict.Invalid; reason = e.GetType().Name + ": " + e.Message; }
            if (verdict != ReleaseInfo.Verdict.Newer)
            {
                Vars.Log.LogInfo("No update: " + reason + ".");
                yield break;
            }

            string version = release.Version.ToString();
            Vars.Log.LogMessage("VRTRAKILL " + version + " is available (installed: " + installed + "). See " + UpdateLayout.ReleasesPageUrl);
            if (mode != UpdateMode.Download)
            {
                host.StartCoroutine(UpdateNotice.ShowOnMainMenu("VRTRAKILL " + version + " is available (installed: " + installed + "). See GitHub releases."));
                yield break;
            }

            bool[] staged = new bool[1];
            yield return Stage(release, version, staged);
            host.StartCoroutine(UpdateNotice.ShowOnMainMenu(staged[0]
                ? "VRTRAKILL " + version + " is downloaded and installs when you restart the game."
                : "VRTRAKILL " + version + " is available (installed: " + installed + "). See GitHub releases."));
        }

        // Downloads the mod-only zip, SHA256SUMS.txt and its signature into staging and verifies them; sets staged[0] on success.
        private static IEnumerator Stage(ReleaseInfo release, string version, bool[] staged)
        {
            string bepInEx = BepInEx.Paths.BepInExRootPath;
            string staging = UpdateLayout.StagingDir(bepInEx);
            string zipName = UpdateLayout.ZipName(version);

            string failedMarker = UpdateLayout.FailedMarkerPath(bepInEx, version);
            if (File.Exists(failedMarker))
            {
                Vars.Log.LogWarning("Not downloading " + version + " again: an earlier install failed. See " + failedMarker);
                yield break;
            }
            if (ReadyVersion(staging) == version)
            {
                Vars.Log.LogInfo("VRTRAKILL " + version + " is already downloaded; it installs at the next start.");
                staged[0] = true;
                yield break;
            }

            foreach (string name in new[] { zipName, UpdateLayout.SumsName, UpdateLayout.SignatureName })
                if (!release.Assets.ContainsKey(name))
                {
                    Vars.Log.LogWarning("Release " + release.Tag + " has no " + name + "; not downloading it. Install it by hand from " + release.PageUrl);
                    yield break;
                }
            if (release.Assets[zipName].Value > MaxZipBytes)
            {
                Vars.Log.LogWarning(zipName + " is larger than " + MaxZipBytes + " bytes; not downloading it.");
                yield break;
            }

            if (!TryResetDir(staging)) yield break;
            Vars.Log.LogInfo("Downloading VRTRAKILL " + version + " into " + staging);
            foreach (string name in new[] { UpdateLayout.SumsName, UpdateLayout.SignatureName, zipName })
            {
                string[] error = new string[1];
                yield return Download(release.Assets[name].Key, Path.Combine(staging, name), error);
                if (error[0] != null)
                {
                    Vars.Log.LogWarning("Download of " + name + " failed: " + error[0] + ". Trying again at the next game start.");
                    TryDeleteDir(staging);
                    yield break;
                }
            }

            // hashing a large zip on the main thread could drop frames in VR
            string zipPath = null, reason = null;
            Thread worker = new Thread(() =>
            {
                try { zipPath = ReleaseVerifier.VerifyStaged(staging, version, out reason); }
                catch (Exception e) { reason = e.GetType().Name + ": " + e.Message; }
            }) { IsBackground = true, Name = "VRTRAKILL update verify" };
            worker.Start();
            while (worker.IsAlive) yield return null;
            worker.Join(); // already finished; makes its results visible to this thread

            if (zipPath == null)
            {
                Vars.Log.LogError("The downloaded update " + version + " failed verification and was deleted: " + reason);
                TryDeleteDir(staging);
                yield break;
            }
            try
            {
                File.WriteAllText(Path.Combine(staging, UpdateLayout.ReadyName), "version=" + version + "\nzip=" + zipName + "\n");
                staged[0] = true;
                Vars.Log.LogMessage("VRTRAKILL " + version + " is downloaded and verified; it installs at the next game start.");
            }
            catch (Exception e)
            {
                Vars.Log.LogWarning("Could not finish staging the update: " + e.Message);
                TryDeleteDir(staging);
            }
        }

        // Downloads a URL straight to a file; error[0] is null on success.
        private static IEnumerator Download(string url, string path, string[] error)
        {
            using (UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET))
            {
                request.downloadHandler = new DownloadHandlerFile(path) { removeFileOnAbort = true };
                request.timeout = DownloadTimeoutSeconds;
                request.SetRequestHeader("User-Agent", "VRTRAKILL/" + PluginInfo.PLUGIN_VERSION);
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                    error[0] = request.error + (request.responseCode > 0 ? " (HTTP " + request.responseCode + ")" : "");
            }
        }

        // Returns the version in staging/ready.txt, or null.
        private static string ReadyVersion(string staging)
        {
            try
            {
                string ready = Path.Combine(staging, UpdateLayout.ReadyName);
                if (!File.Exists(ready)) return null;
                foreach (string line in File.ReadAllLines(ready))
                    if (line.StartsWith("version=", StringComparison.Ordinal)) return line.Substring(8).Trim();
            }
            catch (Exception) { }
            return null;
        }

        // Deletes and recreates the staging folder; returns false and logs on failure.
        private static bool TryResetDir(string dir)
        {
            try
            {
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
                Directory.CreateDirectory(dir);
                return true;
            }
            catch (Exception e) { Vars.Log.LogWarning("Could not prepare " + dir + ": " + e.Message); return false; }
        }

        // Deletes a folder if it exists; ignores errors.
        private static void TryDeleteDir(string dir)
        {
            try { if (Directory.Exists(dir)) Directory.Delete(dir, true); } catch (Exception) { }
        }
    }
}
