using System.ComponentModel.DataAnnotations;

namespace OrchestratorAPI.GitHub
{
    public class GitHubOptions
    {
        public const string SectionName = "GitHub";

        [Required]
        public string Owner { get; set; } = string.Empty;

        [Required]
        public string Repository { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        public string Branch { get; set; } = "main";

        [Required]
        public string QuickBuildWorkflow { get; set; } =
            "quick-build.yml";

        [Required]
        public string BuildAndTestWorkflow { get; set; } =
            "build-and-test.yml";

        [Required]
        public string FullPipelineWorkflow { get; set; } =
            "full-pipeline.yml";

       [Range(5, 3600)]
     public int SyncIntervalSeconds { get; set; } = 30;
    }
}