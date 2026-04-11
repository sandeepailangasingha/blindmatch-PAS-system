using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.Core.Entities
{
    public class MatchRecord
    {
        public int Id { get; set; }

        [Required]
        public int ProjectProposalId { get; set; }
        public ProjectProposal ProjectProposal { get; set; } = null!;

        [Required]
        public string SupervisorId { get; set; } = string.Empty;
        public ApplicationUser Supervisor { get; set; } = null!;

        public bool IsConfirmed { get; set; } = false;

        public DateTime? MatchedDate { get; set; }
    }
}