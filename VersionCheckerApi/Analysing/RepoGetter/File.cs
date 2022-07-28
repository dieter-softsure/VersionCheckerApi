namespace VersionCheckerApi.Analysing.RepoGetter
{
    public class File
    {
        public File(string path, string content)
        {
            Path = path;
            Content = content;
        }

        public string Path { get; }
        public string DirectoryName => new DirectoryInfo(Path).Parent!.Name is "C:\\" or "VersionCheckerApi" ? "root" : new DirectoryInfo(Path).Parent!.Name;
        public string Content { get; }
    }
}
