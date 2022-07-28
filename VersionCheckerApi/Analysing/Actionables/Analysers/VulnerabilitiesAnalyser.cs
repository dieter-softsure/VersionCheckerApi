using VersionCheckerApi.Persistence.Models;

namespace VersionCheckerApi.Analysing.Actionables.Analysers
{
    public class VulnerabilitiesAnalyser : IActionableAnalyser
    {
        public List<Actionable> Analyse(Project project)
        {
            return (from module in project.Modules
                from package in module.Packages
                where package.VulnerabilitySeverity != null
                select new Actionable
                {
                    Problem = $"Package {package.Name} is vulnerable ({package.VulnerabilitySeverity}).",
                    Action = $"Update to non-vulnerable version. Look at {package.VulnerabilityUrl} for more info.",
                    Severity = Severity.Critical
                }).ToList();
        }
    }
}
