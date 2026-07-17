using Microsoft.AspNetCore.Mvc;
using OrchestratorAPI.Analyzer;
using OrchestratorAPI.Data;
using OrchestratorAPI.DecisionEngine;
using OrchestratorAPI.GitHub;
using OrchestratorAPI.Models;
using OrchestratorAPI.State;
using Serilog;

namespace OrchestratorAPI.Controllers
{
    [ApiController]
    [Route("trigger")]
    public class TriggerController : ControllerBase
    {
        private readonly PipelineStateService _stateService;
        private readonly ChangeAnalyzer _analyzer;
        private readonly DecisionService _decision;
        private readonly GitHubActionsService _githubActionsService;
        private readonly AppDbContext _context;

        public TriggerController(
            PipelineStateService stateService,
            ChangeAnalyzer analyzer,
            DecisionService decision,
            GitHubActionsService githubActionsService,
            AppDbContext context)
        {
            _stateService = stateService;
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
                Action = action
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
                await _githubActionsService.TriggerWorkflowAsync(
                    action,
                    cancellationToken);

                run.Status = "Queued";
                await _context.SaveChangesAsync(cancellationToken);

                return Accepted(new
                {
                    Message = "Pipeline selected and dispatched to GitHub Actions.",
                    Level = level,
                    Action = action,
                    Status = run.Status,
                    RunId = run.Id
                });
            }
            catch (HttpRequestException ex)
            {
                run.Status = "DispatchFailed";
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
                        RunId = run.Id
                    });
            }
        }

        [HttpGet("runs")]
        public IActionResult GetRuns()
        {
            return Ok(_stateService.GetAllRuns());
        }

        [HttpGet("run/{id:guid}")]
        public IActionResult GetRunById(Guid id)
        {
            var run = _stateService.GetRunById(id);

            if (run == null)
            {
                return NotFound("Run not found.");
            }

            return Ok(run);
        }
    }
}