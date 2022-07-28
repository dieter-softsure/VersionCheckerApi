using System.Collections.Concurrent;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using NuGet.Versioning;
using VersionCheckerApi.Analysing.RepoGetter;
using VersionCheckerApi.Persistence.Models;

namespace VersionCheckerApi.Analysing.Packages.Security
{
    public class SecurityService
    {
        private const string GraphQlUrl = "https://api.github.com/graphql";

        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        
        private readonly ConcurrentDictionary<PackageType, Dictionary<string, List<VulnerabilityData>>> _vulnerabilities = new();

        public SecurityService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task Populate()
        {
            foreach (var packageType in Enum.GetValues<PackageType>())
                _vulnerabilities[packageType] = await GetVulnerabilities(packageType);
        }

        public VulnerabilityData? GetHighestVulnerability(PackageType type, string packageName, NuGetVersion version)
        {
            if (!_vulnerabilities[type].TryGetValue(packageName, out var vulnerabilities)) return null;

            var highestVuln = vulnerabilities.OrderBy(v => v.Severity)
                .FirstOrDefault(v => IsInVersionRange(version, v.VulnerableVersionRange));

            return highestVuln;
        }

        private bool IsInVersionRange(NuGetVersion version, string versionRange)
        {
            // "< 1.23.1" or "= 2.0.6" or ">= 2.6.0, < 2.6.3"
            if (versionRange.Contains(','))
            {
                var lower = versionRange.Split(", ")[0].Trim();
                var lowerVersion = new NuGetVersion(lower.Split(' ')[1]);
                var upper = versionRange.Split(", ")[1].Trim();
                var upperVersion = new NuGetVersion(upper.Split(' ')[1]);

                if (version >= lowerVersion && version < upperVersion) return true;
                return false;
            }
            else
            {
                var indicator = versionRange.Split(' ')[0];
                var vulnVersion = new NuGetVersion(versionRange.Split(' ')[1]);

                if (indicator == "<" && version < vulnVersion) return true;
                if (indicator == "=" && version == vulnVersion) return true;
                return false;
            }
        }
    

        private async Task<Dictionary<string, List<VulnerabilityData>>> GetVulnerabilities(PackageType type)
        {
            using var client = _httpClientFactory.CreateClient();
            var sources = _configuration.GetSection("Sources").Get<List<RepositoryGetterAuthentication>>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", sources.First(s => s.Source == Source.Github).Auth);
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("versionchecker", "1.0"));

            var vulnerabilities = new Dictionary<string, List<VulnerabilityData>>();
            SecurityResponse? response = null; 
            do
            {
                response = await GetVulnerabilitiesPaged(client, response?.Data.SecurityVulnerabilities.PageInfo.EndCursor, type);
                foreach (var edge in response.Data.SecurityVulnerabilities.Edges)
                {
                    var vulnerability = edge.VulnerabilityData;
                    if (vulnerabilities.TryGetValue(vulnerability.Package.Name, out var value))
                    {
                        value.Add(vulnerability);
                    }
                    else
                    {
                        vulnerabilities[vulnerability.Package.Name] = new List<VulnerabilityData>
                        {
                            edge.VulnerabilityData
                        };
                    }
                }
            } while (response.Data.SecurityVulnerabilities.PageInfo.HasNextPage);

            return vulnerabilities;
        }

        private async Task<SecurityResponse> GetVulnerabilitiesPaged(HttpClient client, string? cursor, PackageType type)
        {
            var obj = new QueryObject
            {
                Query = $@"{{
                          securityVulnerabilities(
                            orderBy: {{field: UPDATED_AT, direction: DESC}}
                            ecosystem: {Enum.GetName(type)!.ToUpper()}
                            first: 100
                            {(cursor != null ? $"after: \"{cursor}\"" : "")}
                          ) {{
                            edges {{
                              node {{
                                vulnerableVersionRange
                                updatedAt
                                severity
                                package {{
                                  name
                                }}
                                advisory {{
                                  permalink
                                }}
                              }}
                              cursor
                            }}
                            pageInfo {{
                              endCursor
                              hasNextPage
                              hasPreviousPage
                              startCursor
                            }}
                          }}
                        }}",
                Variables = "{}"
            };
            var response = await client.PostAsync(GraphQlUrl, new StringContent(JsonConvert.SerializeObject(obj)));
            return JsonConvert.DeserializeObject<SecurityResponse>(await response.Content.ReadAsStringAsync());
        }

        private class QueryObject
        {
            [JsonProperty("query")]
            public string Query { get; set; }
            [JsonProperty("variables")]
            public string Variables { get; set; }
        }
    }

}
