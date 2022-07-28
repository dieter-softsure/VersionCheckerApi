using NuGet.Versioning;

namespace VersionCheckerApi.Analysing.Packages.LatestVersionGetters
{
    public abstract class LatestVersionGetter
    {
        protected readonly IHttpClientFactory HttpClientFactory;
        private readonly Dictionary<string, (NuGetVersion version, List<string> tags)?> _packages = new();

        protected abstract Task<(NuGetVersion version, List<string> tags)?> GetPackageInfoFromWeb(string packageName,
            Dictionary<string, string> metadata);

        protected LatestVersionGetter(IHttpClientFactory httpClientFactory)
        {
            HttpClientFactory = httpClientFactory;
        }

        public async Task<(NuGetVersion version, List<string> tags)?> GetLatestVersion(string packageName, Dictionary<string, string> metadata)
        {
            if (_packages.TryGetValue(packageName, out var package))
                return package;

            var p = await GetPackageInfoFromWeb(packageName, metadata);
            _packages[packageName] = p;

            return p;
        }
    }
}
