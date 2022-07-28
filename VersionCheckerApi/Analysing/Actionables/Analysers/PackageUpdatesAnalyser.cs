using VersionCheckerApi.Persistence.Models;

namespace VersionCheckerApi.Analysing.Actionables.Analysers
{
    public class PackageUpdatesAnalyser : IActionableAnalyser
    {
        public List<Actionable> Analyse(Project project)
        {
            return (from module in project.Modules
                from package in module.Packages
                where package.Version != package.LatestVersion
                select new Actionable
                {
                    Problem = $"Package {package.Name} is outdated.",
                    Action = $"Update to version {package.LatestVersion}",
                    Severity = Severity.Low
                }).ToList();
        }
    }
}
