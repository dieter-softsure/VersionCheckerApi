using System.Collections.Concurrent;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Versioning;

namespace VersionCheckerApi.Analysing.Packages.LatestVersionGetters
{
    public class PackagistService: LatestVersionGetter
    {
        private const string PackagistUrl = "https://repo.packagist.org/p2";
        private const string DrupalUrl = "https://packages.drupal.org/files/packages/";

        private const string WPackagistUrl = "https://wpackagist.org/";
        private readonly ConcurrentDictionary<string, ProviderData> _providers = new();
        public PackagistService(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
        }

        protected override async Task<(NuGetVersion version, List<string> tags)?> GetPackageInfoFromWeb(
            string packageName, Dictionary<string, string> metadata)
        {
            if (metadata.ContainsKey("php")) return null; // Composer files can contain a package that just specifies the pvp version

            if (metadata.ContainsKey("wpackagist")) 
                return await GetPackageInfoFromComposerV1(packageName);

            return await GetPackageInfoFromComposerV2(packageName, metadata);
        }

        private async Task<(NuGetVersion version, List<string> tags)?> GetPackageInfoFromComposerV1(
            string packageName)
        {
            using var client = HttpClientFactory.CreateClient();

            if (!_providers.Any())
                throw new InvalidOperationException("call GetProvidersForV1 before using");

            var packageExists = _providers.Any(p => p.Value.Providers.ContainsKey(packageName));
            if (!packageExists) return null;

            var provider = _providers.First(p => p.Value.Providers.ContainsKey(packageName));
            var response = await client.GetStringAsync(WPackagistUrl + "p/" + packageName + "$" + provider.Value.Providers[packageName].Sha256 + ".json");

            var packages = JsonConvert.DeserializeObject<WPackagistPackageInfo>(response);
            var versions = packages.Packages[packageName].Select(p => new NuGetVersion(p.Value.Version.StartsWith('v') ? p.Value.Version[1..] : p.Value.Version)).ToList();

            var latest = versions.Where(v => !v.IsPrerelease).Max();
            if (latest == null) latest = versions.Last(); //use release candidates if no stable versions are found

            return (latest, new List<string>());
        }

        public async Task GetProvidersForV1()
        {
            using var client = HttpClientFactory.CreateClient();
            var response = await client.GetStringAsync(WPackagistUrl + "packages.json");
            var providers = JsonConvert.DeserializeObject<WPackageBase>(response);

            foreach (var (name, hashes) in providers!.ProviderIncludes)
            {
                var urlExtension = name.Replace("%hash%", hashes.Sha256);
                var data = await client.GetFromJsonAsync<ProviderData>(WPackagistUrl + urlExtension);
                _providers[name] = data!;
            }
        }

        private async Task<(NuGetVersion version, List<string> tags)?> GetPackageInfoFromComposerV2(
            string packageName, Dictionary<string, string> metadata)
        {
            using var client = HttpClientFactory.CreateClient();

            string response;
            try
            {
                var getString = metadata.ContainsKey("drupalVersion")
                    ? DrupalUrl + metadata["drupalVersion"] + "/p2/" + packageName + ".json"
                    : PackagistUrl + "/" + packageName + ".json";

                response = await client.GetStringAsync(getString);
            }
            catch (Exception)
            {
                return null; // ignore further packages that dont exist
            }

            var packages = JsonConvert.DeserializeObject<PackagistPackageInfo>(response);
            var versions = packages.Packages[packageName].Select(p => new NuGetVersion(p.Version.StartsWith('v') ? p.Version[1..] : p.Version)).ToList();

            var latest = versions.Where(v => !v.IsPrerelease).Max();
            if (latest == null) latest = versions.Last(); //use release candidates if no stable versions are found

            if (packages.Packages[packageName][0].Keywords is string s)
                return (latest, s.Split(',').ToList());

            var keywords = (JArray)packages.Packages[packageName][0].Keywords!;
            return (latest, keywords.ToObject<List<string>>()!);
        }

        private class PackagistPackageInfo
        {
            [JsonProperty("packages")]
            public Dictionary<string, List<Package>> Packages { get; set; }
        }

        private class WPackagistPackageInfo
        {
            [JsonProperty("packages")]
            public Dictionary<string, Dictionary<string, Package>> Packages { get; set; }
        }

        private class Package
        {
            [JsonProperty("name")] 
            public string Name { get; set; }
            [JsonProperty("version_normalized")]
            public string Version { get; set; }
            [JsonProperty("keywords")]
            public object? Keywords { get; set; }
        }

        private class WPackageBase
        {
            [JsonProperty("providers-url")]
            public string ProvidersUrl { get; set; }
            [JsonProperty("provider-includes")]
            public Dictionary<string, Provider> ProviderIncludes { get; set; }
        }

        private class ProviderData
        {
            [JsonProperty("providers")]
            public Dictionary<string, Provider> Providers { get; set; }
        }

        private class Provider
        {
            [JsonProperty("sha256")]
            public string Sha256 { get; set; }
        }
    }
}
