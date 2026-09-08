using BreachIn.Models;

namespace BreachIn.Services;

public class InMemoryJobStore : IJobStore
{
    private readonly Dictionary<string, Opportunity> _opportunities =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly Lock _lock = new();

    public Task SaveAsync(
        IReadOnlyCollection<Opportunity> opportunities,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            foreach (var opportunity in opportunities)
            {
                if (_opportunities.TryGetValue(opportunity.Id, out var existing))
                {
                    opportunity.DiscoveredDate = existing.DiscoveredDate;
                }

                _opportunities[opportunity.Id] = opportunity;
            }
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Opportunity>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            IReadOnlyList<Opportunity> opportunities = _opportunities.Values.ToList();
            return Task.FromResult(opportunities);
        }
    }
}
