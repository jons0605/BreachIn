using BreachIn.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSingleton<IJobSource, SampleJobSource>();
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
