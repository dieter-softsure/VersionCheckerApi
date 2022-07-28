namespace VersionCheckerApi.Analysing.Pipelines
{
    public class PipelineYaml
    {
        public Pool? Pool { get; set; }
        public Queue? Queue { get; set; }
        public List<Job>? Jobs { get; set; }
    }
    public class PipelineYamlFull : PipelineYaml
    {
        public Resources? Resources { get; set; }
    }

    public class PipelineYamlSelf : PipelineYaml
    {
        public List<Resource> Resources { get; set; }
    }

    public class Pool
    {
        public string Name { get; set; }
        public string? Demands { get; set; }
        public string? VmImage { get; set; }
        public string? Vmimage { get; set; }
    }

    public class Queue
    {
        public string Name { get; set; }
        public string? Demands { get; set; }
        public string? VmImage { get; set; }
    }

    public class Job
    {
        public string? Template { get; set; }
        public Pool? Pool { get; set; }
    }

    public class Resources
    {
        public string Repo { get; set; }
        public List<RepositorySingle>? Repositories { get; set; }
    }

    public class Resource
    {
        public string Repo { get; set; }
    }

    public class RepositorySingle
    {
        public string Repository { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Ref { get; set; }
    }
}
