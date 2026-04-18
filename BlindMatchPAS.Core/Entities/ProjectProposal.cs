using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.Core.Entities
{
    public class ProjectProposal
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150, MinimumLength = 5)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000, MinimumLength = 50)]
        public string Abstract { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[a-zA-Z0-9\s,\.#\+]+$",
            ErrorMessage = "Only valid tech stack characters allowed")]
        public string TechnicalStack { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";

        [Required]
        public string StudentId { get; set; } = string.Empty;
        public ApplicationUser Student { get; set; } = null!;

        public int ResearchAreaId { get; set; }
        public ResearchArea ResearchArea { get; set; } = null!;
    }
}