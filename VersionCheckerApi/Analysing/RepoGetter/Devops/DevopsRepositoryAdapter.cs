using Microsoft.TeamFoundation.SourceControl.WebApi;
using VersionCheckerApi.Persistence.Models;

namespace VersionCheckerApi.Analysing.RepoGetter.Devops;

public class DevopsRepositoryAdapter : Repository
{
    private readonly GitRepository _repository;
    private readonly DevopsRepositoryGetter _repositoryGetter;

    public DevopsRepositoryAdapter(DevopsRepositoryGetter getter, GitRepository repository, string? branch, List<string> paths)
    {
        _repository = repository;
        Branch = branch;
        Paths = paths;
        _repositoryGetter = getter;
    }
    public override string Name => _repository.Name;
    public override string Id => _repository.Id.ToString();
    public override string? Branch { get; }
    public override Source Source => Source.Devops;
    public override List<string> Paths { get; }
    

    public override string ProjectId => _repository.ProjectReference.Id.ToString();
    public override string FullyDistinctName => Name == _repository.ProjectReference.Name ? Name : _repository.ProjectReference.Name + ": " + Name;

    public override IRepositoryGetter Getter => _repositoryGetter;
}

