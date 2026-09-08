using BreachIn.Models;

namespace BreachIn.Services;

public interface IJobSource
{
    string SourceName { get; }

    Task<IReadOnlyCollection<Opportunity>> GetJobsAsync(
        CancellationToken cancellationToken);
}
