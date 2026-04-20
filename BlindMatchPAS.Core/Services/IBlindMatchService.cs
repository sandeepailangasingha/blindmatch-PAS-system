using BlindMatchPAS.Core.Entities;

namespace BlindMatchPAS.Core.Services
{
    public interface IBlindMatchService
    {
        // Get proposals without student identity (Blind)
        Task<IEnumerable<ProjectProposal>> GetBlindProposalsAsync(
            int? researchAreaId = null);

        // Express interest in a proposal
        Task<MatchRecord> ExpressInterestAsync(
            string supervisorId, int proposalId);

        // Confirm match - triggers Identity Reveal
        Task<MatchRecord> ConfirmMatchAsync(
            string supervisorId, int matchId);

        // Get match with revealed identities
        Task<MatchRecord?> GetRevealedMatchAsync(
            string supervisorId, int matchId);

        // Check if proposal is already matched
        Task<bool> IsProposalMatchedAsync(int proposalId);

        // Get all matches for supervisor
        Task<IEnumerable<MatchRecord>> GetSupervisorMatchesAsync(
            string supervisorId);

        // Get match for student (after reveal)
        Task<MatchRecord?> GetStudentMatchAsync(
            string studentId, int proposalId);
    }
}