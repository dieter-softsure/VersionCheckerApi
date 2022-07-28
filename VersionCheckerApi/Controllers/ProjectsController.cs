using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VersionCheckerApi.Analysing;
using VersionCheckerApi.Analysing.Packages;
using VersionCheckerApi.Analysing.Packages.LatestVersionGetters;
using VersionCheckerApi.Analysing.Packages.Security;
using VersionCheckerApi.Analysing.RepoGetter.Github;
using VersionCheckerApi.Persistence;

namespace VersionCheckerApi.Controllers
{
    [Route("[controller]")]
    [Authorize]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly ProjectContext _projectContext;
        private readonly ProjectBuilder _projectBuilder;

        private readonly PackageBuilder _packageBuilder;
        private readonly SecurityService _securityService;
        private readonly PackagistService _packagistService;
        

        public ProjectsController(ProjectContext projectContext, ProjectBuilder projectBuilder, PackageBuilder packageBuilder, SecurityService securityService, PackagistService packagistService)
        {
            _projectContext = projectContext;
            _projectBuilder = projectBuilder;
            _packageBuilder = packageBuilder;
            _securityService = securityService;
            _packagistService = packagistService;
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProjects()
        {
            var allPackages = await _projectContext.Packages.ToListAsync();
            _packageBuilder.AddPackages(allPackages);
            await _securityService.Populate();
            await _packagistService.GetProvidersForV1();

            //await _projectBuilder.UpdateProject("18065395-349c-4852-bbc0-ef4d09740ed1");
            await _projectBuilder.UpdateAllProjects();

            await _projectContext.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("search/{searchInput?}")]
        public async Task<IActionResult> GetProjects(string? searchInput)
        {
            var projects = searchInput == null ? 
                await _projectContext.Projects.Include(p => p.Pipelines).ToListAsync() :
                await _projectContext.Projects.Where(p => p.Name.Contains(searchInput)).Include(p => p.Pipelines).ToListAsync();

            return Ok(projects);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(int id)
        {
            return Ok(await _projectContext.FullProjects().SingleAsync(p => p.ProjectId == id));
        }
    }
}
