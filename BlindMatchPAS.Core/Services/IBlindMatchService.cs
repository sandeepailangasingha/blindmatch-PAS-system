using BlindMatchPAS.Core.Entities;

namespace BlindMatchPAS.Core.Services
{
    public interface IBlindMatchService
    {
        Task<IEnumerable<ProjectProposal>> GetBlindProposalsAsync(
            int? researchAreaId = null);

        Task<MatchRecord> ExpressInterestAsync(
            string supervisorId, int proposalId);

        Task<MatchRecord> ConfirmMatchAsync(
            string supervisorId, int matchId);

        Task<MatchRecord?> GetRevealedMatchAsync(
            string supervisorId, int matchId);

        Task<bool> IsProposalMatchedAsync(int proposalId);

        Task<IEnumerable<MatchRecord>> GetSupervisorMatchesAsync(
            string supervisorId);

        Task<MatchRecord?> GetStudentMatchAsync(
            string studentId, int proposalId);
    }
}