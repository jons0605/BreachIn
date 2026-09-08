# BreachIn

BreachIn is an open-source career opportunity discovery platform focused initially on cybersecurity roles and UK visa sponsorship information.

## Project Status

BreachIn is an early-stage open-source prototype. The application currently ingests clearly labelled sample opportunities into a local SQLite catalogue and provides a simple search interface. It is not yet connected to live job sources and does not include user accounts.

## Current Features

- ASP.NET Core Razor Pages application
- BreachIn landing page
- Find Opportunities page
- Job title or keyword input
- Location input
- UK visa sponsorship potential filter
- Provider-neutral job-source and storage abstractions
- Opportunity normalization and deterministic deduplication
- Configurable scheduled ingestion with a two-hour default cadence
- Persistent SQLite opportunity storage
- Sample cybersecurity opportunity results
- Sample sponsorship indicators and evidence
- Responsive Bootstrap styling
- Automated tests for SQLite persistence and repeat ingestion

All current opportunity and sponsorship information is sample data. No external job API, live vacancy source, official sponsor-register integration, or authentication is implemented.

## Next Milestones

- First permitted live job source
- GOV.UK licensed sponsor-register ingestion and matching
- Vacancy-level sponsorship evidence analysis
- Salary and location filters
- Job details pages
- Saved opportunities
- CV upload and CV-to-job matching
- Match scores and missing requirement analysis
- Prioritized CV recommendations

## Technology Stack

- C#
- .NET 10
- ASP.NET Core Razor Pages
- Bootstrap
- Git and GitHub

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Git
- Optional: Visual Studio with the ASP.NET and web development workload

### Clone the Repository

Copy the repository URL from GitHub, then run:

```bash
git clone <repository-url>
cd BreachIn
```

### Run with the .NET CLI

From the repository root:

```bash
dotnet restore
dotnet run --project BreachIn/BreachIn.csproj
```

Open the local address shown in the terminal.

The application creates its local SQLite database at `BreachIn/App_Data/breachin.db` when the opportunity store is first used. The database and its journal files are ignored by Git.

Opportunity ingestion runs when the application starts and then every two hours by default. Configure this through the `JobIngestion` section in `appsettings.json`. Searches query the cached SQLite catalogue and do not trigger external source requests.

### Build and Test

From the repository root:

```bash
dotnet restore
dotnet build BreachIn.slnx --configuration Release --no-restore
dotnet test BreachIn.slnx --configuration Release --no-build --no-restore
```

### Run with Visual Studio

1. Open `BreachIn.slnx` in Visual Studio.
2. Select the `BreachIn` project as the startup project if necessary.
3. Run the application with **F5** or **Ctrl+F5**.

## Project Structure

```text
BreachIn/
├── BreachIn.slnx              # Solution file
├── AGENTS.md                   # Repository and architecture guidance
├── CONTRIBUTING.md             # Contribution guidance
├── LICENSE                    # MIT License
├── README.md                  # Project documentation
├── SECURITY.md                 # Vulnerability reporting guidance
├── BreachIn.Tests/             # Automated tests
└── BreachIn/
    ├── Models/                 # Opportunity domain model
    ├── Pages/                  # Razor Pages and page models
    ├── Services/               # Job sources, ingestion, and storage
    ├── wwwroot/                # CSS, JavaScript, and client libraries
    ├── BreachIn.csproj         # Application project
    └── Program.cs              # Application startup
```

## Current Architecture

```text
Registered IJobSource implementations
              ↓
Scheduled background ingestion
              ↓
      JobIngestionService
              ↓
     Normalize + deduplicate
              ↓
          IJobStore
              ↓
      Opportunities Razor Page
```

`SampleJobSource` is currently the only registered source. `SqliteJobStore` implements `IJobStore`, keeping the page and ingestion service independent of SQLite-specific details.

## Roadmap

- [x] Add provider-neutral opportunity ingestion
- [x] Add persistent SQLite opportunity storage
- [x] Add scheduled ingestion
- [ ] Connect the first permitted live job source
- [ ] Ingest and match the official UK sponsor register
- [ ] Analyze vacancy-level sponsorship evidence
- [ ] Add salary and location filters
- [ ] Add job details
- [ ] Allow users to save opportunities
- [ ] Support CV upload
- [ ] Add CV-to-job matching
- [ ] Calculate a match score
- [ ] Identify missing requirements
- [ ] Provide prioritized CV recommendations

## Contributing

Contributions, bug reports, and feature ideas are welcome. Before making a substantial change, open an issue to discuss the proposal. Keep changes focused, beginner-friendly, and supported by clear documentation or tests where appropriate.

## Security

Please do not disclose suspected security vulnerabilities publicly. Report them privately to the project maintainers through an appropriate private contact channel associated with the repository.

## License

BreachIn is available under the [MIT License](LICENSE).
