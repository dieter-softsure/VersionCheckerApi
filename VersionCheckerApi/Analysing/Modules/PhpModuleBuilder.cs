using VersionCheckerApi.Analysing.Modules.DocumentReaders;
using VersionCheckerApi.Analysing.Packages;
using VersionCheckerApi.Persistence.Models;

namespace VersionCheckerApi.Analysing.Modules
{
    public class PhpModuleBuilder : IModuleBuilder
    {
        private readonly PackageBuilder _packageBuilder;

        public PhpModuleBuilder(PackageBuilder packageBuilder)
        {
            _packageBuilder = packageBuilder;
        }

        public async Task<Module?> BuildOrUpdateModule(Repository repo, Module? module, DirectoryPaths paths)
        {
            var packageJsonFile = await repo.Getter.GetFile(repo, paths.GetBestPathForFile("composer.json")!);
            var packageReferences = new ComposerJsonReader().Read(packageJsonFile.Content);

            if (packageReferences.Any())
            {
                var lockFilePath = paths.GetBestPathForFile("composer.lock");
                if (lockFilePath != null) // more accurate versions in lockfile
                {
                    var file = await repo.Getter.GetFile(repo, lockFilePath);
                    packageReferences = new ComposerLockReader().GetInstalledVersions(file.Content, packageReferences);
                }
            }

            var packages = await _packageBuilder.UpdatePackages(PackageType.Composer, module?.Packages ?? new List<Package>(), packageReferences);
            var moduleVersion = "php v" + (packages.FirstOrDefault(p => p.Name == "php")?.Version.ToString() ?? "?.?.?");

            module ??= new Module
            {
                Name = packageJsonFile.DirectoryName,
                FullPath = packageJsonFile.Path,
                ModuleVersion = moduleVersion,
                ModuleType = PackageType.Composer
            };
            
            module.Packages = packages;
            module.ImportantTag = packages.FirstOrDefault(p => p.ImportantTag != null)?.ImportantTag;
            module.BiggestDiscrepancyLevel = packages.Any() ? packages.Max(p => p.DiscrepancyLevel) : DiscrepancyLevel.Latest;
            module.HighestVulnerabilitySeverity = packages.Max(p => p.VulnerabilitySeverity);
            module.ModuleVersion = moduleVersion;

            return module;
        }
    }
}
