using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using VRTRAKILL.Updates;

namespace VRPatcher
{
    // Installs a downloaded and verified VRTRAKILL update before the plugins load; Patcher.Finish runs it.
    public static class UpdateApplier
    {
        public enum Result { NothingStaged, Disabled, NotNewer, Rejected, Applied, RolledBack }

        // one file of the update: where it was unpacked, where it goes, and where the old copy was moved
        private sealed class FileStep
        {
            public string Relative, Unpacked, Target, Backup;
            public bool BackedUp, Placed;
        }

        // Applies the staged update for the running game and logs every step; never throws.
        public static void Run()
        {
            BepInEx.Logging.ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("VRTRAKILL Update");
            try
            {
                // a mod manager profile has its own BepInEx folder; Apply writes the zip layout under the game folder, so it only runs for the zip install
                if (!string.Equals(Path.GetFullPath(BepInEx.Paths.BepInExRootPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), Path.GetFullPath(Path.Combine(BepInEx.Paths.GameRootPath, "BepInEx")).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
                { log.LogInfo("BepInEx runs from a mod manager profile; the update installer is skipped."); return; }
                Apply(BepInEx.Paths.GameRootPath, BepInEx.Paths.BepInExRootPath, VRTRAKILL.BuildVersion.Value,
                      (message, warning) => { if (warning) log.LogWarning(message); else log.LogInfo(message); });
            }
            catch (Exception e) { log.LogError("Update install stopped: " + e); }
            finally { BepInEx.Logging.Logger.Sources.Remove(log); }
        }

        // Verifies and installs the staged update into gameRoot; public so tests can run it on a copy of the game layout.
        public static Result Apply(string gameRoot, string bepInExRoot, string currentVersion, Action<string, bool> log)
        {
            string staging = UpdateLayout.StagingDir(bepInExRoot);
            string extract = UpdateLayout.ExtractDir(bepInExRoot);
            DeleteDir(extract, log); // left over from an install that was cut off
            if (!Directory.Exists(staging)) return Result.NothingStaged;

            string readyPath = Path.Combine(staging, UpdateLayout.ReadyName);
            if (!File.Exists(readyPath))
            {
                log("The update download is incomplete; deleting it.", true);
                DeleteDir(staging, log);
                return Result.Rejected;
            }
            string stagedVersion = ReadReadyVersion(readyPath);
            log("Found a downloaded update: VRTRAKILL " + stagedVersion + " (installed: " + currentVersion + ").", false);

            string configPath = UpdateLayout.ResolveUnder(gameRoot, UpdateLayout.ConfigRelativePath);
            UpdateMode mode = UpdateLayout.ReadModeFromConfigText(File.Exists(configPath) ? File.ReadAllText(configPath) : null);
            if (mode != UpdateMode.Download)
            {
                log("\"Auto update\" is " + mode + ", not Download; deleting the downloaded update without installing it.", true);
                DeleteDir(staging, log);
                return Result.Disabled;
            }

            ReleaseVersion staged = ReleaseVersion.Parse(stagedVersion), current = ReleaseVersion.Parse(currentVersion);
            if (staged == null || staged.IsPrerelease || current == null || staged.CompareTo(current) <= 0)
            {
                log("The downloaded update " + stagedVersion + " is not a newer release than " + currentVersion + "; deleting it.", false);
                DeleteDir(staging, log);
                return Result.NotNewer;
            }
            string version = staged.ToString();

            string reason;
            string zipPath = ReleaseVerifier.VerifyStaged(staging, version, out reason);
            if (zipPath == null)
            {
                Fail(bepInExRoot, version, "Rejected the downloaded update " + version + ": " + reason + ". Nothing was changed.", log);
                DeleteDir(staging, log);
                return Result.Rejected;
            }
            log("Signature and SHA-256 of " + Path.GetFileName(zipPath) + " are valid.", false);

            List<FileStep> steps;
            try { steps = Unpack(zipPath, gameRoot, extract, log); }
            catch (Exception e)
            {
                Fail(bepInExRoot, version, "Unpacking the update failed: " + e.Message.TrimEnd('.') + ". Nothing was changed.", log);
                DeleteDir(extract, log);
                DeleteDir(staging, log);
                return Result.Rejected;
            }

            string backup = UniqueDir(UpdateLayout.BackupDir(bepInExRoot, currentVersion));
            log("Installing " + steps.Count + " changed files; replaced files go to " + backup + ".", false);
            Result result = Commit(steps, gameRoot, backup, log) ? Result.Applied : Result.RolledBack;

            if (result == Result.Applied)
            {
                WriteText(UpdateLayout.ResultPath(bepInExRoot),
                    "Installed VRTRAKILL " + version + " over " + currentVersion + " at " + DateTime.UtcNow.ToString("u") + "\n" +
                    "Replaced files: " + steps.Count + "\nBackup: " + backup + "\n");
                TryDelete(UpdateLayout.FailedMarkerPath(bepInExRoot, version));
                log("VRTRAKILL " + version + " is installed. VRPatcher changes take effect at the next start.", false);
            }
            else Fail(bepInExRoot, version, "Installing " + version + " failed; the previous files were restored.", log);

            DeleteDir(extract, log);
            DeleteDir(staging, log);
            return result;
        }

        // Unpacks every changed file of the zip into the extract folder and returns the files to install.
        private static List<FileStep> Unpack(string zipPath, string gameRoot, string extract, Action<string, bool> log)
        {
            List<FileStep> steps = new List<FileStep>();
            int unchanged = 0;
            Directory.CreateDirectory(extract);
            using (ZipArchive zip = ZipFile.OpenRead(zipPath))
            {
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    string reason;
                    string relative = UpdateLayout.NormalizeEntry(entry.FullName, out reason);
                    if (relative == null) throw new InvalidDataException("zip entry '" + entry.FullName + "' is not allowed: " + reason);
                    if (relative.EndsWith("/")) continue;

                    string target = UpdateLayout.ResolveUnder(gameRoot, relative);
                    if (string.Equals(relative, UpdateLayout.ConfigRelativePath, StringComparison.OrdinalIgnoreCase) && File.Exists(target))
                    {
                        log("Keeping the current " + relative + ".", false);
                        continue;
                    }

                    string unpacked = UpdateLayout.ResolveUnder(extract, relative);
                    Directory.CreateDirectory(Path.GetDirectoryName(unpacked));
                    entry.ExtractToFile(unpacked, false);
                    if (new FileInfo(unpacked).Length != entry.Length) throw new InvalidDataException("zip entry '" + relative + "' unpacked to the wrong size");

                    if (File.Exists(target) && ReleaseVerifier.Sha256Hex(target) == ReleaseVerifier.Sha256Hex(unpacked)) { unchanged++; continue; }
                    steps.Add(new FileStep { Relative = relative, Unpacked = unpacked, Target = target });
                }
            }
            steps.Sort((a, b) => string.CompareOrdinal(a.Relative, b.Relative));
            log("Unpacked the update: " + steps.Count + " changed, " + unchanged + " unchanged.", false);
            return steps;
        }

        // Moves each old file to the backup and each new file into place; on any failure restores everything and returns false.
        private static bool Commit(List<FileStep> steps, string gameRoot, string backup, Action<string, bool> log)
        {
            FileStep current = null;
            try
            {
                foreach (FileStep step in steps)
                {
                    current = step;
                    Directory.CreateDirectory(Path.GetDirectoryName(step.Target));
                    if (File.Exists(step.Target))
                    {
                        step.Backup = UpdateLayout.ResolveUnder(backup, step.Relative);
                        Directory.CreateDirectory(Path.GetDirectoryName(step.Backup));
                        // a rename also works for a DLL that is already loaded (VRPatcher.dll), where an overwrite does not
                        File.Move(step.Target, step.Backup);
                        step.BackedUp = true;
                    }
                    File.Move(step.Unpacked, step.Target);
                    step.Placed = true;
                    log((step.BackedUp ? "Replaced " : "Added ") + step.Relative, false);
                }
                return true;
            }
            catch (Exception e)
            {
                log("Could not install " + (current != null ? current.Relative : "?") + ": " + e.Message.TrimEnd('.') + ". Restoring the previous files.", true);
                Rollback(steps, log);
                return false;
            }
        }

        // Removes the new files and moves the backed up files back, newest first.
        private static void Rollback(List<FileStep> steps, Action<string, bool> log)
        {
            for (int i = steps.Count - 1; i >= 0; i--)
            {
                FileStep step = steps[i];
                try
                {
                    if (step.Placed) { File.Delete(step.Target); step.Placed = false; }
                    if (step.BackedUp) { File.Move(step.Backup, step.Target); step.BackedUp = false; log("Restored " + step.Relative, false); }
                }
                catch (Exception e) { log("Could not restore " + step.Relative + ": " + e.Message + (step.Backup != null ? ". Its old copy is in " + step.Backup : ""), true); }
            }
        }

        // Returns the version line of ready.txt ("version=1.2.3").
        private static string ReadReadyVersion(string path)
        {
            foreach (string line in File.ReadAllLines(path))
                if (line.StartsWith("version=", StringComparison.Ordinal)) return line.Substring(8).Trim();
            return "";
        }

        // Logs the failure and writes the marker that stops the plugin from downloading this version again.
        private static void Fail(string bepInExRoot, string version, string message, Action<string, bool> log)
        {
            log(message, true);
            WriteText(UpdateLayout.FailedMarkerPath(bepInExRoot, version), message + "\nDelete this file to let VRTRAKILL download " + version + " again.\n");
            WriteText(UpdateLayout.ResultPath(bepInExRoot), message + "\n");
        }

        // Returns the path, or the path with -2, -3 and so on when it already exists.
        private static string UniqueDir(string path)
        {
            string result = path;
            for (int i = 2; Directory.Exists(result) || File.Exists(result); i++) result = path + "-" + i;
            return result;
        }

        // Writes a text file, creating its folder; ignores errors.
        private static void WriteText(string path, string text)
        {
            try { Directory.CreateDirectory(Path.GetDirectoryName(path)); File.WriteAllText(path, text, Encoding.UTF8); }
            catch (Exception) { }
        }

        // Deletes a file if it exists; ignores errors.
        private static void TryDelete(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); } catch (Exception) { }
        }

        // Deletes a folder and its contents if it exists, logging a failure.
        private static void DeleteDir(string path, Action<string, bool> log)
        {
            try { if (Directory.Exists(path)) Directory.Delete(path, true); }
            catch (Exception e) { log("Could not delete " + path + ": " + e.Message, true); }
        }
    }
}
