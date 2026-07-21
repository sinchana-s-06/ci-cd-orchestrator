using System.Text.Json.Serialization;

namespace OrchestratorAPI.Models
{
    public class PipelineFile
    {
        public int Id { get; set; }

        public Guid RunId { get; set; }

        [JsonIgnore]
        public PipelineRun? Run { get; set; }

        public required string FileName { get; set; }
    }
}