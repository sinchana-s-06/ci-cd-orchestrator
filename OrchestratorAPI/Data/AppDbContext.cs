using Microsoft.EntityFrameworkCore;
using OrchestratorAPI.Models;

namespace OrchestratorAPI.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<PipelineRun> PipelineRuns { get; set; }
        public DbSet<PipelineFile> PipelineFiles { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
    }
}