namespace VersionCheckerApi.Analysing.RepoGetter;

public interface IRepositoryGetter
{
    string Organisation { get; }

    Task<List<string>> GetOrganisationRepoIds();
    Task<Repository> GetRepository(string id, string? branch = null);
    Task<File> GetFile(Repository repo, string path);
    Task<string> SetUpWebhook(string projectId);
    Task<string?> GetWebhook(string projectId);
}
