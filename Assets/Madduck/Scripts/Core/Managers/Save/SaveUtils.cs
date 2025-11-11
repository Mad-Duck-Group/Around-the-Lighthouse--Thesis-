using System;
using System.Collections.Generic;
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
        [Key("PreReleaseIdentifiers")]
        public List<string> preReleaseIdentifier = new();
        [Key("BuildIdentifiers")]
        public List<string> buildIdentifier = new();
        
        [ShowInInspector, DisplayAsString] private string FullVersionString => ToString();

        public static bool TryParse(string versionString, out VersionInfo versionInfo)
        {
            versionInfo = null;
            if (string.IsNullOrEmpty(versionString)) return false;
            // Example version string: "1.0.0-alpha+windows"
            var parts = versionString.Split('-');
            var coreVersionParts = parts[0].Split('.');
            var preReleaseAndBuildPart = parts[1].Split('+');
            var preReleaseParts = preReleaseAndBuildPart[0].Split('.');
            var buildParts = preReleaseAndBuildPart.Length > 1 ? preReleaseAndBuildPart[1].Split('.') : Array.Empty<string>();
            var majorValue = 0u;
            var minorValue = 0u;
            var patchValue = 0u;
            var preReleaseIdentifiers = new List<string>(preReleaseParts);
            var buildIdentifier = new List<string>(buildParts);
            if (coreVersionParts.Length >= 1)
            {
                if (!uint.TryParse(coreVersionParts[0], out majorValue)) return false;
            }
            if (coreVersionParts.Length >= 2)
            {
                if (!uint.TryParse(coreVersionParts[1], out minorValue)) return false;
            }
            if (coreVersionParts.Length >= 3)
            {
                if (!uint.TryParse(coreVersionParts[2], out patchValue)) return false;
            }
            versionInfo = new VersionInfo
            {
                major = majorValue,
                minor = minorValue,
                patch = patchValue,
                preReleaseIdentifier = preReleaseIdentifiers,
                buildIdentifier = buildIdentifier
            };
            return true;
        }

        public override string ToString()
        {
            return $"{major}.{minor}.{patch}-{string.Join(".", preReleaseIdentifier)}+{string.Join(".", buildIdentifier)}";
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
            result = preReleaseIdentifier.Count.CompareTo(other.preReleaseIdentifier.Count);
            if (result != 0) return result;
            for (int i = 0; i < preReleaseIdentifier.Count; i++)
            {
                result = string.Compare(preReleaseIdentifier[i], other.preReleaseIdentifier[i], StringComparison.Ordinal);
                if (result != 0) return result;
            }
            result = buildIdentifier.Count.CompareTo(other.buildIdentifier.Count);
            if (result != 0) return result;
            for (int i = 0; i < buildIdentifier.Count; i++)
            {
                result = string.Compare(buildIdentifier[i], other.buildIdentifier[i], StringComparison.Ordinal);
                if (result != 0) return result;
            }
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