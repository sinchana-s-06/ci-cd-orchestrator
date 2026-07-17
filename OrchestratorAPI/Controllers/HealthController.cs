using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace OrchestratorAPI.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            Log.Information("Health endpoint hit");

            return Ok(new { status = "Orchestrator Running" });
        }
    }
}