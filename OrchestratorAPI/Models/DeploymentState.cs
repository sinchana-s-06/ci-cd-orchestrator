namespace OrchestratorAPI.Models
{
    public class DeploymentState
    {
        public string CurrentVersion { get; set; } = "v1";
        public string PreviousVersion { get; set; } = "v0";

        public DateTime LastDeployedAt { get; set; } = DateTime.UtcNow;
    }
}