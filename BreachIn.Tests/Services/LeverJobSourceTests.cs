using System.Net;
using System.Text;
using BreachIn.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace BreachIn.Tests.Services;

public sealed class LeverJobSourceTests
{
    [Fact]
    public async Task MapsRelevantPostingAndUsesHostedVacancyUrl()
    {
        Uri? requestedUri = null;
        var source = CreateSource(
            request =>
            {
                requestedUri = request.RequestUri;
                return JsonResponse(
                    """
                    [
                      {
                        "id": "posting-id",
                        "text": "Senior Security Engineer",
                        "categories": {
                          "location": "London",
                          "commitment": "Full-time",
                          "team": "Security",
                          "department": "Engineering"
                        },
                        "descriptionPlain": "Protect important systems.",
                        "hostedUrl": "https://jobs.lever.co/example/posting-id",
                        "applyUrl": "https://jobs.lever.co/example/posting-id/apply",
                        "salaryRange": {
                          "currency": "GBP",
                          "interval": "year",
                          "min": 50000,
                          "max": 70000
                        }
                      }
                    ]
                    """);
            });

        var opportunities = await source.GetJobsAsync(CancellationToken.None);

        var opportunity = Assert.Single(opportunities);
        Assert.Equal("Senior Security Engineer", opportunity.JobTitle);
        Assert.Equal("Example Ltd", opportunity.Company);
        Assert.Equal("London", opportunity.Location);
        Assert.Equal("Full-time", opportunity.JobType);
        Assert.Equal("GBP 50,000–70,000 per year", opportunity.Salary);
        Assert.Equal("Lever", opportunity.Source);
        Assert.Equal("https://jobs.lever.co/example/posting-id", opportunity.SourceUrl);
        Assert.Equal("Sponsorship Unclear", opportunity.SponsorshipStatus);
        Assert.Equal(
            "https://api.lever.co/v0/postings/example?mode=json&limit=100&skip=0",
            requestedUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ExcludesPostingsThatDoNotMatchConfiguredKeywords()
    {
        var source = CreateSource(
            _ => JsonResponse(
                """
                [
                  {
                    "text": "Finance Manager",
                    "categories": {
                      "location": "London",
                      "team": "Finance"
                    },
                    "hostedUrl": "https://jobs.lever.co/example/finance"
                  }
                ]
                """));

        var opportunities = await source.GetJobsAsync(CancellationToken.None);

        Assert.Empty(opportunities);
    }

    [Fact]
    public async Task RejectsHostedUrlOutsideLeverDomains()
    {
        var source = CreateSource(
            _ => JsonResponse(
                """
                [
                  {
                    "text": "Security Analyst",
                    "categories": { "location": "London" },
                    "hostedUrl": "https://untrusted.example/security"
                  }
                ]
                """));

        var opportunity = Assert.Single(
            await source.GetJobsAsync(CancellationToken.None));

        Assert.Null(opportunity.SourceUrl);
    }

    [Fact]
    public async Task FailureForOneSiteDoesNotDiscardAnotherSitesPostings()
    {
        var options = new LeverJobSourceOptions
        {
            Enabled = true,
            Keywords = ["security"],
            Sites =
            [
                new() { SiteName = "unavailable", CompanyName = "Unavailable Ltd" },
                new() { SiteName = "working", CompanyName = "Working Ltd" }
            ]
        };
        var source = CreateSource(
            request => request.RequestUri?.AbsolutePath.Contains("unavailable") == true
                ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                : JsonResponse(
                    """
                    [
                      {
                        "text": "Security Analyst",
                        "categories": { "location": "London" },
                        "hostedUrl": "https://jobs.lever.co/working/security"
                      }
                    ]
                    """),
            options);

        var opportunities = await source.GetJobsAsync(CancellationToken.None);

        var opportunity = Assert.Single(opportunities);
        Assert.Equal("Working Ltd", opportunity.Company);
    }

    private static LeverJobSource CreateSource(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory,
        LeverJobSourceOptions? options = null)
    {
        options ??= new LeverJobSourceOptions
        {
            Enabled = true,
            Keywords = ["security"],
            Sites = [new() { SiteName = "example", CompanyName = "Example Ltd" }]
        };
        var client = new HttpClient(new StubHttpMessageHandler(responseFactory));

        return new LeverJobSource(
            new StubHttpClientFactory(client),
            Options.Create(options),
            NullLogger<LeverJobSource>.Instance);
    }

    private static HttpResponseMessage JsonResponse(string content) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

    private sealed class StubHttpClientFactory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => client;
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(responseFactory(request));
    }
}
