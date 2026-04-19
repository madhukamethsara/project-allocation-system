using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.Models
{
    public class PinnedProject
    {
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        [Required]
        public int SupervisorId { get; set; }
        public ApplicationUser? Supervisor { get; set; }

        public DateTime PinnedAt { get; set; } = DateTime.UtcNow;
    }
}