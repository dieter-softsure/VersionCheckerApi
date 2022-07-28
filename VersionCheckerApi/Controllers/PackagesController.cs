using Microsoft.AspNetCore.Mvc;
using VersionCheckerApi.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace VersionCheckerApi.Controllers
{
    [Route("[controller]")]
    [Authorize]
    [ApiController]
    public class PackagesController : ControllerBase
    {
        private readonly ProjectContext _projectContext;

        public PackagesController(ProjectContext projectContext)
        {
            _projectContext = projectContext;
        }

        [HttpGet("search/{searchInput?}")]
        public async Task<IActionResult> GetPackages(string? searchInput)
        {
            var query = searchInput != null ? 
                _projectContext.Packages.Where(p => p.Name.Contains(searchInput)) : 
                _projectContext.Packages.AsQueryable();

            var packages = await query.GroupBy(p => p.Name, (key, g) => new
            {
                Name = key,
                Type = g.First().Type,
                LatestVersion = g.First().LatestVersion,
                Vulnerability = g.Max(p => p.VulnerabilitySeverity)
            }).ToListAsync();

            return Ok(packages);
        }

        [HttpGet]
        public async Task<IActionResult> GetFullPackage([FromQuery]string name)
        {
            name = Uri.UnescapeDataString(name);
            var query = _projectContext.Packages.Where(p => p.Name == name);

            var packages = await query.GroupBy(p => new { p.Name, p.Version }, (key, g) => new
            {
                Name = key.Name,
                Version = key.Version,
                Vulnerability = g.First().VulnerabilitySeverity,
                VulnerabilityUrl = g.First().VulnerabilityUrl,
                Projects = g.SelectMany(p => p.Modules).Select(m => _projectContext.Projects.First(p => p.ProjectId == m.ProjectId).Name + ":" + m.Name)
            }).ToListAsync();

            return Ok(packages);
        }
    }
}
