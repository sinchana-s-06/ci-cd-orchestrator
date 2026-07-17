using Microsoft.AspNetCore.Mvc;
using OrchestratorAPI.GitHub;

namespace OrchestratorAPI.Controllers
{
    [ApiController]
    [Route("api/github-actions")]
    public class GitHubActionsController : ControllerBase
    {
        private readonly GitHubActionsService _githubActionsService;
        private readonly ILogger<GitHubActionsController> _logger;

        public GitHubActionsController(
            GitHubActionsService githubActionsService,
            ILogger<GitHubActionsController> logger)
        {
            _githubActionsService = githubActionsService;
            _logger = logger;
        }

        [HttpPost("trigger/{pipelineAction}")]
        public async Task<IActionResult> TriggerWorkflow(
            string pipelineAction,
            CancellationToken cancellationToken)
        {
            try
            {
                var normalizedAction = pipelineAction
                    .Trim()
                    .Replace("-", "_")
                    .ToUpperInvariant();

                await _githubActionsService.TriggerWorkflowAsync(
                    normalizedAction,
                    cancellationToken);

                return Accepted(new
                {
                    Message = "GitHub Actions workflow triggered.",
                    Action = normalizedAction
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    Error = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    Error = ex.Message
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "GitHub Actions request failed");

                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    new
                    {
                        Error = "GitHub Actions request failed.",
                        Details = ex.Message
                    });
            }
        }
    }
}