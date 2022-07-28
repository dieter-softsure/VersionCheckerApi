using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VersionCheckerApi.Controllers.Models;
using VersionCheckerApi.Persistence;
using VersionCheckerApi.Persistence.Models;

namespace VersionCheckerApi.Controllers
{
    [Route("[controller]")]
    [Authorize]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly ProjectContext projectContext;

        public AnalyticsController(ProjectContext projectContext)
        {
            this.projectContext = projectContext;
        }

        [HttpGet("packages")]
        public async Task<IActionResult> GetPackageLists()
        {
            var vulnerable = from p in projectContext.Projects.SelectMany(p => p.Modules).SelectMany(m => m.Packages).Where(p => p.VulnerabilitySeverity != null)
                             group p by new { p.Name, p.Type } into g
                             select new VulnerableListItem
                             {
                                 Name = g.Key.Name,
                                 Type = g.Key.Type.ToString(),
                                 CriticalAmount = g.Count(g => g.VulnerabilitySeverity == Severity.Critical),
                                 HighAmount = g.Count(g => g.VulnerabilitySeverity == Severity.High),
                                 ModerateAmount = g.Count(g => g.VulnerabilitySeverity == Severity.Moderate),
                                 LowAmount = g.Count(g => g.VulnerabilitySeverity == Severity.Low),
                             };

            var outdated = from p in projectContext.Projects.SelectMany(p => p.Modules).SelectMany(m => m.Packages).Where(p => p.DiscrepancyLevel != DiscrepancyLevel.Latest)
                           group p by new { p.Name, p.Type } into g
                           select new OutdatedListItem
                           {
                               Name = g.Key.Name,
                               Type = g.Key.Type.ToString(),
                               MajorAmount = g.Count(g => g.DiscrepancyLevel == DiscrepancyLevel.Major),
                               MinorAmount = g.Count(g => g.DiscrepancyLevel == DiscrepancyLevel.Minor),
                               PatchAmount = g.Count(g => g.DiscrepancyLevel == DiscrepancyLevel.Patch),
                           };

            var report = new PackageAnalyticsReport
            {
                Vulnerabilities = await vulnerable.ToListAsync(),
                Outdated = await outdated.ToListAsync(),
            };

            return Ok(report);
        }

        [HttpGet]
        public async Task<IActionResult> GetProjectAnalytics()
        {
            var query = from p in projectContext.Projects.SelectMany(p => p.Modules).SelectMany(m => m.Packages)
                    group p by 1 into g
                    select new
                    {
                        packageAmount = g.Count(),
                        vulnerablePackageAmount = g.Count(p => p.VulnerabilitySeverity != null),

                        critical = g.Count(p => p.VulnerabilitySeverity == Severity.Critical),
                        high = g.Count(p => p.VulnerabilitySeverity == Severity.High),
                        moderate = g.Count(p => p.VulnerabilitySeverity == Severity.Moderate),
                        low = g.Count(p => p.VulnerabilitySeverity == Severity.Low),

                        major = g.Count(p => p.DiscrepancyLevel == DiscrepancyLevel.Major),
                        minor = g.Count(p => p.DiscrepancyLevel == DiscrepancyLevel.Minor),
                        patch = g.Count(p => p.DiscrepancyLevel == DiscrepancyLevel.Patch),
                        latest = g.Count(p => p.DiscrepancyLevel == DiscrepancyLevel.Latest)
                    };
            var result = await query.FirstAsync();

            var report = new AnalyticsReport
            {
                ProjectAmount = await projectContext.Projects.CountAsync(),
                PackageAmount = result.packageAmount,
                Vulnerabilities = new List<DonutData>
                {
                    new DonutData
                    {
                        Category = "Good",
                        Value = result.packageAmount - result.vulnerablePackageAmount
                    },
                    new DonutData
                    {
                        Category = "Vulnerable",
                        Value = result.vulnerablePackageAmount
                    }
                },
                VulnerabilitiesDetailed = new List<DonutData>
                {
                    new DonutData
                    {
                        Category = "Critical",
                        Value = result.critical
                    },
                    new DonutData
                    {
                        Category = "High",
                        Value = result.high
                    },
                    new DonutData
                    {
                        Category = "Moderate",
                        Value = result.moderate
                    },
                    new DonutData
                    {
                        Category = "Low",
                        Value = result.low
                    }
                },
                Outdated = new List<DonutData>
                {
                    new DonutData
                    {
                        Category = "Major",
                        Value = result.major
                    },
                    new DonutData
                    {
                        Category = "Minor",
                        Value = result.minor
                    },
                    new DonutData
                    {
                        Category = "Patch",
                        Value = result.patch
                    },
                    new DonutData
                    {
                        Category = "Latest",
                        Value = result.latest
                    }
                }
            };

            return Ok(report);
        }
    }
}
