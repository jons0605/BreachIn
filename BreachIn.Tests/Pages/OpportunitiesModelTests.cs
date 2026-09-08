using BreachIn.Models;
using BreachIn.Pages;
using BreachIn.Services;
using Xunit;

namespace BreachIn.Tests.Pages;

public sealed class OpportunitiesModelTests
{
    [Fact]
    public async Task SubmittedSearchReadsAndFiltersCachedOpportunities()
    {
        var store = new RecordingJobStore(
        [
            new()
            {
                Id = "security-engineer",
                JobTitle = "Security Engineer",
                Company = "Example Ltd",
                Location = "London"
            },
            new()
            {
                Id = "software-engineer",
                JobTitle = "Software Engineer",
                Company = "Example Ltd",
                Location = "Manchester"
            }
        ]);
        var pageModel = new OpportunitiesModel(store)
        {
            SearchSubmitted = true,
            JobTitleOrKeyword = "Security",
            Location = "London"
        };

        await pageModel.OnGetAsync(CancellationToken.None);

        var opportunity = Assert.Single(pageModel.Opportunities);
        Assert.Equal("security-engineer", opportunity.Id);
        Assert.Equal(1, store.ReadCount);
    }

    private sealed class RecordingJobStore(IReadOnlyList<Opportunity> opportunities)
        : IJobStore
    {
        public int ReadCount { get; private set; }

        public Task SaveAsync(
            IReadOnlyCollection<Opportunity> opportunitiesToSave,
            CancellationToken cancellationToken) =>
            throw new InvalidOperationException("A catalogue search must not ingest jobs.");

        public Task<IReadOnlyList<Opportunity>> GetAllAsync(
            CancellationToken cancellationToken)
        {
            ReadCount++;
            return Task.FromResult(opportunities);
        }
    }
}
