using System.Collections.Concurrent;
using NuGet.Versioning;
using VersionCheckerApi.Analysing.Packages.LatestVersionGetters;
using VersionCheckerApi.Analysing.Packages.Security;
using VersionCheckerApi.Persistence.Models;
using Package = VersionCheckerApi.Persistence.Models.Package;

namespace VersionCheckerApi.Analysing.Packages
{
    public class PackageBuilder
    {
        private readonly SecurityService _security;
        private readonly LatestVersionGetterFactory _versionGetterFactory;

        private readonly List<string> _importantTags;

        private readonly ConcurrentDictionary<PackageType, Dictionary<string, Package>> _packages = new ();

        public PackageBuilder(SecurityService security, LatestVersionGetterFactory versionGetterFactory, IConfiguration config)
        {
            _security = security;
            _versionGetterFactory = versionGetterFactory;

            _importantTags = config.GetSection("ImportantTags").Get<List<string>>();
        }

        public void AddPackages(List<Package> packages)
        {
            foreach (var package in packages)
            {
                if (!_packages.TryGetValue(package.Type, out _))
                    _packages[package.Type] = new Dictionary<string, Package>();

                if (_packages[package.Type].TryGetValue((package.Name + ":" + package.Version).ToLower(), out _))
                    continue;

                _packages[package.Type][(package.Name + ":" + package.Version).ToLower()] = package;
            }
        }

        public async Task<Package> GetPackage(PackageType type, string packageId, NuGetVersion packageVersion, Dictionary<string, string> metadata)
        {
            if (!_packages.TryGetValue(type, out _))
                _packages[type] = new Dictionary<string, Package>();

            _packages[type].TryGetValue((packageId + ":" + packageVersion.ToFullString()).ToLower(), out var package);

            var newPackage = await BuildPackage(type, packageId, packageVersion, metadata);

            if (package != null)
            {
                package.DiscrepancyLevel = newPackage.DiscrepancyLevel;
                package.ImportantTag = newPackage.ImportantTag;
                package.LatestVersion = newPackage.LatestVersion;
                package.VulnerabilitySeverity = newPackage.VulnerabilitySeverity;
                package.VulnerabilityUrl = newPackage.VulnerabilityUrl;

                if (!package.Tags.SequenceEqual(newPackage.Tags) && newPackage.Tags.Any()) // necessary for change tracking
                    package.Tags = newPackage.Tags;
            }
            else
            {
                package = newPackage;
            }

            // Dont overwrite if another thread built the package in the meantime
            // This is so that every reference to a package is to the same package
            _packages[type].TryAdd((packageId + ":" + packageVersion.ToFullString()).ToLower(), package);
            return _packages[type][(packageId + ":" + packageVersion.ToFullString()).ToLower()];
        }

        public async Task<List<Package>> UpdatePackages(PackageType type, List<Package> oldPackages, List<(string name, NuGetVersion version, Dictionary<string, string> metadata)> newPackages)
        {
            foreach (var (name, version, metadata) in newPackages)
            {
                var package = await GetPackage(type, name, version, metadata);
                var oldpackage = oldPackages.FirstOrDefault(p => p.Name.ToLower() == name.ToLower() && p.Version == version.ToFullString());
                // if package is still in list, continue
                if (oldpackage != null) continue;
                
                // if package is new add it
                oldPackages.Add(package);
            }

            // if package was removed delete it
            oldPackages = oldPackages.Where(p => newPackages.Any(i => i.name.ToLower() == p.Name.ToLower() && i.version.ToFullString() == p.Version)).ToList();

            return oldPackages;
        }

        private async Task<Package> BuildPackage(PackageType type, string name, NuGetVersion packageVersion, Dictionary<string, string> metadata)
        {
            var getter = _versionGetterFactory.GetVersionGetter(type);
            var latestPackageVersion = await getter.GetLatestVersion(name, metadata);
            var vulnerability = _security.GetHighestVulnerability(type, name, packageVersion);

            var tags = latestPackageVersion?.tags ?? new List<string>();

            return new Package
            {
                Name = name,
                Version = packageVersion.ToFullString(),
                Type = type,
                Tags = tags,
                ImportantTag = GetImportantTag(tags),
                LatestVersion = latestPackageVersion?.version.ToString(),
                VulnerabilitySeverity = vulnerability != null ? Enum.Parse<Severity>(vulnerability.Severity, true) : null,
                VulnerabilityUrl = vulnerability?.Advisory.PermaLink,
                DiscrepancyLevel = GetDiscrepancyLevel(packageVersion, latestPackageVersion?.version)
            };
        }

        private DiscrepancyLevel GetDiscrepancyLevel(NuGetVersion version, NuGetVersion? latestVersion)
        {
            if (latestVersion == null) return DiscrepancyLevel.Latest;

            if (version.Major < latestVersion.Major) return DiscrepancyLevel.Major;
            if (version.Minor < latestVersion.Minor) return DiscrepancyLevel.Minor;
            if (version.Patch < latestVersion.Patch) return DiscrepancyLevel.Patch;

            return DiscrepancyLevel.Latest;
        }

        private string? GetImportantTag(List<string> tags)
        {
            var matchedTag = tags.FirstOrDefault(t => _importantTags.Contains(t.ToLower()));
            
            if (matchedTag == null) return null;

            // return tag with uppercase first letter ("umbraco" => "Umbraco", "EPiServer" => "Episerver")
            matchedTag = matchedTag.ToLower();
            return string.Concat(matchedTag[0].ToString().ToUpper(), matchedTag.AsSpan(1));
        }
    }
}
