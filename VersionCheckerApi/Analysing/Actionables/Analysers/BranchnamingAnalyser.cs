using VersionCheckerApi.Persistence.Models;

namespace VersionCheckerApi.Analysing.Actionables.Analysers
{
    public class BranchnamingAnalyser : IActionableAnalyser
    {
        public List<Actionable> Analyse(Project project)
        {
            if (project.Branch != "main")
                return new List<Actionable>
                {
                    new()
                    {
                        Problem = "Default branch is not named 'main'",
                        Action = "Update default branch name to 'main'",
                        Severity = Severity.Moderate
                    }
                };

            return new List<Actionable>();
        }
    }
}
