using BlindMatchPAS.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ProjectProposal> ProjectProposals { get; set; }
        public DbSet<MatchRecord> MatchRecords { get; set; }
        public DbSet<ResearchArea> ResearchAreas { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<SupervisorResearchArea> SupervisorResearchAreas { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ProjectProposal>()
                .HasOne(p => p.Student)
                .WithMany()
                .HasForeignKey(p => p.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProjectProposal>()
                .HasOne(p => p.ResearchArea)
                .WithMany(r => r.ProjectProposals)
                .HasForeignKey(p => p.ResearchAreaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MatchRecord>()
                .HasOne(m => m.Supervisor)
                .WithMany()
                .HasForeignKey(m => m.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SupervisorResearchArea>()
                .HasOne(s => s.Supervisor)
                .WithMany()
                .HasForeignKey(s => s.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SupervisorResearchArea>()
                .HasOne(s => s.ResearchArea)
                .WithMany()
                .HasForeignKey(s => s.ResearchAreaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}