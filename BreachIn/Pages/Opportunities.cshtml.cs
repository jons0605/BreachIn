using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BreachIn.Pages;

public class OpportunitiesModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    [Display(Name = "Job title or keyword")]
    public string? JobTitleOrKeyword { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Location { get; set; }

    [BindProperty(SupportsGet = true)]
    [Display(Name = "UK visa sponsorship potential")]
    public bool HasVisaSponsorshipPotential { get; set; }

    public void OnGet()
    {
    }
}
