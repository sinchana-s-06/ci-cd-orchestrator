namespace OrchestratorAPI.Models
{
    public class TriggerRequest
    {
        public required string RepoName { get; set; }

        public required string Branch { get; set; }

        public int FilesChanged { get; set; }
    }
}