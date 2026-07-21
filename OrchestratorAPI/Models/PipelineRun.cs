namespace OrchestratorAPI.Models
{
    public class PipelineRun
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Status { get; set; } = "Pending";

        // Change data
        public bool BackendChanged { get; set; }

        public bool TestsChanged { get; set; }

        public bool DocsOnly { get; set; }

        public int NumberOfFiles { get; set; }

        // Decision data
        public string ChangeLevel { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        // GitHub workflow metadata
        public long? GitHubRunId { get; set; }

        public int? GitHubRunNumber { get; set; }

        public string? GitHubWorkflowName { get; set; }

        public string? GitHubRunUrl { get; set; }

        // GitHub job-stage statuses
        public string BuildStatus { get; set; } = "NotStarted";

        public string TestStatus { get; set; } = "NotApplicable";

        public string DeployStatus { get; set; } = "NotApplicable";

        // Metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        public int RetryCount { get; set; }

        // Files relation
        public List<PipelineFile> Files { get; set; } = new();
    }
}