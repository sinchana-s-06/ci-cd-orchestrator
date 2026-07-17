namespace OrchestratorAPI.GitHub
{
    public class GitHubOptions
    {
        public const string SectionName = "GitHub";

        public string Owner { get; set; } = string.Empty;

        public string Repository { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;

        public string Branch { get; set; } = "main";

        public string QuickBuildWorkflow { get; set; } =
            "quick-build.yml";

        public string BuildAndTestWorkflow { get; set; } =
            "build-and-test.yml";

        public string FullPipelineWorkflow { get; set; } =
            "full-pipeline.yml";
    }
}