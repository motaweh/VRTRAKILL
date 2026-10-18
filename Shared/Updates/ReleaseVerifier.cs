using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace VRTRAKILL.Updates
{
    // Checks a downloaded release: the RSA signature of SHA256SUMS.txt, the zip hash in it, and the zip entry paths.
    internal static class ReleaseVerifier
    {
        private const string Sha256Oid = "2.16.840.1.101.3.4.2.1";
        private const int MaxEntries = 2000;
        private const long MaxUnpackedBytes = 512L * 1024 * 1024;

        // Returns the SHA-256 hash of a file as lowercase hex.
        public static string Sha256Hex(string path)
        {
            using (SHA256 sha256 = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return ToHex(sha256.ComputeHash(stream));
        }

        // Returns bytes as lowercase hex.
        private static string ToHex(byte[] bytes)
        {
            StringBuilder result = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes) result.Append(b.ToString("x2"));
            return result.ToString();
        }

        // Parses "<sha256>  <name>" lines into a name to hash map; malformed lines are ignored.
        public static Dictionary<string, string> ParseSums(string text)
        {
            Dictionary<string, string> sums = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (string raw in text.Split('\n'))
            {
                string line = raw.Trim();
                int space = line.IndexOf(' ');
                if (space != 64) continue;
                string hash = line.Substring(0, 64).ToLowerInvariant();
                string name = line.Substring(space).Trim().TrimStart('*');
                bool hex = true;
                foreach (char c in hash) if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'))) { hex = false; break; }
                if (hex && name.Length > 0 && !sums.ContainsKey(name)) sums.Add(name, hash);
            }
            return sums;
        }

        // Returns true when the base64 signature is a valid RSA PKCS#1 v1.5 SHA-256 signature of the data under the embedded public key.
        public static bool VerifySignature(byte[] data, string signatureBase64)
        {
            byte[] signature;
            try { signature = Convert.FromBase64String(signatureBase64.Trim()); }
            catch (FormatException) { return false; }

            RSAParameters key = new RSAParameters
            {
                Modulus = Convert.FromBase64String(ReleaseKey.ModulusBase64),
                Exponent = Convert.FromBase64String(ReleaseKey.ExponentBase64),
            };
            if (key.Modulus.Length != 384 || signature.Length != key.Modulus.Length) return false;

            byte[] hash;
            using (SHA256 sha256 = SHA256.Create()) hash = sha256.ComputeHash(data);
            // PSS is not available: the game's Mono and .NET Framework RSACryptoServiceProvider only accept PKCS#1 v1.5
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.PersistKeyInCsp = false;
                rsa.ImportParameters(key);
                return rsa.VerifyHash(hash, Sha256Oid, signature);
            }
        }

        // Checks the staged files for a version; returns the zip path, or null with the reason.
        public static string VerifyStaged(string stagingDir, string version, out string reason)
        {
            string zipName = UpdateLayout.ZipName(version);
            string zipPath = Path.Combine(stagingDir, zipName);
            string sumsPath = Path.Combine(stagingDir, UpdateLayout.SumsName);
            string sigPath = Path.Combine(stagingDir, UpdateLayout.SignatureName);
            foreach (string required in new[] { zipPath, sumsPath, sigPath })
                if (!File.Exists(required)) { reason = "missing " + Path.GetFileName(required); return null; }

            byte[] sums = File.ReadAllBytes(sumsPath);
            if (!VerifySignature(sums, File.ReadAllText(sigPath, Encoding.ASCII)))
            { reason = UpdateLayout.SignatureName + " is not a valid signature of " + UpdateLayout.SumsName + " for the VRTRAKILL release key"; return null; }

            string expected;
            if (!ParseSums(Encoding.UTF8.GetString(sums)).TryGetValue(zipName, out expected))
            { reason = UpdateLayout.SumsName + " has no line for " + zipName; return null; }
            string actual = Sha256Hex(zipPath);
            if (actual != expected)
            { reason = zipName + " SHA-256 is " + actual + ", " + UpdateLayout.SumsName + " says " + expected; return null; }

            reason = CheckZipEntries(zipPath);
            return reason == null ? zipPath : null;
        }

        // Returns null when every zip entry is an allowed path and the zip is within the size limits, else the reason.
        public static string CheckZipEntries(string zipPath)
        {
            try
            {
                using (ZipArchive zip = ZipFile.OpenRead(zipPath))
                {
                    if (zip.Entries.Count > MaxEntries) return "the zip has " + zip.Entries.Count + " entries, the limit is " + MaxEntries;
                    long total = 0;
                    HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (ZipArchiveEntry entry in zip.Entries)
                    {
                        string reason;
                        string path = UpdateLayout.NormalizeEntry(entry.FullName, out reason);
                        if (path == null) return "rejected zip entry '" + entry.FullName + "': " + reason;
                        if (!seen.Add(path)) return "duplicate zip entry '" + entry.FullName + "'";
                        total += entry.Length;
                        if (total > MaxUnpackedBytes) return "the zip unpacks to more than " + MaxUnpackedBytes + " bytes";
                    }
                }
                return null;
            }
            catch (Exception e) when (e is IOException || e is InvalidDataException || e is UnauthorizedAccessException)
            {
                return "the zip cannot be read: " + e.Message;
            }
        }
    }
}
