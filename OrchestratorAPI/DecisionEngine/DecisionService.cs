namespace OrchestratorAPI.DecisionEngine
{
    public class DecisionService
    {
        public string Decide(string changeLevel)
        {
            return changeLevel switch
            {
                "HIGH" => "FULL_PIPELINE",
                "MEDIUM" => "BUILD_AND_TEST",
                _ => "QUICK_BUILD"
            };
        }
    }
}