using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.FormInput;
using Microsoft.VisualStudio.Services.ServiceHooks.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace VersionCheckerApi.Analysing.RepoGetter.Devops
{
    public class DevopsRepositoryGetter : IRepositoryGetter, IDisposable
    {
        private readonly IConfiguration _configuration;

        private readonly VssConnection _connection;
        private readonly GitHttpClient _client;
        private readonly ServiceHooksPublisherHttpClient _hooks;
        private readonly BuildHttpClient _build;

        public DevopsRepositoryGetter(IConfiguration configuration, RepositoryGetterAuthentication auth)
        {
            _configuration = configuration;
            Organisation = auth.Org;

            var creds = new VssBasicCredential(string.Empty, auth.Auth);
            _connection = new VssConnection(new Uri("https://dev.azure.com/" + auth.Org + "/"), creds);

            _client = _connection.GetClient<GitHttpClient>();
            _hooks = _connection.GetClient<ServiceHooksPublisherHttpClient>();
            _build = _connection.GetClient<BuildHttpClient>();
        }

        public string Organisation { get; }

        public async Task<List<string>> GetOrganisationRepoIds()
        {
            var repos = await _client.GetRepositoriesAsync();

            return repos.Where(IsValidRepository).Select(r => r.Id.ToString()).ToList();
        }

        public async Task<Repository> GetRepository(string id, string? branch = null)
        {
            var repo = await _client.GetRepositoryAsync(id);
            branch ??= await GetHighestPriorityBranch(repo.Id.ToString());
            var paths = await GetFilePaths(repo.Id.ToString(), branch);
            return new DevopsRepositoryAdapter(this, repo, branch, paths);
        }

        public async Task<Repository> GetRepositoryByName(string name, string? branch = null)
        {
            var repos = await _client.GetRepositoriesAsync();
            return await GetRepository(repos.First(r => r.Name == name).Id.ToString(), branch);
        }

        public async Task<List<string>> GetFilePaths(string repoId, string? branch)
        {
            var items = await _client.GetItemsAsync(repoId, null, VersionControlRecursionType.Full, versionDescriptor: ToVersionDescriptor(branch));

            var foldersToSkip = _configuration.GetSection("FoldersToSkip").Get<string[]>();
            return items.Select(i => i.Path).Where(p => !foldersToSkip.Any(p.Contains)).ToList();
        }

        public async Task<List<File>> GetFiles(Repository repo, List<string> filePaths)
        {
            var files = new List<GitItem>();
            foreach (var packageFilePath in filePaths)
            {
                files.Add(await _client.GetItemAsync(repo.Id, packageFilePath, includeContent: true, versionDescriptor: ToVersionDescriptor(repo.Branch)));
            }

            return files.Select(f => new File(f.Path, f.Content)).ToList();
        }

        public async Task<File> GetFile(Repository repo, string path)
        {
            var file = await _client.GetItemAsync(repo.Id, path, includeContent: true, versionDescriptor: ToVersionDescriptor(repo.Branch));

            return new File(file.Path, file.Content);
        }

        public async Task<List<(string name, string file)>> GetPipelines(string projectId, string repoId)
        {
            var builds = await _build.GetFullDefinitionsAsync(Guid.Parse(projectId));

            var inRepo = builds.Where(b => b.Repository.Id == repoId);
            var result = new List<(string name, string file)>();

            foreach (var pipeline in inRepo)
            {
                if (pipeline.Process is YamlProcess y)
                {
                    result.Add((pipeline.Name, y.YamlFilename));
                }
            }

            return result;
        }

        public async Task<string?> GetHighestPriorityBranch(string id)
        {
            var branchesByPriority = _configuration.GetSection("BranchPriority").Get<string[]>();
            var branchesInRepo = await _client.GetBranchRefsAsync(Guid.Parse(id));

            foreach (var branch in branchesByPriority)
            {
                var branchRef = branchesInRepo.FirstOrDefault(b => b.Name.EndsWith(branch));
                if (branchRef == null) continue;

                return branch;
            }

            // Use default branch
            return null;
        }

        public async Task<string> SetUpWebhook(string projectId)
        {
            var subscriptionParameters = new Subscription
            {
                ConsumerId = "webHooks",
                ConsumerActionId = "httpRequest",
                EventType = "git.pullrequest.merged",
                ConsumerInputs = new Dictionary<string, string>
                {
                    { "url", "****" }
                },
                PublisherId = "tfs",
                PublisherInputs = new Dictionary<string, string>
                {
                    { "projectId", projectId }
                },
            };

            var subscription = await _hooks.CreateSubscriptionAsync(subscriptionParameters);
            return subscription.Id.ToString();
        }

        public async Task<string?> GetWebhook(string projectId)
        {
            var hooks = await _hooks.QuerySubscriptionsAsync(new SubscriptionsQuery
            {
                PublisherInputFilters = new List<InputFilter>
                {
                    new InputFilter
                    {
                        Conditions = new List<InputFilterCondition>
                        {
                            new InputFilterCondition
                            {
                                InputId = "projectId",
                                InputValue = projectId
                            }
                        }
                    }
                }
            });

            return hooks.Results.FirstOrDefault(h =>
            {
                if (!h.ConsumerInputs.TryGetValue("url", out var url)) return false;
                return url == "****";
            })?.Id.ToString();
        }

        private static bool IsValidRepository(GitRepository repo) => repo.DefaultBranch != null;

        private GitVersionDescriptor? ToVersionDescriptor(string? branchName)
        {
            return branchName != null ? new GitVersionDescriptor
            {
                Version = branchName,
                VersionType = GitVersionType.Branch
            } : null;
        }

        public void Dispose()
        {
            _connection.Dispose();
            _client.Dispose();
        }
    }
}
