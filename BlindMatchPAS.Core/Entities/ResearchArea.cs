using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.Core.Entities
{
    public class ResearchArea
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        public ICollection<ProjectProposal> ProjectProposals { get; set; }
            = new List<ProjectProposal>();
    }
}