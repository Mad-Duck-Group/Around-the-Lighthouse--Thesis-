using System;
using MessagePack;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;

namespace Madduck.Core
{
    public enum SaveLocation
    {
        PersistentDataPath,
        DataPath,
        Custom
    }
    
    [Serializable]
    [MessagePackObject]
    public record VersionInfo : IComparable<VersionInfo>
    {
        [Key("Major")]
        public uint major = 0u;
        [Key("Minor")]
        public uint minor = 0u;
        [Key("Patch")]
        public uint patch = 0u;
        [Key("ReleaseEnvironment")]
        public string releaseEnvironment = "Unknown";
        [Key("Adjustment")]
        public uint adjustment = 1u;
        [Key("Platform")]
        public string platform = "Unknown";
        
        [ShowInInspector, DisplayAsString] private string FullVersionString => ToString();

        public static bool TryParse(string versionString, out VersionInfo versionInfo)
        {
            versionInfo = new VersionInfo();
            if (string.IsNullOrEmpty(versionString)) return false;
            // Example version string: "1.0.0-release.adjustment-platform"
            var parts = versionString.Split('-');
            if (parts.Length == 0) return false;
            var versionParts = parts[0].Split('.');
            var releaseParts = parts.Length > 1 ? parts[1].Split('.') : Array.Empty<string>();
            var majorValue = 0u;
            var minorValue = 0u;
            var patchValue = 0u;
            var releaseEnvironment = "Unknown";
            var adjustmentValue = 1u;
            var platform = "Unknown";
            if (versionParts.Length >= 1) uint.TryParse(versionParts[0], out majorValue);
            if (versionParts.Length >= 2) uint.TryParse(versionParts[1], out minorValue);
            if (versionParts.Length >= 3) uint.TryParse(versionParts[2], out patchValue);
            if (releaseParts.Length >= 1) releaseEnvironment = releaseParts[0];
            if (releaseParts.Length >= 2)
                uint.TryParse(releaseParts[1], out adjustmentValue);
            if (parts.Length >= 3) platform = parts[2];
            versionInfo = new VersionInfo
            {
                major = majorValue,
                minor = minorValue,
                patch = patchValue,
                releaseEnvironment = releaseEnvironment,
                adjustment = adjustmentValue,
                platform = platform
            };
            return true;
        }

        public override string ToString()
        {
            return $"{major}.{minor}.{patch}-{releaseEnvironment}.{adjustment}-{platform}";
        }

        public int CompareTo(VersionInfo other)
        {
            if (other == null) return 1;
            int result = major.CompareTo(other.major);
            if (result != 0) return result;
            result = minor.CompareTo(other.minor);
            if (result != 0) return result;
            result = patch.CompareTo(other.patch);
            if (result != 0) return result;
            result = adjustment.CompareTo(other.adjustment);
            return result;
        }

        public static bool operator >(VersionInfo left, VersionInfo right) => left.CompareTo(right) > 0;
        public static bool operator <(VersionInfo left, VersionInfo right) => left.CompareTo(right) < 0;
        public static bool operator >=(VersionInfo left, VersionInfo right) => left.CompareTo(right) >= 0;
        public static bool operator <=(VersionInfo left, VersionInfo right) => left.CompareTo(right) <= 0;
    }
    
    [Serializable]
    public record SaveSettings
    {
        public SaveLocation saveLocation = SaveLocation.DataPath;
        public bool encryptSave;
        [ShowIf(nameof(encryptSave))] public string encryptionKey;
        public string saveDirectory = "TestSave";
        public string saveFileName = "testSave";
        
        [Button("Select Save Location")]
        private void SelectSaveLocation()
        {
#if UNITY_EDITOR
            var path = UnityEditor.EditorUtility.OpenFolderPanel("Select Save Location", "", "");
            if (string.IsNullOrEmpty(path)) return;
            if (path.StartsWith(UnityEngine.Application.dataPath))
            {
                saveLocation = SaveLocation.DataPath;
                saveDirectory = path.Substring(UnityEngine.Application.dataPath.Length)
                    .TrimStart(System.IO.Path.DirectorySeparatorChar)
                    .TrimStart(System.IO.Path.AltDirectorySeparatorChar);
            }
            else if (path.StartsWith(UnityEngine.Application.persistentDataPath))
            {
                saveLocation = SaveLocation.PersistentDataPath;
                saveDirectory = path.Substring(UnityEngine.Application.persistentDataPath.Length)
                    .TrimStart(System.IO.Path.DirectorySeparatorChar)
                    .TrimStart(System.IO.Path.AltDirectorySeparatorChar);
            }
            else
            {
                saveLocation = SaveLocation.Custom;
                saveDirectory = path;
            }
#else
            UnityEngine.Debug.LogWarning("SelectSaveLocation is only available in the Unity Editor.");
#endif
        }
        
        public string GetFullSavePath()
        {
            string basePath = saveLocation switch
            {
                SaveLocation.DataPath => UnityEngine.Application.dataPath,
                SaveLocation.PersistentDataPath => UnityEngine.Application.persistentDataPath,
                SaveLocation.Custom => string.Empty,
                _ => throw new ArgumentOutOfRangeException()
            };
            return System.IO.Path.Combine(basePath, saveDirectory, saveFileName);
        }

        public SaveSettings Copy() => this with { };
    }

    public static class JsonUtils
    {
        public static string BeautifyJson(string unPrettyJson)
        {
            var parsedJson = JToken.Parse(unPrettyJson);
            return parsedJson.ToString(Newtonsoft.Json.Formatting.Indented);
        }
    }
}