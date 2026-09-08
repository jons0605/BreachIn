using BreachIn.Models;
using BreachIn.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace BreachIn.Tests.Services;

public sealed class JobIngestionBackgroundServiceTests
{
    [Fact]
    public async Task RunOnStartupIngestsRegisteredSources()
    {
        var source = new RecordingJobSource();
        var store = new RecordingJobStore();
        var ingestionService = new JobIngestionService(
            [source],
            store,
            NullLogger<JobIngestionService>.Instance);
        var backgroundService = new JobIngestionBackgroundService(
            ingestionService,
            Options.Create(new JobIngestionOptions
            {
                Interval = TimeSpan.FromHours(2),
                RunOnStartup = true
            }),
            NullLogger<JobIngestionBackgroundService>.Instance);

        await backgroundService.StartAsync(CancellationToken.None);

        await source.WasCalled.Task.WaitAsync(TimeSpan.FromSeconds(2));
        await backgroundService.StopAsync(CancellationToken.None);

        Assert.Single(store.Opportunities);
        Assert.NotEmpty(store.Opportunities[0].Id);
    }

    private sealed class RecordingJobSource : IJobSource
    {
        public string SourceName => "Test Source";

        public TaskCompletionSource WasCalled { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<IReadOnlyCollection<Opportunity>> GetJobsAsync(
            CancellationToken cancellationToken)
        {
            WasCalled.TrySetResult();

            IReadOnlyCollection<Opportunity> opportunities =
            [
                new()
                {
                    JobTitle = "Security Engineer",
                    Company = "Example Ltd",
                    Location = "London"
                }
            ];

            return Task.FromResult(opportunities);
        }
    }

    private sealed class RecordingJobStore : IJobStore
    {
        public IReadOnlyList<Opportunity> Opportunities { get; private set; } = [];

        public Task SaveAsync(
            IReadOnlyCollection<Opportunity> opportunities,
            CancellationToken cancellationToken)
        {
            Opportunities = opportunities.ToList();
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Opportunity>> GetAllAsync(
            CancellationToken cancellationToken) =>
            Task.FromResult(Opportunities);
    }
}
