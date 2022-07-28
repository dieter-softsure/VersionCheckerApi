using NuGet.Versioning;
using VersionCheckerApi.Analysing.Modules.DocumentReaders;
using VersionCheckerApi.Analysing.Packages;
using VersionCheckerApi.Persistence.Models;

namespace VersionCheckerApi.Analysing.Modules
{
    public class NetModuleBuilder : IModuleBuilder
    {
        private readonly PackageBuilder _packageBuilder;

        public NetModuleBuilder(PackageBuilder packageBuilder)
        {
            _packageBuilder = packageBuilder;
        }

        public async Task<Module?> BuildOrUpdateModule(Repository repo, Module? module, DirectoryPaths paths)
        {
            var projectFile = await repo.Getter.GetFile(repo, paths.GetBestPathForFile(".csproj")!);

            var reader = new CsprojReader(projectFile.Content);
            var version = reader.GetFrameworkVersion();

            if (version == null) return null; // Could be a build file which is not a project

            var packageReferences = await GetPackageReferences(repo, paths, reader);
            var packages = await _packageBuilder.UpdatePackages(PackageType.Nuget, module?.Packages ?? new List<Package>(), packageReferences);

            module ??= new Module
            {
                Name = projectFile.DirectoryName,
                FullPath = projectFile.Path,
                ModuleType = PackageType.Nuget
            };

            module.ModuleVersion = version;
            module.Packages = packages;
            module.ImportantTag = packages.FirstOrDefault(p => p.ImportantTag != null)?.ImportantTag;
            module.BiggestDiscrepancyLevel = packages.Any() ? packages.Max(p => p.DiscrepancyLevel) : DiscrepancyLevel.Latest;
            module.HighestVulnerabilitySeverity = packages.Max(p => p.VulnerabilitySeverity);

            return module;
        }

        private async Task<List<(string name, NuGetVersion version, Dictionary<string, string> metadata)>> GetPackageReferences(Repository repo, DirectoryPaths paths, CsprojReader reader)
        {
            if (reader.HasPackageReferences()) return reader.GetPackages();

            var packagesFilePath = paths.GetBestPathForFile("packages.config");
            if (packagesFilePath == null)
                return new List<(string name, NuGetVersion version, Dictionary<string, string> metadata)>(); // if this happens someone broke the project

            var packagesFile = await repo.Getter.GetFile(repo, packagesFilePath);
            return new PackagesConfigReader().GetPackages(packagesFile.Content);
        }
    }
}