using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using BreachIn.Models;
using Microsoft.Extensions.Options;

namespace BreachIn.Services;

public sealed class LeverJobSource : IJobSource
{
    public const string HttpClientName = "Lever";

    private const int PageSize = 100;
    private const int MaximumPagesPerSite = 20;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly LeverJobSourceOptions _options;
    private readonly ILogger<LeverJobSource> _logger;

    public LeverJobSource(
        IHttpClientFactory httpClientFactory,
        IOptions<LeverJobSourceOptions> options,
        ILogger<LeverJobSource> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public string SourceName => "Lever";

    public async Task<IReadOnlyCollection<Opportunity>> GetJobsAsync(
        CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            return [];
        }

        var opportunities = new List<Opportunity>();
        var client = _httpClientFactory.CreateClient(HttpClientName);

        foreach (var site in _options.Sites)
        {
            try
            {
                var postings = await GetPostingsAsync(client, site, cancellationToken);
                opportunities.AddRange(postings
                    .Where(IsRelevantPosting)
                    .Select(posting => MapOpportunity(posting, site)));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Lever site {LeverSite} for {CompanyName} could not be retrieved.",
                    site.SiteName,
                    site.CompanyName);
            }
        }

        return opportunities;
    }

    private static async Task<IReadOnlyCollection<LeverPosting>> GetPostingsAsync(
        HttpClient client,
        LeverSiteOptions site,
        CancellationToken cancellationToken)
    {
        var postings = new List<LeverPosting>();

        for (var page = 0; page < MaximumPagesPerSite; page++)
        {
            var skip = page * PageSize;
            var requestUri = CreateRequestUri(site, skip);
            using var response = await client.GetAsync(requestUri, cancellationToken);
            response.EnsureSuccessStatusCode();

            var pagePostings = await response.Content.ReadFromJsonAsync<List<LeverPosting>>(
                cancellationToken)
                ?? [];
            postings.AddRange(pagePostings);

            if (pagePostings.Count < PageSize)
            {
                break;
            }
        }

        return postings;
    }

    private bool IsRelevantPosting(LeverPosting posting)
    {
        var keywords = _options.Keywords
            .Where(keyword => !string.IsNullOrWhiteSpace(keyword))
            .ToList();

        if (keywords.Count == 0)
        {
            return true;
        }

        var searchableText = string.Join(
            ' ',
            posting.Text,
            posting.Categories?.Team,
            posting.Categories?.Department);

        return keywords.Any(keyword =>
            searchableText.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private Opportunity MapOpportunity(LeverPosting posting, LeverSiteOptions site) =>
        new()
        {
            JobTitle = posting.Text ?? string.Empty,
            Company = site.CompanyName,
            Location = posting.Categories?.Location ?? string.Empty,
            Salary = FormatSalary(posting),
            SponsorshipStatus = "Sponsorship Unclear",
            SponsorshipEvidence =
                "The live vacancy has not yet been analyzed for sponsorship evidence.",
            JobType = posting.Categories?.Commitment ?? string.Empty,
            Description = posting.DescriptionPlain ?? string.Empty,
            Source = SourceName,
            SourceUrl = GetHostedUrl(posting.HostedUrl)
        };

    private static string? GetHostedUrl(string? value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttps
            || (uri.Host != "jobs.lever.co" && uri.Host != "jobs.eu.lever.co"))
        {
            return null;
        }

        return uri.AbsoluteUri;
    }

    private static Uri CreateRequestUri(LeverSiteOptions site, int skip)
    {
        var host = site.UseEuropeanApi ? "api.eu.lever.co" : "api.lever.co";
        var siteName = Uri.EscapeDataString(site.SiteName);

        return new Uri(
            $"https://{host}/v0/postings/{siteName}?mode=json&limit={PageSize}&skip={skip}");
    }

    private static string FormatSalary(LeverPosting posting)
    {
        if (!string.IsNullOrWhiteSpace(posting.SalaryDescriptionPlain))
        {
            return posting.SalaryDescriptionPlain;
        }

        if (posting.SalaryRange is null)
        {
            return string.Empty;
        }

        var minimum = posting.SalaryRange.Minimum.ToString("N0", CultureInfo.InvariantCulture);
        var maximum = posting.SalaryRange.Maximum.ToString("N0", CultureInfo.InvariantCulture);
        var interval = string.IsNullOrWhiteSpace(posting.SalaryRange.Interval)
            ? string.Empty
            : $" per {posting.SalaryRange.Interval}";

        return $"{posting.SalaryRange.Currency} {minimum}–{maximum}{interval}".Trim();
    }

    private sealed class LeverPosting
    {
        [JsonPropertyName("text")]
        public string? Text { get; init; }

        [JsonPropertyName("categories")]
        public LeverCategories? Categories { get; init; }

        [JsonPropertyName("descriptionPlain")]
        public string? DescriptionPlain { get; init; }

        [JsonPropertyName("hostedUrl")]
        public string? HostedUrl { get; init; }

        [JsonPropertyName("salaryRange")]
        public LeverSalaryRange? SalaryRange { get; init; }

        [JsonPropertyName("salaryDescriptionPlain")]
        public string? SalaryDescriptionPlain { get; init; }
    }

    private sealed class LeverCategories
    {
        [JsonPropertyName("location")]
        public string? Location { get; init; }

        [JsonPropertyName("commitment")]
        public string? Commitment { get; init; }

        [JsonPropertyName("team")]
        public string? Team { get; init; }

        [JsonPropertyName("department")]
        public string? Department { get; init; }
    }

    private sealed class LeverSalaryRange
    {
        [JsonPropertyName("currency")]
        public string? Currency { get; init; }

        [JsonPropertyName("interval")]
        public string? Interval { get; init; }

        [JsonPropertyName("min")]
        public decimal Minimum { get; init; }

        [JsonPropertyName("max")]
        public decimal Maximum { get; init; }
    }
}
