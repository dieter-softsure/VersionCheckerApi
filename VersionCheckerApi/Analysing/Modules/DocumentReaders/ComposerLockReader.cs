using Newtonsoft.Json;
using NuGet.Versioning;

namespace VersionCheckerApi.Analysing.Modules.DocumentReaders
{
    public class ComposerLockReader
    {
        public List<(string name, NuGetVersion version, Dictionary<string, string> metadata)> GetInstalledVersions(string file, List<(string name, NuGetVersion version, Dictionary<string, string> metadata)> packages)
        {
            var content = JsonConvert.DeserializeObject<ComposerLockContent>(file);

            var updatedPackages = new List<(string name, NuGetVersion version, Dictionary<string, string> metadata)>();

            if (packages.Any(p => p.name == "php"))
            {
                var phpPackage = packages.First(p => p.name == "php");
                updatedPackages.Add(phpPackage);
                packages.Remove(phpPackage);
            }

            updatedPackages = updatedPackages.Concat(from package in packages 
                let updatedVersion = FilterPackageString(content.Packages.First(p => p.Name == package.name).Version) 
                select (package.name, updatedVersion, package.metadata)).ToList();

            return updatedPackages;
        }

        private NuGetVersion FilterPackageString(string pack)
        {
            var versionString = pack.StartsWith('v') ? pack[1..] : pack;

            return NuGetVersion.Parse(versionString);
        }

        private class ComposerLockContent
        {
            [JsonProperty("packages")]
            public List<Package> Packages { get; set; } = new();
        }

        private class Package
        {
            [JsonProperty("version")]
            public string Version { get; set; }
            [JsonProperty("name")]
            public string Name { get; set; }

            // has more
        }
    }
}