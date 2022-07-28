using Newtonsoft.Json;
using NuGet.Versioning;

namespace VersionCheckerApi.Analysing.Packages.LatestVersionGetters
{
    public class NpmService: LatestVersionGetter
    {
        private const string GetUrl = "https://registry.npmjs.org";
        public NpmService(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
        }

        protected override async Task<(NuGetVersion version, List<string> tags)?> GetPackageInfoFromWeb(
            string packageName, Dictionary<string, string> metadata)
        {
            using var client = HttpClientFactory.CreateClient();

            var response = await client.GetStringAsync(GetUrl + "/" + packageName);
            var packages = JsonConvert.DeserializeObject<NpmPackageInfo>(response);

            var versions = packages.Versions.Select(v => new NuGetVersion(new string(v.Key))).ToList();

            var latest = versions.Where(v => !v.IsPrerelease).Max();
            if (latest == null) latest = versions.Last(); //use release candidates if no stable versions are found

            return (latest, packages.Keywords);
        }

        private class NpmPackageInfo
        {
            [JsonProperty("versions")]
            public Dictionary<string, object> Versions { get; set; }
            [JsonProperty("keywords")]
            public List<string> Keywords { get; set; }
        }
    }
}
