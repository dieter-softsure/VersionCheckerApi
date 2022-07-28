using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using VersionCheckerApi.Analysing.Actionables;
using VersionCheckerApi.Analysing.Modules;
using VersionCheckerApi.Analysing.Pipelines;
using VersionCheckerApi.Analysing.RepoGetter;
using VersionCheckerApi.Analysing.RepoGetter.Devops;
using VersionCheckerApi.Analysing.RepoGetter.Github;
using VersionCheckerApi.Persistence;
using VersionCheckerApi.Persistence.Models;
using VersionCheckerApi.Util;

namespace VersionCheckerApi.Analysing
{
    public class ProjectBuilder
    {
        private readonly ProjectContext _projectContext;
        private readonly ModuleUpdater _moduleUpdater;
        private readonly ActionablesService _actionablesService;
        private readonly PipelineAnalyzer _pipelineAnalyzer;

        private readonly IConfiguration _configuration;

        public ProjectBuilder(ProjectContext projectContext, ActionablesService actionablesService, PipelineAnalyzer pipelineAnalyzer, ModuleUpdater moduleUpdater, IConfiguration configuration)
        {
            _projectContext = projectContext;
            _actionablesService = actionablesService;
            _pipelineAnalyzer = pipelineAnalyzer;
            _moduleUpdater = moduleUpdater;
            _configuration = configuration;
        }

        public async Task UpdateAllProjects()
        {
            var projectsToUpdate = await _projectContext.FullProjects().ToListAsync();

            var newProjects = new List<Project>();
            var sources = _configuration.GetSection("Sources").Get<List<RepositoryGetterAuthentication>>();
            foreach (var source in sources)
            {
                IRepositoryGetter getter = source.Source == Source.Github
                    ? new GithubRepositoryGetter(_configuration, source)
                    : new DevopsRepositoryGetter(_configuration, source);

                var p = await GetAndUpdateProjectsForSource(getter, projectsToUpdate.Where(p => p.Source == source.Source && (p.Organisation == source.Org || p.Organisation == "" && source.Default)).ToList());
                newProjects = newProjects.Concat(p).ToList();
            }

            _projectContext.AddRange(newProjects);
        }

        public async Task<bool> UpdateProject(string repoId)
        {
            var project = await _projectContext.FullProjects().FirstAsync(p => p.RepositoryId == repoId);
            var getter = _configuration.GetGetterFromConfig(project);
            project = await TryBuildOrUpdateProject(project, await getter.GetRepository(repoId));

            return project != null;
        }

        private async Task<List<Project>> GetAndUpdateProjectsForSource(IRepositoryGetter source, List<Project> projectsToUpdate)
        {
            var repoIds = await source.GetOrganisationRepoIds();

            var updatedProjects = new ConcurrentBag<Project>();
            await Parallel.ForEachAsync(projectsToUpdate, new ParallelOptions { MaxDegreeOfParallelism = 10 }, async (project, _) =>
            {
                var repoId = repoIds.FirstOrDefault(id => id == project.RepositoryId);
                if (repoId == default) return; //Project or repository has been deleted

                var updatedProject = await TryBuildOrUpdateProject(project, await source.GetRepository(repoId));
                if (updatedProject == null) return; // project has been changed in a way that we are unable to detect packages

                updatedProjects.Add(project);
            });

            var newProjects = new ConcurrentBag<Project>();
            var newRepos = repoIds.Where(id => projectsToUpdate.All(p => p.RepositoryId != id));
            await Parallel.ForEachAsync(newRepos, new ParallelOptions { MaxDegreeOfParallelism = 10 }, async (newRepoId, _) =>
            {
                var newProject = await TryBuildOrUpdateProject(null, await source.GetRepository(newRepoId));
                if (newProject == null) return; // new project is not yet supported

                newProjects.Add(newProject);
            });

            return newProjects.ToList();
        }

        private async Task<Project?> TryBuildOrUpdateProject(Project? project, Repository repo)
        {
            var modules = await _moduleUpdater.UpdateModules(repo, project?.Modules ?? new List<Module>());

            if (!modules.Any()) return null; // No supported document was found.

            project ??= new Project
            {
                Source = repo.Source,
                ProjectReferenceId = repo.ProjectId,
                Name = repo.FullyDistinctName,
                RepositoryId = repo.Id,
                Actionables = new List<Actionable>()
            };

            project.Organisation = repo.Getter.Organisation;
            project.Branch = repo.Branch ?? "default";
            project.Modules = modules;
            project.ImportantTag = modules.FirstOrDefault(m => m.ImportantTag != null)?.ImportantTag;
            project.BiggestDiscrepancyLevel = modules.Any() ? modules.Max(p => p.BiggestDiscrepancyLevel) : DiscrepancyLevel.Latest;
            project.HighestVulnerabilitySeverity = modules.Max(p => p.HighestVulnerabilitySeverity);
            project.Actionables = _actionablesService.CheckForActionables(project);
            project.Pipelines = repo is DevopsRepositoryAdapter c ? await _pipelineAnalyzer.GetOrUpdatePipelineData(c.ProjectId, repo, project.Pipelines) : new List<Pipeline>();

            return project;
        }
    }
}
