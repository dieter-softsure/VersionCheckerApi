namespace VersionCheckerApi.Persistence.Models
{
    public class Actionable
    {
        public int Id { get; set; }
        public virtual Project Project { get; set; }
        public string Problem { get; set; }
        public string Action { get; set; }
        public Severity Severity { get; set; }
    }
}
