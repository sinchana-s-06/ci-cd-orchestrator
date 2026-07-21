using Microsoft.EntityFrameworkCore;
using OrchestratorAPI.Data;
using OrchestratorAPI.Models;

namespace OrchestratorAPI.GitHub
{
    public class GitHubRunSyncService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly GitHubStatusService _statusService;
        private readonly ILogger<GitHubRunSyncService> _logger;

        public GitHubRunSyncService(
            IServiceScopeFactory scopeFactory,
            GitHubStatusService statusService,
            ILogger<GitHubRunSyncService> logger)
        {
            _scopeFactory = scopeFactory;
            _statusService = statusService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "GitHub run synchronization service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SynchronizeRunsAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "An error occurred while synchronizing GitHub runs.");
                }

                try
                {
                    await Task.Delay(
                        TimeSpan.FromSeconds(5),
                        stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }

            _logger.LogInformation(
                "GitHub run synchronization service stopped.");
        }

        private async Task SynchronizeRunsAsync(
            CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var context =
                scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var activeRuns = await context.PipelineRuns
                .Where(run =>
                    run.GitHubRunId != null &&
                    run.Status != "DispatchFailed" &&
                    (
                        run.Status == "Queued" ||
                        run.Status == "Running" ||
                        run.Status == "Dispatching" ||
                        (
                            run.CompletedAt == null &&
                            (
                                run.Status == "Succeeded" ||
                                run.Status == "Failed" ||
                                run.Status == "Cancelled"
                            )
                        )
                    ))
                .ToListAsync(cancellationToken);

            foreach (var run in activeRuns)
            {
                try
                {
                    await SynchronizeRunAsync(
                        run,
                        cancellationToken);
                }
                catch (OperationCanceledException)
                    when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to synchronize pipeline run {RunId}.",
                        run.Id);
                }
            }

            if (context.ChangeTracker.HasChanges())
            {
                await context.SaveChangesAsync(cancellationToken);
            }
        }

        private async Task SynchronizeRunAsync(
            PipelineRun run,
            CancellationToken cancellationToken)
        {
            var workflowRun =
                await _statusService.GetWorkflowRunByIdAsync(
                    run.GitHubRunId!.Value,
                    cancellationToken);

            if (workflowRun is null)
            {
                _logger.LogWarning(
                    "GitHub run {GitHubRunId} was not found.",
                    run.GitHubRunId);

                return;
            }

            run.Status = MapGitHubStatus(
                workflowRun.Status,
                workflowRun.Conclusion);

            run.GitHubRunNumber = workflowRun.RunNumber;
            run.GitHubWorkflowName = workflowRun.Name;
            run.GitHubRunUrl = workflowRun.HtmlUrl;

            var jobs =
                await _statusService.GetWorkflowJobsAsync(
                    run.GitHubRunId.Value,
                    cancellationToken);

            UpdateStageStatuses(run, jobs);
            SetCompletionTime(run, jobs);

            _logger.LogInformation(
                "Pipeline run {RunId} synchronized. " +
                "Overall: {Status}, Build: {BuildStatus}, " +
                "Test: {TestStatus}, Deploy: {DeployStatus}.",
                run.Id,
                run.Status,
                run.BuildStatus,
                run.TestStatus,
                run.DeployStatus);
        }

        private static void SetCompletionTime(
            PipelineRun run,
            IReadOnlyCollection<GitHubJob> jobs)
        {
            if (run.CompletedAt != null ||
                !IsTerminalStatus(run.Status))
            {
                return;
            }

            run.CompletedAt =
                jobs
                    .Where(job => job.CompletedAt.HasValue)
                    .Select(job => job.CompletedAt)
                    .Max()
                ?? DateTime.UtcNow;
        }

        private static bool IsTerminalStatus(string? status)
        {
            return
                string.Equals(
                    status,
                    "Succeeded",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    status,
                    "Failed",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    status,
                    "Cancelled",
                    StringComparison.OrdinalIgnoreCase);
        }

        private static void UpdateStageStatuses(
            PipelineRun run,
            IReadOnlyCollection<GitHubJob> jobs)
        {
            SetApplicableStages(run);

            var buildJob = FindJob(jobs, "Build");
            var testJob = FindJob(jobs, "Test");
            var deployJob = FindJob(jobs, "Deploy");

            if (buildJob is not null)
            {
                run.BuildStatus = MapJobStatus(
                    buildJob.Status,
                    buildJob.Conclusion);
            }

            if (testJob is not null)
            {
                run.TestStatus = MapJobStatus(
                    testJob.Status,
                    testJob.Conclusion);
            }

            if (deployJob is not null)
            {
                run.DeployStatus = MapJobStatus(
                    deployJob.Status,
                    deployJob.Conclusion);
            }
        }

        private static GitHubJob? FindJob(
            IEnumerable<GitHubJob> jobs,
            string expectedName)
        {
            return jobs.FirstOrDefault(job =>
                string.Equals(
                    job.Name,
                    expectedName,
                    StringComparison.OrdinalIgnoreCase));
        }

        private static void SetApplicableStages(PipelineRun run)
        {
            run.BuildStatus = PreserveCurrentStatus(
                run.BuildStatus,
                "NotStarted");

            switch (run.Action?.Trim().ToUpperInvariant())
            {
                case "QUICK_BUILD":
                    run.TestStatus = "NotApplicable";
                    run.DeployStatus = "NotApplicable";
                    break;

                case "BUILD_AND_TEST":
                    run.TestStatus = PreserveCurrentStatus(
                        run.TestStatus,
                        "NotStarted");

                    run.DeployStatus = "NotApplicable";
                    break;

                case "FULL_PIPELINE":
                    run.TestStatus = PreserveCurrentStatus(
                        run.TestStatus,
                        "NotStarted");

                    run.DeployStatus = PreserveCurrentStatus(
                        run.DeployStatus,
                        "NotStarted");
                    break;

                default:
                    run.TestStatus = PreserveCurrentStatus(
                        run.TestStatus,
                        "NotStarted");

                    run.DeployStatus = PreserveCurrentStatus(
                        run.DeployStatus,
                        "NotStarted");
                    break;
            }
        }

        private static string PreserveCurrentStatus(
            string? currentStatus,
            string defaultStatus)
        {
            if (string.IsNullOrWhiteSpace(currentStatus) ||
                string.Equals(
                    currentStatus,
                    "NotApplicable",
                    StringComparison.OrdinalIgnoreCase))
            {
                return defaultStatus;
            }

            return currentStatus;
        }

        private static string MapJobStatus(
            string? status,
            string? conclusion)
        {
            if (string.Equals(
                status,
                "completed",
                StringComparison.OrdinalIgnoreCase))
            {
                return conclusion?.ToLowerInvariant() switch
                {
                    "success" => "Succeeded",
                    "failure" => "Failed",
                    "cancelled" => "Cancelled",
                    "skipped" => "Skipped",
                    "timed_out" => "Failed",
                    "action_required" => "Failed",
                    _ => "Failed"
                };
            }

            if (string.Equals(
                status,
                "in_progress",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Running";
            }

            if (string.Equals(
                    status,
                    "queued",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    status,
                    "waiting",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    status,
                    "pending",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Queued";
            }

            return "NotStarted";
        }

        private static string MapGitHubStatus(
            string? status,
            string? conclusion)
        {
            if (string.Equals(
                status,
                "completed",
                StringComparison.OrdinalIgnoreCase))
            {
                return conclusion?.ToLowerInvariant() switch
                {
                    "success" => "Succeeded",
                    "failure" => "Failed",
                    "cancelled" => "Cancelled",
                    "timed_out" => "Failed",
                    "action_required" => "Failed",
                    _ => "Failed"
                };
            }

            if (string.Equals(
                status,
                "in_progress",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Running";
            }

            return "Queued";
        }
    }
}