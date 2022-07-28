namespace VersionCheckerApi.Persistence.Models
{
    public class Pipeline
    {
        public int PipelineId { get; set; }
        public int projectId { get; set; }
        public string Name { get; set; }
        public string Agent { get; set; }
        public string? VmImage { get; set; }
    }
}
