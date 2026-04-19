using BlindMatchPAS.Core.Entities;
using BlindMatchPAS.Infrastructure.Data;
using BlindMatchPAS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BlindMatchPAS.Tests
{
    public class IntegrationTests
    {
        private ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        // Integration Test 1: DB saves MatchRecord correctly
        [Fact]
        public async Task Database_ShouldSaveMatchRecord()
        {
            // Arrange
            var context = GetInMemoryContext();

            var researchArea = new ResearchArea
            {
                Id = 1,
                Name = "Artificial Intelligence"
            };

            var proposal = new ProjectProposal
            {
                Id = 1,
                Title = "AI System",
                Abstract = "This is a test project about artificial intelligence and machine learning systems.",
                TechnicalStack = "Python, TensorFlow",
                Status = "Pending",
                StudentId = "student1",
                ResearchAreaId = 1,
                ResearchArea = researchArea
            };

            var match = new MatchRecord
            {
                ProjectProposalId = 1,
                SupervisorId = "supervisor1",
                IsConfirmed = false,
                ProjectProposal = proposal
            };

            context.ResearchAreas.Add(researchArea);
            context.ProjectProposals.Add(proposal);

            // Act
            context.MatchRecords.Add(match);
            await context.SaveChangesAsync();

            // Assert
            var saved = await context.MatchRecords
                .FirstOrDefaultAsync(m => m.SupervisorId == "supervisor1");
            Assert.NotNull(saved);
            Assert.Equal(1, saved.ProjectProposalId);
        }

        // Integration Test 2: DB updates proposal status
        [Fact]
        public async Task Database_ShouldUpdateProposalStatus()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new BlindMatchService(context);

            var researchArea = new ResearchArea
            {
                Id = 1,
                Name = "Web Development"
            };

            var proposal = new ProjectProposal
            {
                Id = 1,
                Title = "Web App",
                Abstract = "This is a comprehensive web application built using modern frameworks and best practices.",
                TechnicalStack = "ASP.NET Core",
                Status = "Pending",
                StudentId = "student1",
                ResearchAreaId = 1,
                ResearchArea = researchArea
            };

            context.ResearchAreas.Add(researchArea);
            context.ProjectProposals.Add(proposal);
            await context.SaveChangesAsync();

            // Act
            await service.ExpressInterestAsync("supervisor1", 1);
            await context.SaveChangesAsync();

            // Assert
            var updated = await context.ProjectProposals.FindAsync(1);
            Assert.Equal("Under Review", updated!.Status);
        }

        // Integration Test 3: Full match flow
        [Fact]
        public async Task Database_FullMatchFlow_ShouldWork()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new BlindMatchService(context);

            var researchArea = new ResearchArea
            {
                Id = 1,
                Name = "Cybersecurity"
            };

            var proposal = new ProjectProposal
            {
                Id = 1,
                Title = "Security App",
                Abstract = "A comprehensive cybersecurity application to monitor and protect network infrastructure.",
                TechnicalStack = "Python, Wireshark",
                Status = "Pending",
                StudentId = "student1",
                ResearchAreaId = 1,
                ResearchArea = researchArea
            };

            context.ResearchAreas.Add(researchArea);
            context.ProjectProposals.Add(proposal);
            await context.SaveChangesAsync();

            // Act - Full flow
            var match = await service.ExpressInterestAsync("supervisor1", 1);
            var confirmed = await service.ConfirmMatchAsync("supervisor1", match.Id);

            // Assert
            Assert.True(confirmed.IsConfirmed);
            Assert.NotNull(confirmed.MatchedDate);

            var finalProposal = await context.ProjectProposals.FindAsync(1);
            Assert.Equal("Matched", finalProposal!.Status);
        }

        // Integration Test 4: Research Area filter works
        [Fact]
        public async Task Database_ResearchAreaFilter_ShouldWork()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new BlindMatchService(context);

            var area1 = new ResearchArea { Id = 1, Name = "AI" };
            var area2 = new ResearchArea { Id = 2, Name = "Web" };

            context.ResearchAreas.AddRange(area1, area2);

            context.ProjectProposals.AddRange(
                new ProjectProposal
                {
                    Id = 1,
                    Title = "AI Project",
                    Abstract = "An artificial intelligence project focused on machine learning algorithms.",
                    TechnicalStack = "Python",
                    Status = "Pending",
                    StudentId = "s1",
                    ResearchAreaId = 1,
                    ResearchArea = area1
                },
                new ProjectProposal
                {
                    Id = 2,
                    Title = "Web Project",
                    Abstract = "A modern web development project using latest frameworks and technologies.",
                    TechnicalStack = "ASP.NET",
                    Status = "Pending",
                    StudentId = "s2",
                    ResearchAreaId = 2,
                    ResearchArea = area2
                }
            );
            await context.SaveChangesAsync();

            // Act
            var filtered = await service.GetBlindProposalsAsync(researchAreaId: 1);

            // Assert
            Assert.Single(filtered);
            Assert.All(filtered, p => Assert.Equal(1, p.ResearchAreaId));
        }
    }
}