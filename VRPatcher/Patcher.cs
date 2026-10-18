using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using Mono.Cecil;
using AssetsTools.NET;
using AssetsTools.NET.Extra;

namespace VRPatcher
{
    // Patches globalgamemanagers so the game starts in VR; runs from BepInEx/patchers.
    public class Patcher
    {
        // code successfully borrowed from https://github.com/Raicuparta/two-forks-vr/blob/main/TwoForksVrPatcher/Patcher.cs

        // no targets: a listed DLL counts as patched, so BepInEx rewrote Assembly-CSharp with Cecil and loaded it from memory each start (AssemblyPatcher.PatchAndLoad)
        public static IEnumerable<string> TargetDLLs { get; } = new string[0];
        // Leaves the assembly unchanged; BepInEx requires a Patch method on a preloader patcher.
        public static void Patch(AssemblyDefinition assembly) { }

        // Installs a downloaded mod update; BepInEx calls Finish after it releases the game DLLs and before the plugins load.
        public static void Finish() => UpdateApplier.Run();

        // Finds the game data folder, installs the shipped game files and patches globalgamemanagers before the game loads.
        public static void Initialize()
        {
            string InstallerPath = Assembly.GetExecutingAssembly().Location,
                   GameExePath = Process.GetCurrentProcess().MainModule.FileName,
                   PatcherPath = Path.GetDirectoryName(InstallerPath),

                   GamePath = Path.GetDirectoryName(GameExePath),
                   GameName = Path.GetFileNameWithoutExtension(GameExePath),
                   DataPath = Path.Combine(GamePath, $"{GameName}_Data/"),

                   GameManagersPath = Path.Combine(DataPath, $"globalgamemanagers"),
                   ClassDataPath = Path.Combine(PatcherPath, "classdata.tpk"),
                   // mod managers cannot write to the game folder, so a Thunderstore package ships these files next to the patcher
                   PayloadDataPath = Path.Combine(PatcherPath, "GameData", $"{GameName}_Data");

            Console.WriteLine("Begin epic conversion from ULTRAKILL to VRTRAKILL...");
            InstallGameFiles(PayloadDataPath, DataPath);
            PatchGameManagers(GameManagersPath, ClassDataPath);
        }

        // Copies the shipped game files and logs whether this session can still use them.
        private static void InstallGameFiles(string PayloadDataPath, string DataPath)
        {
            if (!Directory.Exists(PayloadDataPath))
            {
                Console.WriteLine($"No game files next to the patcher ('{PayloadDataPath}'); the game folder is used as installed.");
                return;
            }

            GameFileSyncResult result = SyncGameFiles(PayloadDataPath, DataPath);
            Console.WriteLine($"Game files: {result.Copied} copied, {result.Unchanged} already up to date, {result.Failed} failed.");
            if (result.Failed > 0)
                Console.WriteLine("Some game files could not be copied. Close other programs that use the ULTRAKILL folder, then start the game again.");
            if (!result.SubsystemsChanged)
                return;

            bool? scanned = HasUnityScannedSubsystems(DataPath);
            if (scanned == true)
                Console.WriteLine("RESTART NEEDED: Unity read UnitySubsystems before the VR files were copied. VR will not start this time. Close the game and start it again.");
            else if (scanned == false)
                Console.WriteLine("The VR files were copied before Unity read them. No restart is needed.");
            else
                Console.WriteLine("Could not read Player.log to check the start-up order. If VR does not start this time, close the game and start it again.");
        }

        // Counts of one game file sync.
        public class GameFileSyncResult
        {
            public int Copied, Unchanged, Failed;
            public bool SubsystemsChanged;
        }

        // Copies each payload file into the data folder when it is missing or its SHA-256 differs; public so tests can use a copy of the folder.
        public static GameFileSyncResult SyncGameFiles(string PayloadDataPath, string DataPath)
        {
            GameFileSyncResult result = new GameFileSyncResult();
            string payloadRoot = Path.GetFullPath(PayloadDataPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

            foreach (string source in Directory.GetFiles(payloadRoot, "*", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal))
            {
                string relative = source.Substring(payloadRoot.Length);
                string target = Path.Combine(DataPath, relative);
                try
                {
                    string sourceHash = ComputeSha256(source);
                    if (File.Exists(target)
                        && new FileInfo(target).Length == new FileInfo(source).Length
                        && string.Equals(ComputeSha256(target), sourceHash, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Unchanged++;
                        continue;
                    }

                    Console.WriteLine(File.Exists(target) ? $"Updating game file '{relative}'." : $"Adding game file '{relative}'.");
                    CopyFileAtomic(source, target, sourceHash);
                    result.Copied++;
                    if (relative.StartsWith("UnitySubsystems", StringComparison.OrdinalIgnoreCase))
                        result.SubsystemsChanged = true;
                }
                catch (Exception e)
                {
                    result.Failed++;
                    Console.WriteLine($"Could not copy game file '{relative}': {e.Message}");
                }
            }
            return result;
        }

        // Writes the file to a temporary name, checks its hash, then swaps it in so the target is never half-written.
        private static void CopyFileAtomic(string source, string target, string sourceHash)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(target));
            string temporaryPath = target + ".vrtrakill.tmp";
            try
            {
                File.Copy(source, temporaryPath, true);
                if (!string.Equals(ComputeSha256(temporaryPath), sourceHash, StringComparison.OrdinalIgnoreCase))
                    throw new IOException("The copied file does not match the shipped file.");

                if (File.Exists(target))
                    File.Replace(temporaryPath, target, null);
                else
                    File.Move(temporaryPath, target);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }
        }

        // Returns whether this session's Player.log already shows the UnitySubsystems scan, or null if the log cannot be read.
        private static bool? HasUnityScannedSubsystems(string DataPath)
        {
            try
            {
                string logPath = null;
                string[] args = Environment.GetCommandLineArgs();
                for (int i = 0; i < args.Length - 1; i++)
                    if (string.Equals(args[i], "-logFile", StringComparison.OrdinalIgnoreCase))
                        logPath = args[i + 1];

                if (logPath == null)
                {
                    // Unity writes Player.log to LocalLow\<company>\<product>; app.info holds those two names
                    string[] appInfo = File.ReadAllLines(Path.Combine(DataPath, "app.info"));
                    string localLow = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), "AppData", "LocalLow");
                    logPath = Path.Combine(localLow, appInfo[0].Trim(), appInfo[1].Trim(), "Player.log");
                }
                if (logPath == "-" || !File.Exists(logPath))
                    return null;

                using (FileStream stream = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                using (StreamReader reader = new StreamReader(stream))
                    return reader.ReadToEnd().Contains("[Subsystems] Discovering subsystems");
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Patches globalgamemanagers unless it is already patched; public so tests can use a copy of the file.
        public static void PatchGameManagers(string GameManagersPath, string ClassDataPath)
        {
            string GameManagersBackupPath = PrepareCurrentGameManagers(GameManagersPath);
            if (GameManagersBackupPath == null)
            {
                Console.WriteLine("The current globalgamemanagers file is already patched.");
                return;
            }

            PatchVR(GameManagersBackupPath, GameManagersPath, ClassDataPath);
            File.WriteAllText(GetPatchStatePath(GameManagersPath), ComputeSha256(GameManagersPath), Encoding.UTF8);
            Console.WriteLine("Epic conversion from ULTRAKILL to VRTRAKILL is complete.");
        }

        // Backs up the current file and returns the backup path, or returns null if the file is already patched.
        private static string PrepareCurrentGameManagers(string GameManagersPath)
        {
            string statePath = GetPatchStatePath(GameManagersPath);
            string currentHash = ComputeSha256(GameManagersPath);
            if (File.Exists(statePath))
            {
                string patchedHash = File.ReadAllText(statePath, Encoding.UTF8).Trim();
                if (string.Equals(currentHash, patchedHash, StringComparison.OrdinalIgnoreCase))
                    return null;
            }

            Console.WriteLine($"Backing up the current file '{GameManagersPath}'...");
            string BackupPath = GameManagersPath + ".vrtrakill.bak";
            File.Copy(GameManagersPath, BackupPath, true);
            Console.WriteLine($"Created a current-version backup in '{BackupPath}'.");
            return BackupPath;
        }

        // Returns the path of the file that stores the hash of the patched file.
        private static string GetPatchStatePath(string gameManagersPath)
            => gameManagersPath + ".vrtrakill.sha256";

        // Returns the SHA-256 hash of a file as lowercase hex.
        private static string ComputeSha256(string path)
        {
            using (SHA256 sha256 = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
            {
                byte[] hash = sha256.ComputeHash(stream);
                StringBuilder result = new StringBuilder(hash.Length * 2);
                foreach (byte value in hash)
                    result.Append(value.ToString("x2"));
                return result.ToString();
            }
        }

        // Writes and verifies a patched copy, then replaces the live file with it.
        private static void PatchVR(string GameManagersBackupPath, string GameManagersPath, string ClassDataPath)
        {
            Console.WriteLine($"Using ClassData file from '{ClassDataPath}'");

            string temporaryPath = GameManagersPath + ".vrtrakill.tmp";
            try
            {
                WritePatchedGameManagers(GameManagersBackupPath, temporaryPath, ClassDataPath);
                VerifyPatchedGameManagers(temporaryPath, ClassDataPath);

                // keep the live file intact until a complete, verified replacement exists
                File.Replace(temporaryPath, GameManagersPath, null);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }
        }

        // Writes a copy of the source file with OpenVR as the only enabled VR device.
        private static void WritePatchedGameManagers(string sourcePath, string outputPath, string classDataPath)
        {
            AssetsManager AM = new AssetsManager();
            try
            {
                AM.LoadClassPackage(classDataPath);

                AssetsFileInstance GGM = AM.LoadAssetsFile(sourcePath, false);
                AssetsFile GGMFile = GGM.file;

                if (AM.LoadClassDatabaseFromPackage(GGMFile.Metadata.UnityVersion) == null)
                    throw new InvalidDataException($"No class database is available for Unity {GGMFile.Metadata.UnityVersion}.");

                AssetFileInfo BuildSettings = GetBuildSettings(GGMFile);
                AssetTypeValueField BuildSettingsBase = AM.GetBaseField(GGM, BuildSettings);
                AssetTypeValueField EnabledVRDevices = BuildSettingsBase["enabledVRDevices"]["Array"];
                if (EnabledVRDevices.IsDummy)
                    throw new InvalidDataException("The enabledVRDevices array was not found in BuildSettings.");

                AssetTypeValueField OpenVR = ValueBuilder.DefaultValueFieldFromArrayTemplate(EnabledVRDevices);
                OpenVR.AsString = "OpenVR";
                EnabledVRDevices.Children = new List<AssetTypeValueField> { OpenVR };

                BuildSettings.SetNewData(BuildSettingsBase);

                using (AssetsFileWriter Writer = new AssetsFileWriter(File.Create(outputPath)))
                    GGMFile.Write(Writer);
            }
            finally
            {
                AM.UnloadAll(true);
            }
        }

        // Throws if the file does not list OpenVR as its only enabled VR device.
        private static void VerifyPatchedGameManagers(string path, string classDataPath)
        {
            AssetsManager AM = new AssetsManager();
            try
            {
                AM.LoadClassPackage(classDataPath);
                AssetsFileInstance GGM = AM.LoadAssetsFile(path, false);
                if (AM.LoadClassDatabaseFromPackage(GGM.file.Metadata.UnityVersion) == null)
                    throw new InvalidDataException($"No class database is available for Unity {GGM.file.Metadata.UnityVersion}.");

                AssetTypeValueField BuildSettings = AM.GetBaseField(GGM, GetBuildSettings(GGM.file));
                AssetTypeValueField EnabledVRDevices = BuildSettings["enabledVRDevices"]["Array"];
                bool isValid = !EnabledVRDevices.IsDummy
                    && EnabledVRDevices.Children.Count == 1
                    && EnabledVRDevices.Children[0].AsString == "OpenVR";

                if (!isValid)
                    throw new InvalidDataException("The generated globalgamemanagers file failed VR validation.");
            }
            finally
            {
                AM.UnloadAll(true);
            }
        }

        // Returns the BuildSettings asset, or throws if there is not exactly one.
        private static AssetFileInfo GetBuildSettings(AssetsFile file)
        {
            List<AssetFileInfo> matches = file.GetAssetsOfType(AssetClassID.BuildSettings);
            if (matches.Count != 1)
                throw new InvalidDataException($"Expected one BuildSettings asset, found {matches.Count}.");
            return matches.Single();
        }
    }
}
