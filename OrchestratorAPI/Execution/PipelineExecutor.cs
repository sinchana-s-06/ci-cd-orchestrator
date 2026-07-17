using System;
using System.Threading.Tasks;
using OrchestratorAPI.Models;
using OrchestratorAPI.Data;

namespace OrchestratorAPI.Execution
{
    public class PipelineExecutor
    {
        private readonly AppDbContext _context;
        private Random _random = new Random();

        public PipelineExecutor(AppDbContext context)
        {
            _context = context;
        }

        public async Task ExecuteAsync(string action, PipelineRun run)
        {
            Console.WriteLine($"Starting pipeline: {action}");

            int maxRetries = 2;

            while (run.RetryCount < maxRetries)
            {
                try
                {
                    run.Status = run.RetryCount == 0
                        ? "Running"
                        : $"Retrying ({run.RetryCount})";

                    _context.SaveChanges();

                    if (action == "FULL_PIPELINE")
                    {
                        await RunStage("Build", run);
                        await RunStage("Test", run, allowFailure: true);
                        await RunStage("Deploy", run);
                    }
                    else if (action == "BUILD_AND_TEST")
                    {
                        await RunStage("Build", run);
                        await RunStage("Test", run, allowFailure: true);
                    }
                    else
                    {
                        await RunStage("Quick Build", run);
                    }

                    run.Status = "Completed";
                    _context.SaveChanges();

                    Console.WriteLine($"Pipeline {run.Id} completed successfully");
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Pipeline failed: {ex.Message}");

                    run.RetryCount++;
                    _context.SaveChanges();

                    if (run.RetryCount >= maxRetries)
                    {
                        run.Status = "Failed";
                        _context.SaveChanges();

                        Console.WriteLine($"Pipeline {run.Id} permanently failed");
                        return;
                    }
                }
            }
        }

        // ✅ THIS METHOD MUST BE INSIDE THE CLASS
        private async Task RunStage(string stage, PipelineRun run, bool allowFailure = false)
        {
            // 🔹 Stage started
            run.Status = $"Running: {stage}";
            _context.SaveChanges();   // ✅ save immediately

            Console.WriteLine($"[Pipeline {run.Id}] {stage} started");

            // ⏳ delay so UI can see running
            await Task.Delay(6000);

            // 🔹 Failure simulation
            if (allowFailure && stage == "Test" && _random.Next(0, 4) == 0)
            {
                Console.WriteLine($"[Pipeline {run.Id}] {stage} FAILED");

                run.Status = $"Failed: {stage}";
                _context.SaveChanges();

                throw new Exception($"{stage} failed");
            }

            // 🔹 Stage completed
            run.Status = $"Completed: {stage}";
            _context.SaveChanges();

            Console.WriteLine($"[Pipeline {run.Id}] {stage} completed");

            // ⏳ smoother transition
            if (stage == "Deploy")
            {
                await Task.Delay(4000);
            }
            else
            {
                await Task.Delay(3000);
            }
        }   // closes RunStage

    }   // closes class PipelineExecutor
}       // closes namespace