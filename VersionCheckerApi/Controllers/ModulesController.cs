using Microsoft.AspNetCore.Mvc;
using VersionCheckerApi.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace VersionCheckerApi.Controllers
{
    [Route("[controller]")]
    [Authorize]
    [ApiController]
    public class ModulesController : ControllerBase
    {
        private readonly ProjectContext _projectContext;

        public ModulesController(ProjectContext projectContext)
        {
            _projectContext = projectContext;
        }

        [HttpGet("search/{searchInput?}")]
        public async Task<IActionResult> GetModules(string? searchInput)
        {
            var query = searchInput != null ?
                _projectContext.Modules.Where(p => p.Name.Contains(searchInput)) :
                _projectContext.Modules.AsQueryable();

            var modules = await query.Select(m => new
                {
                    m.ProjectId,
                    Name = m.Project.Name + ": " + m.Name,
                    m.ImportantTag,
                    m.HighestVulnerabilitySeverity,
                    m.ModuleVersion,
                    m.BiggestDiscrepancyLevel,
                    m.ModuleType,
                    m.FullPath,
                    m.ModuleId
                })
                .ToListAsync();

            return Ok(modules);
        }
    }
}
