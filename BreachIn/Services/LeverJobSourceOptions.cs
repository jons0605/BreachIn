namespace BreachIn.Services;

public sealed class LeverJobSourceOptions
{
    public const string SectionName = "Lever";

    public bool Enabled { get; set; }

    public List<string> Keywords { get; set; } = [];

    public List<LeverSiteOptions> Sites { get; set; } = [];
}

public sealed class LeverSiteOptions
{
    public string SiteName { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public bool UseEuropeanApi { get; set; }
}
