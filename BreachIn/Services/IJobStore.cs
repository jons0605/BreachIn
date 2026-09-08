using BreachIn.Models;

namespace BreachIn.Services;

public interface IJobStore
{
    Task SaveAsync(
        IReadOnlyCollection<Opportunity> opportunities,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Opportunity>> GetAllAsync(
        CancellationToken cancellationToken);
}
