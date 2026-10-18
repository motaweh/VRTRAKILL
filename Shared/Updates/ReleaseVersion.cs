using System;

namespace VRTRAKILL.Updates
{
    // A release version: major.minor.patch with an optional prerelease suffix (1.2.3-beta).
    internal sealed class ReleaseVersion : IComparable<ReleaseVersion>
    {
        public int Major { get; private set; }
        public int Minor { get; private set; }
        public int Patch { get; private set; }
        public string Prerelease { get; private set; }

        // True when the version has a prerelease suffix.
        public bool IsPrerelease => !string.IsNullOrEmpty(Prerelease);

        // Parses "1.2.3", "v1.2.3", "1.2" or "1.2.3-beta.1+build"; returns null when the text is not a version.
        public static ReleaseVersion Parse(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            string s = text.Trim();
            if (s.StartsWith("v", StringComparison.OrdinalIgnoreCase)) s = s.Substring(1);

            int plus = s.IndexOf('+');
            if (plus >= 0) s = s.Substring(0, plus);
            string pre = null;
            int dash = s.IndexOf('-');
            if (dash >= 0) { pre = s.Substring(dash + 1); s = s.Substring(0, dash); if (pre.Length == 0) return null; }

            string[] parts = s.Split('.');
            if (parts.Length < 2 || parts.Length > 3) return null;
            int[] numbers = new int[3];
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length == 0 || parts[i].Length > 9) return null;
                foreach (char c in parts[i]) if (c < '0' || c > '9') return null;
                numbers[i] = int.Parse(parts[i]);
            }
            return new ReleaseVersion { Major = numbers[0], Minor = numbers[1], Patch = numbers[2], Prerelease = pre };
        }

        // Orders by major, minor, patch; a prerelease sorts before the release with the same numbers.
        public int CompareTo(ReleaseVersion other)
        {
            if (other == null) return 1;
            int c = Major.CompareTo(other.Major);
            if (c == 0) c = Minor.CompareTo(other.Minor);
            if (c == 0) c = Patch.CompareTo(other.Patch);
            if (c == 0 && IsPrerelease != other.IsPrerelease) c = IsPrerelease ? -1 : 1;
            if (c == 0 && IsPrerelease) c = string.CompareOrdinal(Prerelease, other.Prerelease);
            return c;
        }

        // Returns the version as major.minor.patch, plus the prerelease suffix if there is one.
        public override string ToString()
            => Major + "." + Minor + "." + Patch + (IsPrerelease ? "-" + Prerelease : "");
    }
}
