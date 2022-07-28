using VersionCheckerApi.Persistence.Models;

namespace VersionCheckerApi.Analysing.Actionables.Analysers
{
    public class VersionMismatchAnalyser : IActionableAnalyser
    {
        public List<Actionable> Analyse(Project project)
        {
            var packageGroups = project.Modules.SelectMany(m => m.Packages).GroupBy(p => p.Name);

            return (from packages in packageGroups
                where packages.DistinctBy(p => p.Version).Count() > 1
                select new Actionable
                {
                    Problem = $"Package {packages.Key} has multiple versions in same project.",
                    Action = "Update all packages to use the same version.",
                    Severity = Severity.Moderate
                }).ToList();
        }
    }
}
