using OrchestratorAPI.Models;
using OrchestratorAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace OrchestratorAPI.State
{
    public class PipelineStateService
    {
        private readonly AppDbContext _context;

        public PipelineStateService(AppDbContext context)
        {
            _context = context;
        }

        public List<PipelineRun> GetAllRuns()
        {
            return _context.PipelineRuns
                .Include(r => r.Files)
                .ToList();
        }

        public PipelineRun AddRun(PipelineRun run)
        {
            _context.PipelineRuns.Add(run);
            _context.SaveChanges();
            return run;
        }

        public PipelineRun? GetRunById(Guid id)
        {
            return _context.PipelineRuns
                .Include(r => r.Files)
                .FirstOrDefault(r => r.Id == id);
        }
    }
}