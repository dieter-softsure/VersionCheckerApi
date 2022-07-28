using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace VersionCheckerApi.Persistence.Models;

[DebuggerDisplay("{Name}: {Version}")]
public class Package
{
    [Key, Column(Order = 0), DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Name { get; set; }
    [Key, Column(Order = 1), DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Version { get; set; }
    [Key, Column(Order = 2), DatabaseGenerated(DatabaseGeneratedOption.None)]
    public PackageType Type { get; set; }
    public List<string> Tags { get; set; }
    public string? ImportantTag { get; set; }
    public string? LatestVersion { get; set; }
    public DiscrepancyLevel DiscrepancyLevel { get; set; }
    public Severity? VulnerabilitySeverity { get; set; }
    public string? VulnerabilityUrl { get; set; }
    [JsonIgnore]
    public virtual List<Module> Modules { get; set; }
}