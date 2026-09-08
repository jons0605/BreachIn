using BreachIn.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient(LeverJobSource.HttpClientName, client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "BreachIn/0.1 (+https://github.com/jons0605/BreachIn)");
});
builder.Services
    .AddOptions<LeverJobSourceOptions>()
    .Bind(builder.Configuration.GetSection(LeverJobSourceOptions.SectionName))
    .Validate(
        options => !options.Enabled || options.Sites.Count > 0,
        "At least one Lever site is required when the Lever source is enabled.")
    .Validate(
        options => options.Sites.All(site =>
            !string.IsNullOrWhiteSpace(site.SiteName)
            && !string.IsNullOrWhiteSpace(site.CompanyName)
            && site.SiteName.All(character =>
                char.IsAsciiLetterOrDigit(character) || character == '-')),
        "Lever site names may contain only letters, numbers, and hyphens, and company names are required.")
    .ValidateOnStart();
builder.Services.AddSingleton<IJobSource, LeverJobSource>();

if (builder.Configuration.GetValue("SampleData:Enabled", false))
{
    builder.Services.AddSingleton<IJobSource, SampleJobSource>();
}

builder.Services.AddSingleton<IJobStore>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var environment = serviceProvider.GetRequiredService<IWebHostEnvironment>();
    var connectionString = configuration.GetConnectionString("Jobs")
        ?? throw new InvalidOperationException(
            "The ConnectionStrings:Jobs configuration value is required.");

    return new SqliteJobStore(connectionString, environment.ContentRootPath);
});
builder.Services.AddSingleton<JobIngestionService>();
builder.Services
    .AddOptions<JobIngestionOptions>()
    .Bind(builder.Configuration.GetSection(JobIngestionOptions.SectionName))
    .Validate(
        options => options.Interval >= TimeSpan.FromHours(1),
        "The job ingestion interval must be at least one hour.")
    .ValidateOnStart();
builder.Services.AddHostedService<JobIngestionBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
