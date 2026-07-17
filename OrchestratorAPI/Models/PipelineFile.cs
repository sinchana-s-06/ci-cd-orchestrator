using System.Text.Json.Serialization;

namespace OrchestratorAPI.Models
{
    public class PipelineFile
    {
        public int Id { get; set; }

        public Guid RunId { get; set; }

        [JsonIgnore]   // 🔥 FIXES CIRCULAR LOOP
        public PipelineRun Run { get; set; }

        public string FileName { get; set; }
    }
}