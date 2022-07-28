using VersionCheckerApi.Persistence.Models;

namespace VersionCheckerApi.Analysing.Modules
{
    public class ModuleUpdater
    {
        private readonly ModuleBuilderFactory _moduleBuilderFactory;

        public ModuleUpdater(ModuleBuilderFactory moduleBuilderFactory)
        {
            _moduleBuilderFactory = moduleBuilderFactory;
        }

        public async Task<List<Module>> UpdateModules(Repository repo, List<Module> modules)
        {
            foreach (var supportedFileExtension in ModuleBuilderFactory.SupportedFileExtensions)
            {
                var filePaths = repo.GetPathsWithNameEndingIn(supportedFileExtension);
                if (!filePaths.Any()) continue;

                foreach (var filePath in filePaths)
                {
                    var directory = repo.GetDirectory(filePath);
                    var builder = _moduleBuilderFactory.CreateBuilder(filePath);

                    var oldModule = modules.FirstOrDefault(m => m.FullPath == filePath);
                    var newModule = await builder.BuildOrUpdateModule(repo, oldModule, directory);

                    if (oldModule != null && newModule == null) modules.Remove(oldModule);
                    if (oldModule == null && newModule != null) modules.Add(newModule);
                }
            }

            // remove old modules
            modules = modules.Where(m => repo.Paths.Contains(m.FullPath)).ToList();

            return modules;
        }
    }
}
