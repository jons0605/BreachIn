using System.ComponentModel.DataAnnotations;
using BreachIn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BreachIn.Pages;

public class OpportunitiesModel : PageModel
{
    private const string SponsorshipLikely = "Sponsorship Likely";

    private static readonly List<Opportunity> SampleOpportunities =
    [
        new()
        {
            JobTitle = "Security Analyst",
            Company = "Northstar Cyber Ltd",
            Location = "London",
            Salary = "£45,000 - £55,000",
            SponsorshipStatus = SponsorshipLikely,
            SponsorshipEvidence = "Sample evidence: employer listed as a licensed UK sponsor.",
            JobType = "Full-time"
        },
        new()
        {
            JobTitle = "Penetration Tester",
            Company = "Redbridge Security",
            Location = "Manchester",
            Salary = "£50,000 - £65,000",
            SponsorshipStatus = "Sponsorship Unclear",
            SponsorshipEvidence = "No sponsorship information is included in the sample listing.",
            JobType = "Full-time"
        },
        new()
        {
            JobTitle = "SOC Analyst",
            Company = "Bluefort Digital",
            Location = "Birmingham",
            Salary = "£35,000 - £45,000",
            SponsorshipStatus = "No Sponsorship Identified",
            SponsorshipEvidence = "The sample listing does not identify a sponsorship route.",
            JobType = "Full-time"
        },
        new()
        {
            JobTitle = "Cloud Security Engineer",
            Company = "Cloudhaven Technologies",
            Location = "Remote (UK)",
            Salary = "£65,000 - £80,000",
            SponsorshipStatus = SponsorshipLikely,
            SponsorshipEvidence = "Sample evidence: employer listed as a licensed UK sponsor.",
            JobType = "Full-time"
        },
        new()
        {
            JobTitle = "Cybersecurity Consultant",
            Company = "Westgate Assurance",
            Location = "Bristol",
            Salary = "£55,000 - £70,000",
            SponsorshipStatus = "Sponsorship Unclear",
            SponsorshipEvidence = "Sponsorship would need to be confirmed with the sample employer.",
            JobType = "Full-time"
        },
        new()
        {
            JobTitle = "Incident Response Analyst",
            Company = "Sentinel Works",
            Location = "Leeds",
            Salary = "£42,000 - £52,000",
            SponsorshipStatus = "No Sponsorship Identified",
            SponsorshipEvidence = "The sample listing does not mention visa sponsorship.",
            JobType = "Contract"
        }
    ];

    [BindProperty(SupportsGet = true)]
    [Display(Name = "Job title or keyword")]
    public string? JobTitleOrKeyword { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Location { get; set; }

    [BindProperty(SupportsGet = true)]
    [Display(Name = "UK visa sponsorship potential")]
    public bool HasVisaSponsorshipPotential { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool SearchSubmitted { get; set; }

    public IReadOnlyList<Opportunity> Opportunities { get; private set; } = [];

    public void OnGet()
    {
        if (!SearchSubmitted)
        {
            return;
        }

        IEnumerable<Opportunity> results = SampleOpportunities;

        if (!string.IsNullOrWhiteSpace(JobTitleOrKeyword))
        {
            results = results.Where(opportunity =>
                opportunity.JobTitle.Contains(JobTitleOrKeyword, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(Location))
        {
            results = results.Where(opportunity =>
                opportunity.Location.Contains(Location, StringComparison.OrdinalIgnoreCase));
        }

        if (HasVisaSponsorshipPotential)
        {
            results = results.Where(opportunity =>
                opportunity.SponsorshipStatus == SponsorshipLikely);
        }

        Opportunities = results.ToList();
    }
}
