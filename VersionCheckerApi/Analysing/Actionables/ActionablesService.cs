using VersionCheckerApi.Analysing.Actionables.Analysers;
using VersionCheckerApi.Persistence.Models;

namespace VersionCheckerApi.Analysing.Actionables
{
    public class ActionablesService
    {
        private static readonly List<IActionableAnalyser> Analysers = new()
        {
            new BranchnamingAnalyser(),
            new FrameworkMismatchAnalyser(),
            new PackageUpdatesAnalyser(),
            new VersionMismatchAnalyser(),
            new VulnerabilitiesAnalyser()
        };

        public List<Actionable> CheckForActionables(Project project)
        {
            var newActionables = new List<Actionable>();

            foreach (var analyser in Analysers)
                newActionables = newActionables.Concat(analyser.Analyse(project)).ToList();

            //Add new
            foreach (var actionable in newActionables)
            {
                if (project.Actionables.Any(a => a.Action == actionable.Action && a.Problem == actionable.Problem)) continue;

                project.Actionables.Add(actionable);
            }

            //Remove old
            for (var index = project.Actionables.Count - 1; index >= 0; index--)
            {
                var actionable = project.Actionables[index];
                if (newActionables.Any(a => a.Action == actionable.Action && a.Problem == actionable.Problem)) continue;

                project.Actionables.RemoveAt(index);
            }

            return project.Actionables;
        }
    }
}
