using VersionCheckerApi.Analysing.Modules.DocumentReaders;
using VersionCheckerApi.Analysing.Packages;
using VersionCheckerApi.Persistence.Models;

namespace VersionCheckerApi.Analysing.Modules
{
    public class NodeModuleBuilder : IModuleBuilder
    {
        private readonly PackageBuilder _packageBuilder;

        public NodeModuleBuilder(PackageBuilder packageBuilder)
        {
            _packageBuilder = packageBuilder;
        }

        public async Task<Module?> BuildOrUpdateModule(Repository repo, Module? module, DirectoryPaths paths)
        {
            var packageJsonFile = await repo.Getter.GetFile(repo, paths.GetBestPathForFile("package.json")!);
            var packageReferences = new PackageJsonReader().Read(packageJsonFile.Content);

            if (packageReferences.Any())
            {
                var lockFilePath = paths.GetBestPathForFile("package-lock.json");
                if (lockFilePath != null) // more accurate versions in lockfile
                {
                    var file = await repo.Getter.GetFile(repo, lockFilePath);
                    packageReferences = new PackageLockJsonReader().GetInstalledVersions(file.Content, packageReferences.Select(d => d.name).ToList());
                }
            }

            var packages = await _packageBuilder.UpdatePackages(PackageType.Npm, module?.Packages ?? new List<Package>(), packageReferences);

            module ??= new Module
            {
                Name = packageJsonFile.DirectoryName,
                FullPath = packageJsonFile.Path,
                ModuleVersion = "NodeJs",
                ModuleType = PackageType.Npm
            };
            
            module.Packages = packages;
            module.ImportantTag = packages.FirstOrDefault(p => p.ImportantTag != null)?.ImportantTag;
            module.BiggestDiscrepancyLevel = packages.Any() ? packages.Max(p => p.DiscrepancyLevel) : DiscrepancyLevel.Latest;
            module.HighestVulnerabilitySeverity = packages.Max(p => p.VulnerabilitySeverity);

            return module;
        }
    }
}
