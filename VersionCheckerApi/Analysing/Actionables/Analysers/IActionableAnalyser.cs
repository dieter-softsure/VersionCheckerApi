using VersionCheckerApi.Persistence.Models;

namespace VersionCheckerApi.Analysing.Actionables.Analysers
{
    public interface IActionableAnalyser
    {
        List<Actionable> Analyse(Project project);
    }
}
