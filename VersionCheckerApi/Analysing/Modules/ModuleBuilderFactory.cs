using VersionCheckerApi.Analysing.Packages;

namespace VersionCheckerApi.Analysing.Modules
{
    public class ModuleBuilderFactory
    {
        private readonly PackageBuilder _packageBuilder;

        public ModuleBuilderFactory(PackageBuilder packageBuilder)
        {
            _packageBuilder = packageBuilder;
        }

        public IModuleBuilder CreateBuilder(string fileName)
        {
            return fileName switch
            {
                _ when fileName.EndsWith(".csproj") => new NetModuleBuilder(_packageBuilder),
                _ when fileName.EndsWith("package.json") => new NodeModuleBuilder(_packageBuilder),
                _ when fileName.EndsWith("composer.json") => new PhpModuleBuilder(_packageBuilder),
                _ => throw new ArgumentException("Invalid file name specified", fileName)
            };
        }

        public static string[] SupportedFileExtensions => new[]
        {
            "package.json",
            ".csproj",
            "composer.json"
        };
    }
}
