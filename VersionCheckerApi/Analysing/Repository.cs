using VersionCheckerApi.Analysing.RepoGetter;
using VersionCheckerApi.Persistence.Models;

namespace VersionCheckerApi.Analysing
{
    public abstract class Repository
    {
        public abstract string Id { get; }
        public abstract string Name { get; }
        public abstract string? Branch { get; }
        public abstract Source Source { get; }
        public abstract List<string> Paths { get; }

        public abstract string? ProjectId { get; }
        public abstract string FullyDistinctName { get; }

        public abstract IRepositoryGetter Getter { get; }

        public List<string> GetPathsWithNameEndingIn(string supportedFileExtension)
        {
            return Paths.Where(p => p.EndsWith(supportedFileExtension)).ToList();
        }

        public DirectoryPaths GetDirectory(string filePath)
        {
            var dir = filePath.LastIndexOf('/') != -1 ? filePath[..filePath.LastIndexOf('/')] : "";
            var directoryPaths = dir is "C:\\" or "root" ? Paths : Paths.Where(p => p.StartsWith(dir)).ToList();

            return new DirectoryPaths(Branch, dir, directoryPaths);
        }
    }

    public class DirectoryPaths
    {
        public string? Branch { get; }
        public string DirectoryName { get; }
        public List<string> Paths { get; }

        public DirectoryPaths(string? branch, string directoryName, List<string> paths)
        {
            Branch = branch;
            DirectoryName = directoryName;
            Paths = paths;
        }

        public string? GetBestPathForFile(string file)
        {
            return Paths.Where(p => p.EndsWith(file)).OrderBy(p => p.Length).FirstOrDefault();
        }
    }
}
