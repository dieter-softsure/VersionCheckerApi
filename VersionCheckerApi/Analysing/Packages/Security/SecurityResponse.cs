using Newtonsoft.Json;

namespace VersionCheckerApi.Analysing.Packages.Security
{
    public class Package
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class VulnerabilityData
    {
        [JsonProperty("vulnerableVersionRange")]
        public string VulnerableVersionRange { get; set; }

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("severity")]
        public string Severity { get; set; }

        [JsonProperty("advisory")]
        public Advisory Advisory { get; set; }

        [JsonProperty("package")]
        public Package Package { get; set; }
    }

    public class Advisory
    {
        [JsonProperty("permalink")]
        public string PermaLink { get; set; }
    }

    public class Edge
    {
        [JsonProperty("node")]
        public VulnerabilityData VulnerabilityData { get; set; }

        [JsonProperty("cursor")]
        public string Cursor { get; set; }
    }

    public class PageInfo
    {
        [JsonProperty("endCursor")]
        public string EndCursor { get; set; }

        [JsonProperty("hasNextPage")]
        public bool HasNextPage { get; set; }

        [JsonProperty("hasPreviousPage")]
        public bool HasPreviousPage { get; set; }

        [JsonProperty("startCursor")]
        public string StartCursor { get; set; }
    }

    public class SecurityVulnerabilities
    {
        [JsonProperty("edges")]
        public List<Edge> Edges { get; set; }

        [JsonProperty("pageInfo")]
        public PageInfo PageInfo { get; set; }
    }

    public class Data
    {
        [JsonProperty("securityVulnerabilities")]
        public SecurityVulnerabilities SecurityVulnerabilities { get; set; }
    }

    public class SecurityResponse
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }
}
