namespace VersionCheckerApi.Controllers.Models
{
    public class AnalyticsReport
    {
        public int ProjectAmount { get; set; }
        public int PackageAmount { get; set; }
        public List<DonutData> Vulnerabilities { get; set; }
        public List<DonutData> VulnerabilitiesDetailed { get; set; }
        public List<DonutData> Outdated { get; set; }
    }

    public class DonutData
    {
        public int Value { get; set; }
        public string Category { get; set; }
    }

    public class PackageAnalyticsReport
    {
        public List<VulnerableListItem> Vulnerabilities { get; set; }
        public List<OutdatedListItem> Outdated { get; set; }
    }

    public class VulnerableListItem
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int CriticalAmount { get; set; }
        public int HighAmount { get; set; }
        public int ModerateAmount { get; set; }
        public int LowAmount { get; set; }
    }

    public class OutdatedListItem
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int MajorAmount { get; set; }
        public int MinorAmount { get; set; }
        public int PatchAmount { get; set; }
    }
}
