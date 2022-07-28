using Newtonsoft.Json;
using NuGet.Versioning;

namespace VersionCheckerApi.Analysing.Modules.DocumentReaders
{
    public class PackageLockJsonReader
    {
        public List<(string name, NuGetVersion version, Dictionary<string, string> metadata)> GetInstalledVersions(string file, List<string> packages)
        {
            var content = JsonConvert.DeserializeObject<PackageLockJsonContent>(file);
            if (content.LockfileVersion == 1)
            {
                if (content.Dependencies == null) return new List<(string name, NuGetVersion version, Dictionary<string, string> metadata)>();

                return content.Dependencies
                    .Where(d => packages.Contains(d.Key))
                    .Select(d => (d.Key, FilterPackageString(d.Value), new Dictionary<string, string>()))
                    .Where(d => d.Item2 != null).ToList()!;
            }

            if (content.Packages == null) return new List<(string name, NuGetVersion version, Dictionary<string, string> metadata)>();

            return content.Packages
                .Where(d => packages.Contains(d.Key))
                .Select(d => (d.Key, FilterPackageString(d.Value), new Dictionary<string, string>()))
                .Where(d => d.Item2 != null).ToList()!;
        }

        // NPM versions can have have all sorts of differences.
        // This function tries to format them into the x.y.z format
        private NuGetVersion? FilterPackageString(Package pack)
        {
            if (pack.Dev) return null;
            return NuGetVersion.TryParse(pack.Version, out var version) ? version : null;
        }

        private class PackageLockJsonContent
        {
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("version")]
            public string Version { get; set; }
            [JsonProperty("lockfileVersion")]
            public int LockfileVersion { get; set; }
            [JsonProperty("requires")]
            public bool Requires { get; set; }
            [JsonProperty("dependencies")]
            public Dictionary<string, Package>? Dependencies { get; set; }
            [JsonProperty("packages")]
            public Dictionary<string, Package>? Packages { get; set; }
        }

        private class Package
        {
            [JsonProperty("version")]
            public string Version { get; set; }
            [JsonProperty("dev")]
            public bool Dev { get; set; }

            // has more
        }
    }
}