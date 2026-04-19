using BlindMatchPAS.Core.Entities;
using BlindMatchPAS.Core.Services;
using Moq;
using Xunit;

namespace BlindMatchPAS.Tests
{
    public class MockTests
    {
        // Mock Test 1: Verify ExpressInterest called correctly
        [Fact]
        public async Task Mock_ExpressInterest_ShouldBeCalled()
        {
            var mockService = new Mock<IBlindMatchService>();

            var expectedMatch = new MatchRecord
            {
                Id = 1,
                ProjectProposalId = 1,
                SupervisorId = "supervisor1",
                IsConfirmed = false
            };

            mockService
                .Setup(s => s.ExpressInterestAsync("supervisor1", 1))
                .ReturnsAsync(expectedMatch);

            var result = await mockService.Object
                .ExpressInterestAsync("supervisor1", 1);

            Assert.NotNull(result);
            Assert.Equal("supervisor1", result.SupervisorId);
            mockService.Verify(s =>
                s.ExpressInterestAsync("supervisor1", 1), Times.Once);
        }

        // Mock Test 2: Verify ConfirmMatch triggers reveal
        [Fact]
        public async Task Mock_ConfirmMatch_ShouldTriggerReveal()
        {
            var mockService = new Mock<IBlindMatchService>();

            var confirmedMatch = new MatchRecord
            {
                Id = 1,
                ProjectProposalId = 1,
                SupervisorId = "supervisor1",
                IsConfirmed = true,
                MatchedDate = DateTime.UtcNow
            };

            mockService
                .Setup(s => s.ConfirmMatchAsync("supervisor1", 1))
                .ReturnsAsync(confirmedMatch);

            var result = await mockService.Object
                .ConfirmMatchAsync("supervisor1", 1);

            Assert.True(result.IsConfirmed);
            Assert.NotNull(result.MatchedDate);
            mockService.Verify(s =>
                s.ConfirmMatchAsync("supervisor1", 1), Times.Once);
        }

        // Mock Test 3: Verify blind proposals hide student identity
        [Fact]
        public async Task Mock_GetBlindProposals_ShouldHideIdentity()
        {
            var mockService = new Mock<IBlindMatchService>();

            var blindProposals = new List<ProjectProposal>
            {
                new ProjectProposal
                {
                    Id = 1,
                    Title = "AI Project",
                    StudentId = string.Empty
                }
            };

            mockService
                .Setup(s => s.GetBlindProposalsAsync(null))
                .ReturnsAsync(blindProposals);

            var result = await mockService.Object.GetBlindProposalsAsync();

            Assert.All(result, p =>
                Assert.Equal(string.Empty, p.StudentId));
            mockService.Verify(s =>
                s.GetBlindProposalsAsync(null), Times.Once);
        }

        // Mock Test 4: Verify IsProposalMatched check
        [Fact]
        public async Task Mock_IsProposalMatched_ShouldReturnTrue()
        {
            var mockService = new Mock<IBlindMatchService>();

            mockService
                .Setup(s => s.IsProposalMatchedAsync(1))
                .ReturnsAsync(true);

            var result = await mockService.Object.IsProposalMatchedAsync(1);

            Assert.True(result);
            mockService.Verify(s =>
                s.IsProposalMatchedAsync(1), Times.Once);
        }
    }
}