using Microsoft.AspNetCore.Mvc;
using OrchestratorAPI.Models;
using OrchestratorAPI.State;
using OrchestratorAPI.Analyzer;
using OrchestratorAPI.DecisionEngine;
using OrchestratorAPI.Execution;
using Serilog;
using OrchestratorAPI.Data;
namespace OrchestratorAPI.Controllers
{
    [ApiController]
    [Route("trigger")]
    public class TriggerController : ControllerBase
    {
        private readonly PipelineStateService _stateService;
        private readonly ChangeAnalyzer _analyzer;
        private readonly DecisionService _decision;
        private readonly PipelineExecutor _executor;
        private readonly AppDbContext _context;

        public TriggerController(
            PipelineStateService stateService,
            ChangeAnalyzer analyzer,
            DecisionService decision,
            PipelineExecutor executor,
            AppDbContext context)  
        {
            _stateService = stateService;
            _analyzer = analyzer;
            _decision = decision;
            _executor = executor;
            _context = context; 
        }

        [HttpPost]
   public async Task<IActionResult> TriggerPipeline([FromBody] ChangeData change)     
{
    // 🔹 Model validation check (automatic validation result)
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    // 🔹 Custom validation rules
    if (change.FilesChanged.Count != change.NumberOfFiles)
    {
        return BadRequest("NumberOfFiles does not match FilesChanged count");
    }

    if (change.DocsOnly && (change.BackendChanged || change.TestsChanged))
    {
        return BadRequest("DocsOnly cannot be true when backend or tests are changed");
    }

    Log.Information("Pipeline trigger received");

    var level = _analyzer.Analyze(change);
    var action = _decision.Decide(level);

   var run = new PipelineRun
{
    Status = "Initialized",

    // 🔹 Change Data mapping
    BackendChanged = change.BackendChanged,
    TestsChanged = change.TestsChanged,
    DocsOnly = change.DocsOnly,
    NumberOfFiles = change.NumberOfFiles,

    // 🔹 Decision mapping
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
    _context.SaveChanges();

    await _executor.ExecuteAsync(action, run);

    return Ok(new
    {
        Message = "Pipeline executed successfully",
        Level = level,
        Action = action,
        RunId = run.Id
    });
}
        [HttpGet("runs")]
        public IActionResult GetRuns()
        {
            return Ok(_stateService.GetAllRuns());
        }

        [HttpGet("run/{id}")]
        public IActionResult GetRunById(Guid id)
        {
            var run = _stateService.GetRunById(id);

            if (run == null)
            {
                return NotFound("Run not found");
            }

            return Ok(run);
        }
    }
}