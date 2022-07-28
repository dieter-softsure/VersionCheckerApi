using System.Diagnostics;

namespace VersionCheckerApi.Persistence.Models;

[DebuggerDisplay("{Name}")]
public class Module
{
    public int ModuleId { get; set; }
    public int ProjectId { get; set; }
    public virtual Project Project { get; set; }
    public string Name { get; set; }
    public string FullPath { get; set; }
    public PackageType ModuleType { get; set; }
    public string ModuleVersion { get; set; }
    public string? ImportantTag { get; set; }
    public DiscrepancyLevel BiggestDiscrepancyLevel { get; set; }
    public Severity? HighestVulnerabilitySeverity { get; set; }
    public virtual List<Package> Packages { get; set; }
}