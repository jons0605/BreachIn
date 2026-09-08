using BreachIn.Models;

namespace BreachIn.Services;

public class SampleJobSource : IJobSource
{
    public string SourceName => "Sample Data";

    public Task<IReadOnlyCollection<Opportunity>> GetJobsAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyCollection<Opportunity> opportunities =
        [
            new()
            {
                JobTitle = "Security Analyst",
                Company = "Northstar Cyber Ltd",
                Location = "London",
                Salary = "£45,000 - £55,000",
                SponsorshipStatus = "Sponsorship Likely",
                SponsorshipEvidence = "SAMPLE DATA: employer shown as a licensed UK sponsor.",
                JobType = "Full-time",
                Description = "Monitor security alerts and help investigate potential incidents.",
                Source = SourceName,
                PostedDate = new DateTimeOffset(2026, 8, 28, 0, 0, 0, TimeSpan.Zero)
            },
            new()
            {
                JobTitle = "Penetration Tester",
                Company = "Redbridge Security",
                Location = "Manchester",
                Salary = "£50,000 - £65,000",
                SponsorshipStatus = "Sponsorship Unclear",
                SponsorshipEvidence = "SAMPLE DATA: no sponsorship information is included.",
                JobType = "Full-time",
                Description = "Assess applications and infrastructure for exploitable vulnerabilities.",
                Source = SourceName,
                PostedDate = new DateTimeOffset(2026, 8, 30, 0, 0, 0, TimeSpan.Zero)
            },
            new()
            {
                JobTitle = "SOC Analyst",
                Company = "Bluefort Digital",
                Location = "Birmingham",
                Salary = "£35,000 - £45,000",
                SponsorshipStatus = "No Sponsorship Identified",
                SponsorshipEvidence = "SAMPLE DATA: the listing does not identify a sponsorship route.",
                JobType = "Full-time",
                Description = "Triage security events and escalate confirmed threats.",
                Source = SourceName,
                PostedDate = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero)
            },
            new()
            {
                JobTitle = "Cloud Security Engineer",
                Company = "Cloudhaven Technologies",
                Location = "Remote (UK)",
                Salary = "£65,000 - £80,000",
                SponsorshipStatus = "Sponsorship Likely",
                SponsorshipEvidence = "SAMPLE DATA: employer shown as a licensed UK sponsor.",
                JobType = "Full-time",
                Description = "Build and maintain security controls for cloud infrastructure.",
                Source = SourceName,
                PostedDate = new DateTimeOffset(2026, 9, 2, 0, 0, 0, TimeSpan.Zero)
            },
            new()
            {
                JobTitle = "Cybersecurity Consultant",
                Company = "Westgate Assurance",
                Location = "Bristol",
                Salary = "£55,000 - £70,000",
                SponsorshipStatus = "Sponsorship Unclear",
                SponsorshipEvidence = "SAMPLE DATA: sponsorship would need to be confirmed.",
                JobType = "Full-time",
                Description = "Advise clients on cyber risk, controls, and security improvement plans.",
                Source = SourceName,
                PostedDate = new DateTimeOffset(2026, 9, 3, 0, 0, 0, TimeSpan.Zero)
            },
            new()
            {
                JobTitle = "Incident Response Analyst",
                Company = "Sentinel Works",
                Location = "Leeds",
                Salary = "£42,000 - £52,000",
                SponsorshipStatus = "No Sponsorship Identified",
                SponsorshipEvidence = "SAMPLE DATA: the listing does not mention visa sponsorship.",
                JobType = "Contract",
                Description = "Investigate security incidents and support containment and recovery.",
                Source = SourceName,
                PostedDate = new DateTimeOffset(2026, 9, 4, 0, 0, 0, TimeSpan.Zero)
            }
        ];

        return Task.FromResult(opportunities);
    }
}
