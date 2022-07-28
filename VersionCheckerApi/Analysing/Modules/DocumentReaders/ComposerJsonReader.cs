using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Versioning;

namespace VersionCheckerApi.Analysing.Modules.DocumentReaders
{
    public class ComposerJsonReader
    {
        public List<(string name, NuGetVersion version, Dictionary<string, string> metadata)> Read(string file)
        {
            var content = JsonConvert.DeserializeObject<ComposerJsonContent>(file);
            if (content.Require == null) return new List<(string name, NuGetVersion version, Dictionary<string, string> metadata)>();

            int drupalVersion;
            if (content.Repositories is JObject j)
            {
                var repositories = j.ToObject<Dictionary<string, Repository>>();
                var hasDrupal = repositories?.ContainsKey("drupal") ?? false;
                drupalVersion = hasDrupal ? repositories!["drupal"].Url!.Last() - '0' : -1;
            }
            else
            {
                var repositories = ((JArray?) content.Repositories)?.ToObject<List<Repository>>();
                var drupalRepo = repositories?.FirstOrDefault(r => r.Url?.Contains("drupal") ?? false);
                drupalVersion = drupalRepo != null ? drupalRepo.Url!.Last() - '0' : -1;
            }

            var fullPackages = new List<(string name, NuGetVersion version, Dictionary<string, string> metadata)>();
            foreach (var (key, value) in content.Require)
            {
                var metadata = new Dictionary<string, string>();

                if (key == "php")
                    metadata["php"] = value;

                if (key.Contains("library")) // not public
                    continue;

                if (drupalVersion != -1 && key.StartsWith("drupal"))
                    metadata["drupalVersion"] = drupalVersion.ToString();

                if (key.StartsWith("wpackagist"))
                    metadata["wpackagist"] = "true";

                var version = FilterPackageString(value);
                if (version != null)
                    fullPackages.Add((key.ToLower(), version, metadata));
            }

            return fullPackages;
        }

        private NuGetVersion? FilterPackageString(string packageString)
        {
            var match = Regex.Match(packageString, "[~^]?([\\dvx*]+(?:[.](?:[\\dx*]+))*)");
            var versionString = !match.Success ? null : match.Groups[1].Value;

            return NuGetVersion.TryParse(versionString, out var version) ? version : null;
        }

        private class ComposerJsonContent
        {
            [JsonProperty("repositories")] 
            public object? Repositories { get; set; }

            [JsonProperty("require")]
            public Dictionary<string, string>? Require { get; set; }
        }

        private class Repository
        {
            public string Type { get; set; }
            public string? Url { get; set; }
        }
    }

}
