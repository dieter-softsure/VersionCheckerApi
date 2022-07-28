using VersionCheckerApi.Persistence.Models;

namespace VersionCheckerApi.Analysing.RepoGetter
{
    public class RepositoryGetterAuthentication
    {
        public Source Source { get; set; }
        public string Org { get; set; }
        public string User { get; set; }
        public string Auth { get; set; }
        public bool Default { get; set; }
    }
}
