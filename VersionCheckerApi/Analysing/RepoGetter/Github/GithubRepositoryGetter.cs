using Octokit;

namespace VersionCheckerApi.Analysing.RepoGetter.Github
{
    public class GithubRepositoryGetter : IRepositoryGetter
    {
        private readonly GitHubClient _client;
        private readonly IConfiguration _configuration;

        public GithubRepositoryGetter(IConfiguration configuration, RepositoryGetterAuthentication auth)
        {
            Organisation = auth.Org;
            _configuration = configuration;
            _client = new GitHubClient(new ProductHeaderValue(auth.User))
            {
                Credentials = new Credentials(auth.Auth)
            };
        }

        public string Organisation { get; }

        public async Task<List<string>> GetOrganisationRepoIds()
        {
            var repos = await _client.Repository.GetAllForOrg("****");
            return repos.Where(IsValidRepository).Select(r => r.Id.ToString()).ToList();
        }

        public async Task<Repository> GetRepository(string id, string? branch = null)
        {
            var repo = await _client.Repository.Get(long.Parse(id));
            branch ??= await GetHighestPriorityBranch(id) ?? repo.DefaultBranch;
            var paths = await GetFilePaths(long.Parse(id), branch!);

            return new GithubRepositoryAdapter(this, repo, branch, paths);
        }

        private async Task<List<string>> GetFilePaths(long repo, string branch)
        {
            var tree = await _client.Git.Tree.GetRecursive(repo, branch);

            var paths = tree.Tree.Select(t => t.Path).ToList();
            var foldersToSkip = _configuration.GetSection("FoldersToSkip").Get<string[]>();
            return paths.Where(p => !foldersToSkip.Any(p.Contains)).ToList();
        }

        public async Task<File> GetFile(Repository repo, string path)
        {
            var file = await _client.Repository.Content.GetAllContentsByRef(long.Parse(repo.Id), path, repo.Branch);
            return new File(path, file[0].Content);
        }

        public Task<string> SetUpWebhook(string projectId)
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetWebhook(string projectId)
        {
            throw new NotImplementedException();
        }

        public async Task<string?> GetHighestPriorityBranch(string id)
        {
            var branchesByPriority = _configuration.GetSection("BranchPriority").Get<string[]>();
            var branchesInRepo = await _client.Repository.Branch.GetAll(long.Parse(id));

            foreach (var branch in branchesByPriority)
            {
                var branchRef = branchesInRepo.FirstOrDefault(b => b.Name.EndsWith(branch));
                if (branchRef == null) continue;

                return branch;
            }

            // Use default branch
            return null;
        }

        public async Task<List<string>> GetAllOrganisations()
        {
            var z = await _client.Organization.GetAllForCurrent();
            var x = await _client.Organization.Get(z[0].Login);
            return z.Select(z => z.Name).ToList();
        }

        private bool IsValidRepository(Octokit.Repository repo)
        {
            return !repo.Archived && repo.Size != 0;
        }
    }
}
