using OrchestratorAPI.Models;

namespace OrchestratorAPI.Analyzer
{
    public class ChangeAnalyzer
    {
        public string Analyze(ChangeData change)
        {
            if (change.DocsOnly)
                return "LOW";

            if (change.BackendChanged && change.TestsChanged)
                return "HIGH";

            if (change.NumberOfFiles > 5)
                return "MEDIUM";

            return "LOW";
        }
    }
}