using BlindMatchPAS.Core.Entities;
using BlindMatchPAS.Infrastructure.Data;
using BlindMatchPAS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BlindMatchPAS.Tests
{
    public class BlindMatchServiceTests
    {
        private ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        // Unit Test 1: Express Interest creates MatchRecord
        [Fact]
        public async Task ExpressInterest_ShouldCreateMatchRecord()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new BlindMatchService(context);

            var student = new ApplicationUser
            {
                Id = "student1",
                FullName = "Kamal Perera",
                Email = "kamal@nsbm.ac.lk",
                UserName = "kamal@nsbm.ac.lk"
            };

            var researchArea = new ResearchArea
            {
                Id = 1,
                Name = "Artificial Intelligence"
            };

            var proposal = new ProjectProposal
            {
                Id = 1,
                Title = "AI Traffic System",
                Abstract = "This project develops an AI system for managing traffic in urban areas using machine learning.",
                TechnicalStack = "Python, TensorFlow",
                Status = "Pending",
                StudentId = "student1",
                ResearchAreaId = 1,
                ResearchArea = researchArea,
                Student = student
            };

            context.Users.Add(student);
            context.ResearchAreas.Add(researchArea);
            context.ProjectProposals.Add(proposal);
            await context.SaveChangesAsync();

            // Act
            var match = await service.ExpressInterestAsync("supervisor1", 1);

            // Assert
            Assert.NotNull(match);
            Assert.Equal("supervisor1", match.SupervisorId);
            Assert.Equal(1, match.ProjectProposalId);
            Assert.False(match.IsConfirmed);
        }

        // Unit Test 2: Status changes to Under Review
        [Fact]
        public async Task ExpressInterest_ShouldUpdateStatusToUnderReview()
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
                Title = "E-Commerce Platform",
                Abstract = "Building a full-stack e-commerce platform using modern web technologies and best practices.",
                TechnicalStack = "ASP.NET Core, React",
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

            // Assert
            var updatedProposal = await context.ProjectProposals.FindAsync(1);
            Assert.Equal("Under Review", updatedProposal!.Status);
        }

        // Unit Test 3: Confirm Match triggers Identity Reveal
        [Fact]
        public async Task ConfirmMatch_ShouldTriggerIdentityReveal()
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
                Title = "Network Security System",
                Abstract = "Developing a comprehensive network security monitoring system using modern cybersecurity tools.",
                TechnicalStack = "Python, Wireshark",
                Status = "Under Review",
                StudentId = "student1",
                ResearchAreaId = 1,
                ResearchArea = researchArea
            };

            var match = new MatchRecord
            {
                Id = 1,
                ProjectProposalId = 1,
                SupervisorId = "supervisor1",
                IsConfirmed = false,
                ProjectProposal = proposal
            };

            context.ResearchAreas.Add(researchArea);
            context.ProjectProposals.Add(proposal);
            context.MatchRecords.Add(match);
            await context.SaveChangesAsync();

            // Act
            var confirmedMatch = await service.ConfirmMatchAsync("supervisor1", 1);

            // Assert
            Assert.True(confirmedMatch.IsConfirmed);
            Assert.NotNull(confirmedMatch.MatchedDate);
        }

        // Unit Test 4: Proposal status changes to Matched
        [Fact]
        public async Task ConfirmMatch_ShouldUpdateProposalStatusToMatched()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new BlindMatchService(context);

            var researchArea = new ResearchArea
            {
                Id = 1,
                Name = "Cloud Computing"
            };

            var proposal = new ProjectProposal
            {
                Id = 1,
                Title = "Cloud Migration Tool",
                Abstract = "Creating an automated tool for migrating legacy systems to cloud infrastructure efficiently.",
                TechnicalStack = "Azure, .NET Core",
                Status = "Under Review",
                StudentId = "student1",
                ResearchAreaId = 1,
                ResearchArea = researchArea
            };

            var match = new MatchRecord
            {
                Id = 1,
                ProjectProposalId = 1,
                SupervisorId = "supervisor1",
                IsConfirmed = false,
                ProjectProposal = proposal
            };

            context.ResearchAreas.Add(researchArea);
            context.ProjectProposals.Add(proposal);
            context.MatchRecords.Add(match);
            await context.SaveChangesAsync();

            // Act
            await service.ConfirmMatchAsync("supervisor1", 1);

            // Assert
            var updatedProposal = await context.ProjectProposals.FindAsync(1);
            Assert.Equal("Matched", updatedProposal!.Status);
        }

        // Unit Test 5: Blind Proposals hide student identity
        [Fact]
        public async Task GetBlindProposals_ShouldHideStudentIdentity()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new BlindMatchService(context);

            var researchArea = new ResearchArea
            {
                Id = 1,
                Name = "Machine Learning"
            };

            var proposal = new ProjectProposal
            {
                Id = 1,
                Title = "ML Recommendation System",
                Abstract = "Building a machine learning based recommendation system for e-commerce platforms.",
                TechnicalStack = "Python, Scikit-learn",
                Status = "Pending",
                StudentId = "student-secret-id",
                ResearchAreaId = 1,
                ResearchArea = researchArea
            };

            context.ResearchAreas.Add(researchArea);
            context.ProjectProposals.Add(proposal);
            await context.SaveChangesAsync();

            // Act
            var blindProposals = await service.GetBlindProposalsAsync();

            // Assert - Student identity hidden!
            Assert.All(blindProposals, p =>
                Assert.Equal(string.Empty, p.StudentId));
        }

        // Unit Test 6: Duplicate interest prevention
        [Fact]
        public async Task ExpressInterest_ShouldNotCreateDuplicateMatchRecord()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new BlindMatchService(context);

            var researchArea = new ResearchArea
            {
                Id = 1,
                Name = "IoT"
            };

            var proposal = new ProjectProposal
            {
                Id = 1,
                Title = "Smart Home System",
                Abstract = "Developing an IoT-based smart home automation system with mobile app control.",
                TechnicalStack = "Arduino, React Native",
                Status = "Pending",
                StudentId = "student1",
                ResearchAreaId = 1,
                ResearchArea = researchArea
            };

            context.ResearchAreas.Add(researchArea);
            context.ProjectProposals.Add(proposal);
            await context.SaveChangesAsync();

            // Act - Express interest twice
            await service.ExpressInterestAsync("supervisor1", 1);
            await service.ExpressInterestAsync("supervisor1", 1);

            // Assert - Only one record created
            var matches = context.MatchRecords.Where(m =>
                m.SupervisorId == "supervisor1" &&
                m.ProjectProposalId == 1);
            Assert.Single(matches);
        }
    }
}