# BreachIn

BreachIn is an open-source career opportunity discovery platform focused initially on cybersecurity roles and UK visa sponsorship information.

## Project Status

BreachIn is an early-stage open-source prototype. The current application provides a simple opportunity search interface, but it is not yet connected to job data, a database, or user accounts.

## Current Features

- ASP.NET Core Razor Pages application
- BreachIn landing page
- Find Opportunities page
- Job title or keyword input
- Location input
- UK visa sponsorship potential filter
- Responsive Bootstrap styling

The search form is currently a user interface only. No external job API, database, or authentication is implemented.

## Planned Features

- Integration with real job opportunity data
- UK sponsorship eligibility information and supporting evidence
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

### Run with Visual Studio

1. Open `BreachIn.slnx` in Visual Studio.
2. Select the `BreachIn` project as the startup project if necessary.
3. Run the application with **F5** or **Ctrl+F5**.

## Project Structure

```text
BreachIn/
├── BreachIn.slnx              # Solution file
├── LICENSE                    # MIT License
├── README.md                  # Project documentation
└── BreachIn/
    ├── Pages/                 # Razor Pages and page models
    │   ├── Index.cshtml       # Landing page
    │   └── Opportunities.cshtml
    ├── wwwroot/               # CSS, JavaScript, and client libraries
    ├── BreachIn.csproj        # Application project
    └── Program.cs             # Application startup
```

## Roadmap

- [ ] Connect to real job opportunity data
- [ ] Add UK sponsorship eligibility information
- [ ] Show sponsorship evidence and its source
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
