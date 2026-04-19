using System;
using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.Models
{
    public class RegistrationRequest
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        // Student fields
        public string? Batch { get; set; }
        public string? RegistrationNumber { get; set; }

        // Supervisor fields
        public string? Department { get; set; }
        public int? ResearchAreaId { get; set; }

        // Status
        public string Status { get; set; } = "Pending"; // Pending / Approved / Rejected

        public DateTime RequestedAt { get; set; } = DateTime.Now;
    }
}