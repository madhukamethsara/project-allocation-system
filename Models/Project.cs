using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlindMatchPAS.Models
{
    public enum ProjectStatus
    {
        Pending,
        UnderReview,
        Matched,
        Withdrawn
    }

    public class Project
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string TechStack { get; set; } = string.Empty;

        [Required]
        public ProjectStatus Status { get; set; } = ProjectStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? MatchedAt { get; set; }

        // FK: Student
        [Required]
        public int StudentId { get; set; }

        [ForeignKey(nameof(StudentId))]
        public ApplicationUser? Student { get; set; }

        // FK: Supervisor (null until matched)
        public int? SupervisorId { get; set; }

        [ForeignKey(nameof(SupervisorId))]
        public ApplicationUser? Supervisor { get; set; }

        // FK: ResearchArea
        [Required]
        public int ResearchAreaId { get; set; }

        [ForeignKey(nameof(ResearchAreaId))]
        public ResearchArea? ResearchArea { get; set; }

        // NEW: Revision flag
        public bool NeedsRevision { get; set; } = false;

        // NEW: Navigation collections
        public ICollection<ProjectFeedback> Feedbacks { get; set; } = new List<ProjectFeedback>();
        public ICollection<PinnedProject> PinnedBy { get; set; } = new List<PinnedProject>();
    }
}