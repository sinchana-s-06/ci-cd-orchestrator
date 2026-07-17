using System.ComponentModel.DataAnnotations;

namespace OrchestratorAPI.Models
{
    public class ChangeData
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one file must be provided")]
        public List<string> FilesChanged { get; set; } = new();

        [Range(1, int.MaxValue, ErrorMessage = "NumberOfFiles must be greater than 0")]
        public int NumberOfFiles { get; set; }

        public bool BackendChanged { get; set; }
        public bool TestsChanged { get; set; }
        public bool DocsOnly { get; set; }
    }
}