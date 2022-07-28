using VersionCheckerApi.Persistence.Models;

namespace VersionCheckerApi.Analysing.Actionables.Analysers
{
    public class FrameworkMismatchAnalyser : IActionableAnalyser
    {
        public List<Actionable> Analyse(Project project)
        {
            var moduleGroups = project.Modules.GroupBy(m => m.ModuleType);

            return (from modules in moduleGroups
                where modules.DistinctBy(m => m.ModuleVersion).Count() > 1
                select new Actionable
                {
                    Problem = $"There are {modules.First().ModuleType} modules with different versions in this project.",
                    Action = "Update all modules to use the same version.",
                    Severity = Severity.Moderate
                }).ToList();
        }
    }
}
