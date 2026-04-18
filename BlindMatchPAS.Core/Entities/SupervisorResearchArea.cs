using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.Core.Entities
{
    public class SupervisorResearchArea
    {
        public int Id { get; set; }

        [Required]
        public string SupervisorId { get; set; } = string.Empty;
        public ApplicationUser Supervisor { get; set; } = null!;

        [Required]
        public int ResearchAreaId { get; set; }
        public ResearchArea ResearchArea { get; set; } = null!;
    }
}