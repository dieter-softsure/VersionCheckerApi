using VersionCheckerApi.Persistence.Models;

namespace VersionCheckerApi.Analysing.Modules
{
    public interface IModuleBuilder
    {
        Task<Module?> BuildOrUpdateModule(Repository repo, Module? module, DirectoryPaths paths);
    }
}
