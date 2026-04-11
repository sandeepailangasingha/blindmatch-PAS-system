using BlindMatchPAS.Core.Entities;
using BlindMatchPAS.Infrastructure.Data;
using BlindMatchPAS.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BlindMatchPAS.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Dashboard
        public async Task<IActionResult> Index()
        {
            var model = new AdminDashboardViewModel
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalProposals = await _context.ProjectProposals.CountAsync(),
                TotalMatches = await _context.MatchRecords.CountAsync(),
                TotalResearchAreas = await _context.ResearchAreas.CountAsync()
            };
            return View(model);
        }

        #region Research Areas CRUD
        public async Task<IActionResult> ResearchAreas()
        {
            var areas = await _context.ResearchAreas.ToListAsync();
            return View(areas);
        }

        [HttpPost]
        public async Task<IActionResult> CreateResearchArea(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return RedirectToAction(nameof(ResearchAreas));

            var area = new ResearchArea { Name = name.Trim() };
            _context.ResearchAreas.Add(area);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ResearchAreas));
        }

        public async Task<IActionResult> DeleteResearchArea(int id)
        {
            var area = await _context.ResearchAreas.FindAsync(id);
            if (area != null)
            {
                _context.ResearchAreas.Remove(area);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ResearchAreas));
        }
        #endregion

        #region User Management
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> PromoteToAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var adminUser = await _userManager.GetUserAsync(User);

            if (user != null && adminUser != null)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                
                // Audit Log
                await AddAuditLog(adminUser?.Email ?? "System", "Promote to Admin", $"Promoted user {user.Email} to Admin role.");
            }
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var adminUser = await _userManager.GetUserAsync(User);

            if (user != null && adminUser != null)
            {
                await _userManager.DeleteAsync(user);
                await AddAuditLog(adminUser?.Email ?? "System", "Delete User", $"Deleted user account: {user.Email}.");
            }
            return RedirectToAction(nameof(Users));
        }
        #endregion

        #region Allocation Oversight
        public async Task<IActionResult> Allocations()
        {
            var matches = await _context.MatchRecords
                .Include(m => m.Supervisor)
                .Include(m => m.ProjectProposal)
                .ThenInclude(p => p.Student)
                .ToListAsync();
            return View(matches);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveMatch(int matchId)
        {
            var match = await _context.MatchRecords
                .Include(m => m.ProjectProposal)
                .FirstOrDefaultAsync(m => m.Id == matchId);

            var adminUser = await _userManager.GetUserAsync(User);

            if (match != null && adminUser != null)
            {
                // Reset proposal status to Pending
                if (match.ProjectProposal != null)
                {
                    match.ProjectProposal.Status = "Pending";
                }

                _context.MatchRecords.Remove(match);
                await _context.SaveChangesAsync();

                await AddAuditLog(adminUser?.Email ?? "System", "Remove Match", $"Removed match between Supervisor {match.SupervisorId} and Proposal {match.ProjectProposalId}.");
            }
            return RedirectToAction(nameof(Allocations));
        }
        #endregion

        #region Audit Logs
        public async Task<IActionResult> AuditLogs()
        {
            var logs = await _context.AuditLogs.OrderByDescending(l => l.Timestamp).ToListAsync();
            return View(logs);
        }
        #endregion

        private async Task AddAuditLog(string adminEmail, string action, string details)
        {
            var log = new AuditLog
            {
                AdminEmail = adminEmail,
                Action = action,
                Details = details,
                Timestamp = DateTime.UtcNow
            };
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
