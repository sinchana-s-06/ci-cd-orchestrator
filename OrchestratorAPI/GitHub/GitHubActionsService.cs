using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace OrchestratorAPI.GitHub
{
    public class GitHubActionsService
    {
        private readonly HttpClient _httpClient;
        private readonly GitHubOptions _options;
        private readonly ILogger<GitHubActionsService> _logger;

        public GitHubActionsService(
            HttpClient httpClient,
            IOptions<GitHubOptions> options,
            ILogger<GitHubActionsService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task TriggerWorkflowAsync(
            string action,
            CancellationToken cancellationToken = default)
        {
            ValidateConfiguration();

            var workflowFile = GetWorkflowFile(action);

            var endpoint =
                $"repos/{_options.Owner}/{_options.Repository}/actions/workflows/{workflowFile}/dispatches";

            var payload = new
            {
                @ref = _options.Branch
            };

            _logger.LogInformation(
                "Triggering GitHub workflow {WorkflowFile} for branch {Branch}",
                workflowFile,
                _options.Branch);

            using var response = await _httpClient.PostAsJsonAsync(
                endpoint,
                payload,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(
                    cancellationToken);

                _logger.LogError(
                    "GitHub workflow trigger failed. Status: {StatusCode}. Response: {Response}",
                    response.StatusCode,
                    errorBody);

                throw new HttpRequestException(
                    $"GitHub workflow trigger failed with status {(int)response.StatusCode}: {errorBody}");
            }

            _logger.LogInformation(
                "GitHub workflow {WorkflowFile} was triggered successfully",
                workflowFile);
        }

        private string GetWorkflowFile(string action)
        {
            return action switch
            {
                "FULL_PIPELINE" => _options.FullPipelineWorkflow,
                "BUILD_AND_TEST" => _options.BuildAndTestWorkflow,
                "QUICK_BUILD" => _options.QuickBuildWorkflow,
                _ => throw new ArgumentException(
                    $"Unsupported pipeline action: {action}",
                    nameof(action))
            };
        }

        private void ValidateConfiguration()
        {
            if (string.IsNullOrWhiteSpace(_options.Owner))
            {
                throw new InvalidOperationException(
                    "GitHub owner is not configured.");
            }

            if (string.IsNullOrWhiteSpace(_options.Repository))
            {
                throw new InvalidOperationException(
                    "GitHub repository is not configured.");
            }

            if (string.IsNullOrWhiteSpace(_options.Token))
            {
                throw new InvalidOperationException(
                    "GitHub token is not configured.");
            }
        }
    }
}