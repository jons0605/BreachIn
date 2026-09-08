namespace BreachIn.Services;

public sealed class JobIngestionOptions
{
    public const string SectionName = "JobIngestion";

    public TimeSpan Interval { get; set; } = TimeSpan.FromHours(2);

    public bool RunOnStartup { get; set; } = true;
}
