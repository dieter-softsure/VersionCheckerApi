using VersionCheckerApi.Persistence.Models;

namespace VersionCheckerApi.Analysing.RepoGetter.Github
{
    public class GithubRepositoryAdapter : Repository
    {
        private readonly Octokit.Repository _repository;
        private readonly GithubRepositoryGetter _repositoryGetter;

        public GithubRepositoryAdapter(GithubRepositoryGetter getter, Octokit.Repository repository, string? branch, List<string> paths)
        {
            _repository = repository;
            Branch = branch;
            Paths = paths;
            _repositoryGetter = getter;
        }

        public override string Name => _repository.Name;
        public override string Id => _repository.Id.ToString();
        public override string? Branch { get; }
        public override Source Source => Source.Github;
        public override List<string> Paths { get; }

        public override string? ProjectId => null;
        public override string FullyDistinctName => Name;

        public override IRepositoryGetter Getter => _repositoryGetter;
    }
}
