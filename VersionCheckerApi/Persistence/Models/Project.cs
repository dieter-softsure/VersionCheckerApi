using System.Diagnostics;

namespace VersionCheckerApi.Persistence.Models
{
    [DebuggerDisplay("{Name}")]
    public class Project
    {
        public int ProjectId { get; set; }
        public Source Source { get; set; }
        public string Organisation { get; set; }
        public string? ProjectReferenceId { get; set; }
        public string RepositoryId { get; set; }
        public string Name { get; set; }
        public string Branch { get; set; }
        public string? ImportantTag { get; set; }
        public DiscrepancyLevel BiggestDiscrepancyLevel { get; set; }
        public Severity? HighestVulnerabilitySeverity { get; set; }
        public string? WebhookId { get; set; }
        public virtual List<Module> Modules { get; set; }
        public virtual List<Actionable> Actionables { get; set; }
        public virtual List<Pipeline> Pipelines { get; set; }
    }

    public enum Source
    {
        Devops,
        Github
    }

    public enum PackageType
    {
        Nuget,
        Npm,
        Composer
    }

    public enum DiscrepancyLevel
    {
        Latest,
        Patch,
        Minor,
        Major
    }

    public enum Severity
    {
        Low,
        Moderate,
        High,
        Critical
    }
}
