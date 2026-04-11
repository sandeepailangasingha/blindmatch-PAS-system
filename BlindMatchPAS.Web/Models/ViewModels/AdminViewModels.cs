using BlindMatchPAS.Core.Entities;
using System.Collections.Generic;

namespace BlindMatchPAS.Web.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalProposals { get; set; }
        public int TotalMatches { get; set; }
        public int TotalResearchAreas { get; set; }
    }

    public class UserManagementViewModel
    {
        public List<ApplicationUser> Users { get; set; } = new();
        public List<string> AdminEmails { get; set; } = new();
    }

    public class AllocationOversightViewModel
    {
        public List<MatchRecord> Matches { get; set; } = new();
    }

    public class ResearchAreaViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
