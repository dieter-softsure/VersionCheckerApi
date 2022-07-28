using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VersionCheckerApi.Analysing;
using VersionCheckerApi.Analysing.Packages;
using VersionCheckerApi.Analysing.Packages.LatestVersionGetters;
using VersionCheckerApi.Analysing.Packages.Security;
using VersionCheckerApi.Controllers.Models;
using VersionCheckerApi.Persistence;
using VersionCheckerApi.Util;

namespace VersionCheckerApi.Controllers
{
    [Route("hooks")]
    [ApiController]
    public class WebhooksController : ControllerBase
    {
        private readonly ProjectBuilder _projectBuilder;
        private readonly ProjectContext _projectContext;

        private readonly PackageBuilder _packageBuilder;
        private readonly SecurityService _securityService;
        private readonly PackagistService _packagistService;

        private readonly IConfiguration _configuration;

        public WebhooksController(ProjectBuilder projectBuilder, ProjectContext projectContext, PackageBuilder packageBuilder, SecurityService securityService, PackagistService packagistService, IConfiguration configuration)
        {
            _projectBuilder = projectBuilder;
            _projectContext = projectContext;
            _packageBuilder = packageBuilder;
            _securityService = securityService;
            _packagistService = packagistService;
            _configuration = configuration;
        }

        [HttpPost("{projectId}")]
        [Authorize]
        public async Task<IActionResult> CreateWebhookForProject(string projectId)
        {
            var projects = await _projectContext.Projects.Where(p => p.ProjectReferenceId == projectId).ToListAsync();
            var getter = _configuration.GetGetterFromConfig(projects[0]);

            var oldWebhook = await getter.GetWebhook(projectId);
            if (oldWebhook != null)
            {
                if (projects[0].WebhookId == oldWebhook)
                    return Ok(oldWebhook); // webhook was configured correctly already

                foreach (var project in projects)
                    project.WebhookId = oldWebhook;

                await _projectContext.SaveChangesAsync();
                return Ok(oldWebhook); // webhook was not configured correctly
            } // -> webhook does not exist

            var webhookId = await getter.SetUpWebhook(projectId);

            foreach (var project in projects)
                project.WebhookId = webhookId;
            await _projectContext.SaveChangesAsync();

            return Content(webhookId);
        }

        // Gets called by devops when a merge has gone through
        [HttpPost("onmerge")]
        public async Task<IActionResult> OnMerge(DevopsMergeHook message)
        {
            var allPackages = await _projectContext.Packages.ToListAsync();
            _packageBuilder.AddPackages(allPackages);
            await _securityService.Populate();
            await _packagistService.GetProvidersForV1();

            return Ok(await _projectBuilder.UpdateProject(message.Resource.Repository.Id));
        }
    }
}
