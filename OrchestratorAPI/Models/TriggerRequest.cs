namespace OrchestratorAPI.Models
{
    public class TriggerRequest
    {
        public string RepoName { get; set; }
        public string Branch { get; set; }
        public int FilesChanged { get; set; }
    }
}