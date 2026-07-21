using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrchestratorAPI.Analyzer;
using OrchestratorAPI.Data;
using OrchestratorAPI.DecisionEngine;
using OrchestratorAPI.GitHub;
using OrchestratorAPI.Models;
using Serilog;

namespace OrchestratorAPI.Controllers
{
    [ApiController]
    [Route("trigger")]
    public class TriggerController : ControllerBase
    {
        private readonly ChangeAnalyzer _analyzer;
        private readonly DecisionService _decision;
        private readonly GitHubActionsService _githubActionsService;
        private readonly AppDbContext _context;

        public TriggerController(
            ChangeAnalyzer analyzer,
            DecisionService decision,
            GitHubActionsService githubActionsService,
            AppDbContext context)
        {
            _analyzer = analyzer;
            _decision = decision;
            _githubActionsService = githubActionsService;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> TriggerPipeline(
            [FromBody] ChangeData change,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (change.FilesChanged.Count != change.NumberOfFiles)
            {
                return BadRequest(
                    "NumberOfFiles does not match FilesChanged count.");
            }

            if (change.DocsOnly &&
                (change.BackendChanged || change.TestsChanged))
            {
                return BadRequest(
                    "DocsOnly cannot be true when backend or tests are changed.");
            }

            Log.Information("Intelligent pipeline trigger received.");

            var level = _analyzer.Analyze(change);
            var action = _decision.Decide(level);

            var run = new PipelineRun
            {
                Status = "Dispatching",
                BackendChanged = change.BackendChanged,
                TestsChanged = change.TestsChanged,
                DocsOnly = change.DocsOnly,
                NumberOfFiles = change.NumberOfFiles,
                ChangeLevel = level,
                Action = action,
                BuildStatus = "NotStarted",
                TestStatus = GetInitialTestStatus(action),
                DeployStatus = GetInitialDeployStatus(action)
            };

            foreach (var file in change.FilesChanged)
            {
                run.Files.Add(new PipelineFile
                {
                    FileName = file
                });
            }

            _context.PipelineRuns.Add(run);
            await _context.SaveChangesAsync(cancellationToken);

            try
            {
                var workflowRun =
                    await _githubActionsService.TriggerWorkflowAsync(
                        action,
                        cancellationToken);

                if (workflowRun is not null)
                {
                    run.GitHubRunId = workflowRun.Id;
                    run.GitHubRunNumber = workflowRun.RunNumber;
                    run.GitHubWorkflowName = workflowRun.Name;
                    run.GitHubRunUrl = workflowRun.HtmlUrl;

                    run.Status = MapGitHubStatus(
                        workflowRun.Status,
                        workflowRun.Conclusion);
                }
                else
                {
                    run.Status = "Queued";
                }

                await _context.SaveChangesAsync(cancellationToken);

                return Accepted(new
                {
                    Message =
                        "Pipeline selected and dispatched to GitHub Actions.",
                    Level = level,
                    Action = action,
                    Status = run.Status,
                    BuildStatus = run.BuildStatus,
                    TestStatus = run.TestStatus,
                    DeployStatus = run.DeployStatus,
                    RunId = run.Id,
                    GitHubRunId = run.GitHubRunId,
                    GitHubRunNumber = run.GitHubRunNumber,
                    GitHubWorkflowName = run.GitHubWorkflowName,
                    GitHubRunUrl = run.GitHubRunUrl
                });
            }
            catch (HttpRequestException ex)
            {
                run.Status = "DispatchFailed";
                run.BuildStatus = "Failed";

                await _context.SaveChangesAsync(cancellationToken);

                Log.Error(
                    ex,
                    "Failed to dispatch pipeline run {RunId}",
                    run.Id);

                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    new
                    {
                        Error = "GitHub Actions workflow dispatch failed.",
                        Details = ex.Message,
                        Level = level,
                        Action = action,
                        Status = run.Status,
                        BuildStatus = run.BuildStatus,
                        TestStatus = run.TestStatus,
                        DeployStatus = run.DeployStatus,
                        RunId = run.Id
                    });
            }
        }

        [HttpGet("runs")]
        public async Task<IActionResult> GetRuns(
            CancellationToken cancellationToken)
        {
            var runs = await _context.PipelineRuns
                .AsNoTracking()
                .Include(run => run.Files)
                .OrderByDescending(run => run.CreatedAt)
                .Select(run => new
                {
                    run.Id,
                    run.Status,
                    run.BuildStatus,
                    run.TestStatus,
                    run.DeployStatus,

                    run.BackendChanged,
                    run.TestsChanged,
                    run.DocsOnly,
                    run.NumberOfFiles,

                    run.ChangeLevel,
                    run.Action,

                    run.GitHubRunId,
                    run.GitHubRunNumber,
                    run.GitHubWorkflowName,
                    run.GitHubRunUrl,

                    run.CreatedAt,
                    run.CompletedAt,
                    run.RetryCount,

                    Files = run.Files.Select(file => new
                    {
                        file.Id,
                        file.FileName
                    })
                })
                .ToListAsync(cancellationToken);

            return Ok(runs);
        }

        [HttpGet("run/{id:guid}")]
        public async Task<IActionResult> GetRunById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var run = await _context.PipelineRuns
                .AsNoTracking()
                .Include(item => item.Files)
                .Where(item => item.Id == id)
                .Select(item => new
                {
                    item.Id,
                    item.Status,
                    item.BuildStatus,
                    item.TestStatus,
                    item.DeployStatus,

                    item.BackendChanged,
                    item.TestsChanged,
                    item.DocsOnly,
                    item.NumberOfFiles,

                    item.ChangeLevel,
                    item.Action,

                    item.GitHubRunId,
                    item.GitHubRunNumber,
                    item.GitHubWorkflowName,
                    item.GitHubRunUrl,

                    item.CreatedAt,
                    item.CompletedAt,
                    item.RetryCount,

                    Files = item.Files.Select(file => new
                    {
                        file.Id,
                        file.FileName
                    })
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (run is null)
            {
                return NotFound("Run not found.");
            }

            return Ok(run);
        }

        private static string GetInitialTestStatus(string action)
        {
            return string.Equals(
                action,
                "QUICK_BUILD",
                StringComparison.OrdinalIgnoreCase)
                    ? "NotApplicable"
                    : "NotStarted";
        }

        private static string GetInitialDeployStatus(string action)
        {
            return string.Equals(
                action,
                "FULL_PIPELINE",
                StringComparison.OrdinalIgnoreCase)
                    ? "NotStarted"
                    : "NotApplicable";
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
                return string.Equals(
                    conclusion,
                    "success",
                    StringComparison.OrdinalIgnoreCase)
                        ? "Succeeded"
                        : "Failed";
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