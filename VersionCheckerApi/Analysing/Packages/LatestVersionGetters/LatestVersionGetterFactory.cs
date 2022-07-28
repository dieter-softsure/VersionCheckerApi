using PackageType = VersionCheckerApi.Persistence.Models.PackageType;

namespace VersionCheckerApi.Analysing.Packages.LatestVersionGetters
{
    public class LatestVersionGetterFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public LatestVersionGetterFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public LatestVersionGetter GetVersionGetter(PackageType type)
        {
            return type switch
            {
                PackageType.Nuget => _serviceProvider.GetRequiredService<NugetService>(),
                PackageType.Npm => _serviceProvider.GetRequiredService<NpmService>(),
                PackageType.Composer => _serviceProvider.GetRequiredService<PackagistService>(),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}
