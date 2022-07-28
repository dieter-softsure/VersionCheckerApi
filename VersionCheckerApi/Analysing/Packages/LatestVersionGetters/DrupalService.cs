using Newtonsoft.Json;
using NuGet.Versioning;

namespace VersionCheckerApi.Analysing.Packages.LatestVersionGetters
{
    public class DrupalServer: LatestVersionGetter
    {
        private const string GetUrl = "https://repo.packagist.org/p2";
        public DrupalServer(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
        }

        protected override async Task<(NuGetVersion version, List<string> tags)?> GetPackageInfoFromWeb(
            string packageName, Dictionary<string, string> metadata)
        {
            using var client = HttpClientFactory.CreateClient();

            var response = await client.GetStringAsync(GetUrl + "/" + packageName + ".json");
            var packages = JsonConvert.DeserializeObject<PackagistPackageInfo>(response);

            var versions = packages.Packages[packageName].Select(p => new NuGetVersion(p.Version.StartsWith('v') ? p.Version[1..] : p.Version)).ToList();

            var latest = versions.Where(v => !v.IsPrerelease).Max();
            if (latest == null) latest = versions.Last(); //use release candidates if no stable versions are found

            return (latest, packages.Packages[packageName][0].Keywords);
        }

        private class PackagistPackageInfo
        {
            [JsonProperty("packages")]
            public Dictionary<string, List<Package>> Packages { get; set; }
        }

        private class Package
        {
            [JsonProperty("name")] 
            public string Name { get; set; }
            [JsonProperty("version")]
            public string Version { get; set; }
            [JsonProperty("keywords")]
            public List<string> Keywords { get; set; }
        }
    }
}
