namespace BreachIn.Models;

public class Opportunity
{
    public string Id { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;

    public string Company { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string Salary { get; set; } = string.Empty;

    public string SponsorshipStatus { get; set; } = string.Empty;

    public string SponsorshipEvidence { get; set; } = string.Empty;

    public string JobType { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public string? SourceUrl { get; set; }

    public DateTimeOffset? PostedDate { get; set; }

    public DateTimeOffset DiscoveredDate { get; set; }

    public DateTimeOffset LastSeenDate { get; set; }
}
