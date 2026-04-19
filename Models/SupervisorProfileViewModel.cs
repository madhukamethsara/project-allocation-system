using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.Models.ViewModels
{
    public class SupervisorProfileViewModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Department { get; set; }

        [Required(ErrorMessage = "Please select a research area.")]
        public int? ResearchAreaId { get; set; }

        public List<ResearchArea> ResearchAreas { get; set; } = new();
    }
}