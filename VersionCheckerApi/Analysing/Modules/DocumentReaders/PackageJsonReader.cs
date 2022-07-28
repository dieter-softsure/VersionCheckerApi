using System.Text.RegularExpressions;
using Newtonsoft.Json;
using NuGet.Versioning;

namespace VersionCheckerApi.Analysing.Modules.DocumentReaders
{
    public class PackageJsonReader
    {
        public List<(string name, NuGetVersion version, Dictionary<string, string> metadata)> Read(string file)
        {
            var content = JsonConvert.DeserializeObject<PackageJsonContent>(file);
            if (content.Dependencies == null) return new List<(string name, NuGetVersion version, Dictionary<string, string> metadata)>();

            return content.Dependencies.Select(d => (d.Key, FilterPackageString(d.Value), new Dictionary<string, string>())).Where(d => d.Item2 != null).ToList()!; 
        }

        // NPM versions can have have all sorts of differences.
        // This function tries to format them into the x.y.z format
        private NuGetVersion? FilterPackageString(string packageString)
        {
            var match = Regex.Match(packageString, "[~^]?([\\dvx*]+(?:[.](?:[\\dx*]+))*)");
            var versionString = !match.Success ? null : match.Groups[1].Value;

            return NuGetVersion.TryParse(versionString, out var version) ? version : null;
        }

        private class PackageJsonContent
        {
            [JsonProperty("dependencies")]
            public Dictionary<string, string>? Dependencies { get; set; }
        }
    }

}
