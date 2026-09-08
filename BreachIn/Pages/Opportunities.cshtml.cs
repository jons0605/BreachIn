using System.ComponentModel.DataAnnotations;
using BreachIn.Models;
using BreachIn.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BreachIn.Pages;

public class OpportunitiesModel : PageModel
{
    private const string SponsorshipLikely = "Sponsorship Likely";
    private readonly IJobStore _jobStore;

    public OpportunitiesModel(IJobStore jobStore)
    {
        _jobStore = jobStore;
    }

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

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        if (!SearchSubmitted)
        {
            return;
        }

        IEnumerable<Opportunity> results = await _jobStore.GetAllAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(JobTitleOrKeyword))
        {
            results = results.Where(opportunity =>
                opportunity.JobTitle.Contains(
                    JobTitleOrKeyword,
                    StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(Location))
        {
            results = results.Where(opportunity =>
                opportunity.Location.Contains(
                    Location,
                    StringComparison.OrdinalIgnoreCase));
        }

        if (HasVisaSponsorshipPotential)
        {
            results = results.Where(opportunity =>
                opportunity.SponsorshipStatus == SponsorshipLikely);
        }

        Opportunities = results.ToList();
    }
}
