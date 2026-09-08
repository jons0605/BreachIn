using System.Security.Cryptography;
using System.Text;
using BreachIn.Models;

namespace BreachIn.Services;

public class JobIngestionService
{
    private readonly IEnumerable<IJobSource> _jobSources;
    private readonly IJobStore _jobStore;
    private readonly ILogger<JobIngestionService> _logger;

    public JobIngestionService(
        IEnumerable<IJobSource> jobSources,
        IJobStore jobStore,
        ILogger<JobIngestionService> logger)
    {
        _jobSources = jobSources;
        _jobStore = jobStore;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Opportunity>> IngestAsync(
        CancellationToken cancellationToken)
    {
        var normalizedJobs = new List<Opportunity>();

        foreach (var source in _jobSources)
        {
            try
            {
                var jobs = await source.GetJobsAsync(cancellationToken);
                normalizedJobs.AddRange(jobs.Select(job => Normalize(job, source.SourceName)));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Job source {SourceName} could not be ingested.",
                    source.SourceName);
            }
        }

        var uniqueJobs = normalizedJobs
            .GroupBy(CreateDeduplicationKey, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();

        await _jobStore.SaveAsync(uniqueJobs, cancellationToken);
        return await _jobStore.GetAllAsync(cancellationToken);
    }

    private static Opportunity Normalize(Opportunity opportunity, string sourceName)
    {
        var now = DateTimeOffset.UtcNow;

        opportunity.JobTitle = NormalizeText(opportunity.JobTitle);
        opportunity.Company = NormalizeText(opportunity.Company);
        opportunity.Location = NormalizeText(opportunity.Location);
        opportunity.Salary = NormalizeText(opportunity.Salary);
        opportunity.JobType = NormalizeText(opportunity.JobType);
        opportunity.Description = NormalizeText(opportunity.Description);
        opportunity.Source = NormalizeText(
            string.IsNullOrWhiteSpace(opportunity.Source) ? sourceName : opportunity.Source);
        opportunity.SourceUrl = NormalizeUrl(opportunity.SourceUrl);
        opportunity.SponsorshipStatus = NormalizeText(opportunity.SponsorshipStatus);
        opportunity.SponsorshipEvidence = NormalizeText(opportunity.SponsorshipEvidence);
        opportunity.DiscoveredDate = now;
        opportunity.LastSeenDate = now;

        var key = CreateDeduplicationKey(opportunity);
        opportunity.Id = CreateDeterministicId(key);

        return opportunity;
    }

    private static string CreateDeduplicationKey(Opportunity opportunity)
    {
        var parts = new List<string>
        {
            opportunity.Company,
            opportunity.JobTitle,
            opportunity.Location
        };

        if (!string.IsNullOrWhiteSpace(opportunity.SourceUrl))
        {
            parts.Add(opportunity.SourceUrl);
        }

        return string.Join('|', parts).ToUpperInvariant();
    }

    private static string CreateDeterministicId(string key)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string NormalizeText(string? value) =>
        string.Join(' ', (value ?? string.Empty)
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static string? NormalizeUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri)
            ? uri.AbsoluteUri.TrimEnd('/')
            : null;
    }
}
