using BlindMatchPAS.Core.Entities;
using BlindMatchPAS.Core.Services;
using BlindMatchPAS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Infrastructure.Services
{
    public class BlindMatchService : IBlindMatchService
    {
        private readonly ApplicationDbContext _context;

        public BlindMatchService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProjectProposal>> GetBlindProposalsAsync(
            int? researchAreaId = null)
        {
            var query = _context.ProjectProposals
                .Include(p => p.ResearchArea)
                .Where(p => p.Status == "Pending" ||
                            p.Status == "Under Review")
                .AsQueryable();

            if (researchAreaId.HasValue)
                query = query.Where(p => p.ResearchAreaId == researchAreaId);

            return await query
                .Select(p => new ProjectProposal
                {
                    Id = p.Id,
                    Title = p.Title,
                    Abstract = p.Abstract,
                    TechnicalStack = p.TechnicalStack,
                    Status = p.Status,
                    ResearchAreaId = p.ResearchAreaId,
                    ResearchArea = p.ResearchArea,
                    StudentId = string.Empty
                })
                .ToListAsync();
        }

        public async Task<MatchRecord> ExpressInterestAsync(
            string supervisorId, int proposalId)
        {
            var existing = await _context.MatchRecords
                .FirstOrDefaultAsync(m =>
                    m.ProjectProposalId == proposalId &&
                    m.SupervisorId == supervisorId);

            if (existing != null)
                return existing;

            var match = new MatchRecord
            {
                ProjectProposalId = proposalId,
                SupervisorId = supervisorId,
                IsConfirmed = false
            };

            _context.MatchRecords.Add(match);

            var proposal = await _context.ProjectProposals
                .FindAsync(proposalId);

            if (proposal != null && proposal.Status == "Pending")
                proposal.Status = "Under Review";

            await _context.SaveChangesAsync();
            return match;
        }

        public async Task<MatchRecord> ConfirmMatchAsync(
            string supervisorId, int matchId)
        {
            var match = await _context.MatchRecords
                .Include(m => m.ProjectProposal)
                .FirstOrDefaultAsync(m =>
                    m.Id == matchId &&
                    m.SupervisorId == supervisorId);

            if (match == null)
                throw new InvalidOperationException("Match not found!");

            if (match.IsConfirmed)
                return match;

            match.IsConfirmed = true;
            match.MatchedDate = DateTime.UtcNow;
            match.ProjectProposal!.Status = "Matched";

            await _context.SaveChangesAsync();
            return match;
        }

        public async Task<MatchRecord?> GetRevealedMatchAsync(
            string supervisorId, int matchId)
        {
            return await _context.MatchRecords
                .Include(m => m.Supervisor)
                .Include(m => m.ProjectProposal)
                    .ThenInclude(p => p!.Student)
                .Include(m => m.ProjectProposal)
                    .ThenInclude(p => p!.ResearchArea)
                .FirstOrDefaultAsync(m =>
                    m.Id == matchId &&
                    m.SupervisorId == supervisorId &&
                    m.IsConfirmed == true);
        }

        public async Task<bool> IsProposalMatchedAsync(int proposalId)
        {
            return await _context.MatchRecords
                .AnyAsync(m =>
                    m.ProjectProposalId == proposalId &&
                    m.IsConfirmed == true);
        }

        public async Task<IEnumerable<MatchRecord>> GetSupervisorMatchesAsync(
            string supervisorId)
        {
            return await _context.MatchRecords
                .Include(m => m.ProjectProposal)
                    .ThenInclude(p => p!.ResearchArea)
                .Where(m => m.SupervisorId == supervisorId)
                .OrderByDescending(m => m.MatchedDate)
                .ToListAsync();
        }

        public async Task<MatchRecord?> GetStudentMatchAsync(
            string studentId, int proposalId)
        {
            return await _context.MatchRecords
                .Include(m => m.Supervisor)
                .Include(m => m.ProjectProposal)
                .FirstOrDefaultAsync(m =>
                    m.ProjectProposal!.StudentId == studentId &&
                    m.ProjectProposalId == proposalId &&
                    m.IsConfirmed == true);
        }
    }
}