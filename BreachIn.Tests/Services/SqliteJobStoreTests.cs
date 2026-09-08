using BreachIn.Models;
using BreachIn.Services;
using Microsoft.Data.Sqlite;
using Xunit;

namespace BreachIn.Tests.Services;

public sealed class SqliteJobStoreTests : IDisposable
{
    private readonly string _temporaryDirectory = Path.Combine(
        Path.GetTempPath(),
        "BreachIn.Tests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task SavedOpportunityCanBeReadByANewStoreInstance()
    {
        var expected = CreateOpportunity();
        var firstStore = CreateStore();
        await firstStore.SaveAsync([expected], CancellationToken.None);

        var secondStore = CreateStore();
        var results = await secondStore.GetAllAsync(CancellationToken.None);

        var actual = Assert.Single(results);
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.JobTitle, actual.JobTitle);
        Assert.Equal(expected.Company, actual.Company);
        Assert.Equal(expected.SourceUrl, actual.SourceUrl);
        Assert.Equal(expected.PostedDate, actual.PostedDate);
        Assert.Equal(expected.DiscoveredDate, actual.DiscoveredDate);
        Assert.Equal(expected.LastSeenDate, actual.LastSeenDate);
    }

    [Fact]
    public async Task SavingAnExistingOpportunityPreservesDiscoveryAndUpdatesLastSeen()
    {
        var original = CreateOpportunity();
        var store = CreateStore();
        await store.SaveAsync([original], CancellationToken.None);

        var updated = CreateOpportunity();
        updated.DiscoveredDate = original.DiscoveredDate.AddDays(1);
        updated.LastSeenDate = original.LastSeenDate.AddHours(2);
        updated.Salary = "£55,000 - £65,000";
        await store.SaveAsync([updated], CancellationToken.None);

        var actual = Assert.Single(
            await store.GetAllAsync(CancellationToken.None));
        Assert.Equal(original.DiscoveredDate, actual.DiscoveredDate);
        Assert.Equal(updated.LastSeenDate, actual.LastSeenDate);
        Assert.Equal(updated.Salary, actual.Salary);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();

        if (Directory.Exists(_temporaryDirectory))
        {
            Directory.Delete(_temporaryDirectory, recursive: true);
        }
    }

    private SqliteJobStore CreateStore() =>
        new("Data Source=jobs.db", _temporaryDirectory);

    private static Opportunity CreateOpportunity() =>
        new()
        {
            Id = "deterministic-id",
            JobTitle = "Security Engineer",
            Company = "Example Ltd",
            Location = "London",
            Salary = "£50,000 - £60,000",
            SponsorshipStatus = "Sponsorship Unclear",
            SponsorshipEvidence = "SAMPLE DATA: sponsorship must be confirmed.",
            JobType = "Full-time",
            Description = "Protect systems and investigate security events.",
            Source = "Sample Data",
            SourceUrl = "https://example.test/jobs/security-engineer",
            PostedDate = new DateTimeOffset(2026, 9, 1, 9, 30, 0, TimeSpan.Zero),
            DiscoveredDate = new DateTimeOffset(2026, 9, 2, 10, 0, 0, TimeSpan.Zero),
            LastSeenDate = new DateTimeOffset(2026, 9, 2, 10, 0, 0, TimeSpan.Zero)
        };
}
