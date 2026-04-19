using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.Models
{
    public enum UserRole
    {
        Student,
        Supervisor,
        Admin
    }

    public class ApplicationUser
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        // 🔹 NEW: Student fields
        [MaxLength(20)]
        public string? Batch { get; set; } // Example: 24.1

        [MaxLength(50)]
        public string? RegistrationNumber { get; set; }

        // 🔹 NEW: Supervisor fields
        [MaxLength(100)]
        public string? Department { get; set; }

        // 🔹 NEW: System control
        public bool IsActive { get; set; } = true;

        // 🔹 OPTIONAL: Simple Research Area assignment (for supervisor)
        public int? ResearchAreaId { get; set; }
        public ResearchArea? ResearchArea { get; set; }

        // Navigation
        public ICollection<Project> StudentProjects { get; set; } = new List<Project>();
        public ICollection<Project> SupervisedProjects { get; set; } = new List<Project>();
    }
}