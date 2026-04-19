using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.Models
{
    public class ResearchArea
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // Navigation
        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}
