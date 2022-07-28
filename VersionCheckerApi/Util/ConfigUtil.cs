using VersionCheckerApi.Analysing.RepoGetter;
using VersionCheckerApi.Analysing.RepoGetter.Devops;
using VersionCheckerApi.Analysing.RepoGetter.Github;
using VersionCheckerApi.Persistence.Models;


namespace VersionCheckerApi.Util
{
    public static class ConfigUtil
    {
        public static IRepositoryGetter GetGetterFromConfig(this IConfiguration config, Project project)
        {
            var source = config.GetSection("Sources").Get<List<RepositoryGetterAuthentication>>().First(s => s.Org == project.Organisation || project.Organisation == "" && s.Default);
            return  source.Source == Source.Github
                ? new GithubRepositoryGetter(config, source)
                : new DevopsRepositoryGetter(config, source);
        }
    }
}
