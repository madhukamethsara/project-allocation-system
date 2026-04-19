using System.ComponentModel.DataAnnotations;
using BlindMatchPAS.Models;

namespace BlindMatchPAS.Models.ViewModels
{
    // Auth
    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    // Admin create user
    public class AdminCreateUserViewModel
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        // Student fields
        [MaxLength(20)]
        public string? Batch { get; set; }

        [MaxLength(50)]
        public string? RegistrationNumber { get; set; }

        // Supervisor fields
        [MaxLength(100)]
        public string? Department { get; set; }

        public int? ResearchAreaId { get; set; }

        public bool IsActive { get; set; } = true;

        public List<ResearchArea> ResearchAreas { get; set; } = new();
    }

    // Project submission
    public class ProjectSubmitViewModel
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string TechStack { get; set; } = string.Empty;

        [Required]
        public int ResearchAreaId { get; set; }

        public List<ResearchArea> ResearchAreas { get; set; } = new();
    }

    // Supervisor's blind view of a project
    public class BlindProjectViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TechStack { get; set; } = string.Empty;
        public string ResearchAreaName { get; set; } = string.Empty;
        public ProjectStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? RevealedStudentName { get; set; }
        public string? RevealedStudentEmail { get; set; }

        public bool IsMatched { get; set; }

        // NEW
        public bool IsPinned { get; set; }
        public bool NeedsRevision { get; set; }
        public int FeedbackCount { get; set; }
    }

    // NEW: Feedback submit model
    public class ProjectFeedbackInputViewModel
    {
        [Required]
        public int ProjectId { get; set; }

        [Required, MaxLength(2000)]
        public string Comment { get; set; } = string.Empty;
    }

    // NEW: Feedback display model
    public class ProjectFeedbackViewModel
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int SupervisorId { get; set; }
        public string SupervisorName { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // NEW: Pinned project display model
    public class PinnedProjectViewModel
    {
        public int ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TechStack { get; set; } = string.Empty;
        public string ResearchAreaName { get; set; } = string.Empty;
        public ProjectStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime PinnedAt { get; set; }
        public bool NeedsRevision { get; set; }
    }

    // NEW: Student project feedback panel
    public class StudentProjectFeedbackViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;
        public ProjectStatus Status { get; set; }
        public bool NeedsRevision { get; set; }
        public List<ProjectFeedbackViewModel> Feedbacks { get; set; } = new();
    }

    // Admin overview
    public class MatchOverviewViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string SupervisorName { get; set; } = string.Empty;
        public string SupervisorEmail { get; set; } = string.Empty;
        public string ResearchArea { get; set; } = string.Empty;
        public DateTime? MatchedAt { get; set; }
    }

    public class AdminUserManagementPageViewModel
    {
        public AdminCreateUserViewModel NewStudent { get; set; } = new();
        public AdminCreateUserViewModel NewSupervisor { get; set; } = new();

        public List<ApplicationUser> Users { get; set; } = new();
        public List<ResearchArea> ResearchAreas { get; set; } = new();
        public List<string> Batches { get; set; } = new();
    }

    public class StudentProfileEditViewModel
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Batch { get; set; } = "";
        public string RegistrationNumber { get; set; } = "";
    }

    public class RegisterViewModel
    {
        [Required]
        public string Name { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required]
        public UserRole Role { get; set; }

        public string? Batch { get; set; }
        public string? RegistrationNumber { get; set; }

        public string? Department { get; set; }
        public int? ResearchAreaId { get; set; }

        public List<ResearchArea> ResearchAreas { get; set; } = new();
    }
}