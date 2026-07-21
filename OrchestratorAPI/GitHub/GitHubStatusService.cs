using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace OrchestratorAPI.GitHub
{
    public class GitHubStatusService
    {
        private readonly HttpClient _httpClient;
        private readonly GitHubOptions _options;
        private readonly ILogger<GitHubStatusService> _logger;

        public GitHubStatusService(
            HttpClient httpClient,
            IOptions<GitHubOptions> options,
            ILogger<GitHubStatusService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<GitHubWorkflowRun?> GetLatestWorkflowRunAsync(
            string workflowFile,
            CancellationToken cancellationToken = default)
        {
            var endpoint =
                $"repos/{_options.Owner}/{_options.Repository}" +
                $"/actions/workflows/{workflowFile}/runs" +
                $"?branch={Uri.EscapeDataString(_options.Branch)}" +
                "&event=workflow_dispatch" +
                "&per_page=1";

            _logger.LogInformation(
                "Fetching latest workflow run for {Workflow} on branch {Branch}",
                workflowFile,
                _options.Branch);

            try
            {
                var response =
                    await _httpClient
                        .GetFromJsonAsync<GitHubWorkflowRunsResponse>(
                            endpoint,
                            cancellationToken);

                return response?.WorkflowRuns.FirstOrDefault();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to fetch latest GitHub workflow run for {Workflow}",
                    workflowFile);

                throw;
            }
        }

        public async Task<GitHubWorkflowRun?> GetWorkflowRunByIdAsync(
            long runId,
            CancellationToken cancellationToken = default)
        {
            var endpoint =
                $"repos/{_options.Owner}/{_options.Repository}" +
                $"/actions/runs/{runId}";

            _logger.LogInformation(
                "Fetching GitHub workflow run {RunId}",
                runId);

            try
            {
                return await _httpClient
                    .GetFromJsonAsync<GitHubWorkflowRun>(
                        endpoint,
                        cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to fetch GitHub workflow run {RunId}",
                    runId);

                throw;
            }
        }

        public async Task<List<GitHubJob>> GetWorkflowJobsAsync(
            long runId,
            CancellationToken cancellationToken = default)
        {
            var endpoint =
                $"repos/{_options.Owner}/{_options.Repository}" +
                $"/actions/runs/{runId}/jobs";

            _logger.LogInformation(
                "Fetching GitHub jobs for run {RunId}",
                runId);

            try
            {
                var response =
                    await _httpClient
                        .GetFromJsonAsync<GitHubJobsResponse>(
                            endpoint,
                            cancellationToken);

                return response?.Jobs ?? new List<GitHubJob>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to fetch GitHub jobs for run {RunId}",
                    runId);

                throw;
            }
        }
    }
}