using Microsoft.Extensions.Options;

namespace BreachIn.Services;

public sealed class JobIngestionBackgroundService : BackgroundService
{
    private readonly JobIngestionService _jobIngestionService;
    private readonly JobIngestionOptions _options;
    private readonly ILogger<JobIngestionBackgroundService> _logger;

    public JobIngestionBackgroundService(
        JobIngestionService jobIngestionService,
        IOptions<JobIngestionOptions> options,
        ILogger<JobIngestionBackgroundService> logger)
    {
        _jobIngestionService = jobIngestionService;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_options.RunOnStartup)
        {
            await IngestSafelyAsync(stoppingToken);
        }

        using var timer = new PeriodicTimer(_options.Interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await IngestSafelyAsync(stoppingToken);
        }
    }

    private async Task IngestSafelyAsync(CancellationToken cancellationToken)
    {
        try
        {
            var opportunities = await _jobIngestionService.IngestAsync(cancellationToken);
            _logger.LogInformation(
                "Scheduled job ingestion completed. The catalogue contains {OpportunityCount} opportunities.",
                opportunities.Count);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Scheduled job ingestion failed.");
        }
    }
}
