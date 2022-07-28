using System.Text.Json.Serialization;
using NuGet.Versioning;

namespace VersionCheckerApi.Analysing.Packages.LatestVersionGetters
{
    public class NugetService : LatestVersionGetter
    {
        private const string GetUrl = "https://azuresearch-usnc.nuget.org/query";

        public NugetService(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
        }

        protected override async Task<(NuGetVersion version, List<string> tags)?> GetPackageInfoFromWeb(
            string packageName, Dictionary<string, string> metadata)
        {
            using var client = HttpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            var response = await client.GetFromJsonAsync<NugetPackageInfo>(GetUrl + $"?q={packageName}");

            NugetPackage package;
            if (response == null || response.Data.Count == 0) return null;
            else package = response.Data[0];

            return (new NuGetVersion(package.Version), package.Tags);
        }

        private class NugetPackageInfo
        {
            [JsonPropertyName("@context")]
            public Context Context { get; set; }

            [JsonPropertyName("totalHits")]
            public int TotalHits { get; set; }

            [JsonPropertyName("data")]
            public List<NugetPackage> Data { get; set; }
        }

        private class Context
        {
            [JsonPropertyName("@vocab")]
            public string Vocab { get; set; }

            [JsonPropertyName("@base")]
            public string Base { get; set; }
        }

        private class PackageType
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
        }

        private class VersionInfo
        {
            [JsonPropertyName("version")]
            public string Version { get; set; }

            [JsonPropertyName("downloads")]
            public int Downloads { get; set; }

            [JsonPropertyName("@id")]
            public string Id { get; set; }
        }

        private class NugetPackage
        {
            [JsonPropertyName("@id")]
            public string IdLong { get; set; }

            [JsonPropertyName("@type")]
            public string Type { get; set; }

            [JsonPropertyName("registration")]
            public string Registration { get; set; }

            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("version")]
            public string Version { get; set; }

            [JsonPropertyName("description")]
            public string Description { get; set; }

            [JsonPropertyName("summary")]
            public string Summary { get; set; }

            [JsonPropertyName("title")]
            public string Title { get; set; }

            [JsonPropertyName("iconUrl")]
            public string IconUrl { get; set; }

            [JsonPropertyName("licenseUrl")]
            public string LicenseUrl { get; set; }

            [JsonPropertyName("projectUrl")]
            public string ProjectUrl { get; set; }

            [JsonPropertyName("tags")]
            public List<string> Tags { get; set; }

            [JsonPropertyName("authors")]
            public List<string> Authors { get; set; }

            [JsonPropertyName("owners")]
            public List<string> Owners { get; set; }

            [JsonPropertyName("totalDownloads")]
            public int TotalDownloads { get; set; }

            [JsonPropertyName("verified")]
            public bool Verified { get; set; }

            [JsonPropertyName("packageTypes")]
            public List<PackageType> PackageTypes { get; set; }

            [JsonPropertyName("versions")]
            public List<VersionInfo> Versions { get; set; }
        }
    }
}
