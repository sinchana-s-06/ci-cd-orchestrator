namespace OrchestratorAPI.Models
{
    public class PipelineRun
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Status { get; set; } = "Pending";

        // 🔹 Change Data (flattened)
        public bool BackendChanged { get; set; }
        public bool TestsChanged { get; set; }
        public bool DocsOnly { get; set; }
        public int NumberOfFiles { get; set; }

        // 🔹 Decision Data
        public string ChangeLevel { get; set; }
        public string Action { get; set; }

        // 🔹 Metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int RetryCount { get; set; } = 0;

        // 🔹 Files relation (Option B)
        public List<PipelineFile> Files { get; set; } = new();
    }
}