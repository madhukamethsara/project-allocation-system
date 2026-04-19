using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.Models
{
    public class BatchAccess
    {
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string BatchName { get; set; } = string.Empty;

        public bool IsLoginEnabled { get; set; } = true;
    }
}