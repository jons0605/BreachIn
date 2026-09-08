# AGENTS.md — BreachIn Repository Instructions

## Project Overview

**BreachIn** is an open-source career opportunity discovery platform focused initially on cybersecurity roles and UK visa sponsorship information.

The project is currently an **early-stage prototype**. Build incrementally, keep the architecture understandable, and do not claim production readiness or functionality that is not implemented.

## Technology Stack

- C#
- ASP.NET Core Razor Pages
- .NET 10
- Bootstrap
- Git
- GitHub
- GitHub Actions CI

Do not introduce a new frontend framework, container platform, cloud dependency, or major architectural pattern unless it is clearly justified by the current milestone.

## Current Architecture

The current job-discovery architecture is provider-neutral:

```text
Registered IJobSource implementations
              ↓
      JobIngestionService
              ↓
     Normalize + deduplicate
              ↓
          IJobStore
              ↓
      Opportunities Razor Page
```

### Current components

- `IJobSource`
  - Represents a permitted job source.
  - Must support asynchronous retrieval.
  - Must support `CancellationToken`.
  - Must expose a source name.

- `JobIngestionService`
  - Retrieves jobs from registered `IJobSource` implementations.
  - Normalizes job records.
  - Deduplicates jobs.
  - Sends clean opportunities to `IJobStore`.

- `IJobStore`
  - Abstracts job persistence.
  - Current implementation is in-memory.
  - A persistent SQLite implementation is the next planned milestone.

- `SampleJobSource`
  - Currently the only registered job source.
  - Sample records must remain clearly labelled as sample data.

- `Opportunity`
  - Domain model for job opportunities.

### Deduplication

The current implementation uses a deterministic identifier derived from normalized:

- Company
- Job title
- Location
- Source URL when available

Do not replace this approach casually. Any change to deduplication must preserve deterministic, repeatable matching and avoid duplicate listings.

## Product Direction

BreachIn should evolve into a **cached opportunity catalogue**, not a live search proxy.

Target architecture:

```text
Permitted job sources
        ↓
Scheduled ingestion
        ↓
Normalize + deduplicate
        ↓
Persistent job store
        ↓
Sponsorship enrichment
        ↓
BreachIn search/filter UI
```

Users should search BreachIn's stored catalogue rather than triggering internet searches for every query.

### Planned ingestion cadence

Exact scheduling may change, but the intended model is approximately:

- Job feeds: every 1–3 hours
- Company career pages: every 3–6 hours
- Job detail revalidation: every 12–24 hours
- UK sponsor register refresh: daily
- Expired-job cleanup: daily

Do not implement aggressive polling without a clear need.

## External Job Sources

The project must remain **provider-neutral**.

Future sources may include permitted sources such as:

- Official company career pages
- Greenhouse-backed career sites
- Lever-backed career sites
- Job-board APIs with permitted programmatic access
- Other lawful and documented feeds

### Important restriction

Do **not** build the product around automated scraping of Google Search result pages.

Search engines may be used for manual source discovery, but BreachIn should not depend on prohibited or brittle search-result scraping.

Do not add LinkedIn, Indeed, Google, or any other source without checking that the intended access method is permitted.

## UK Visa Sponsorship Model

Job retrieval and sponsorship verification are **separate concerns**.

Do not infer that a vacancy offers sponsorship merely because:

- The employer is a licensed sponsor.
- The employer has sponsored roles in the past.
- The vacancy comes from a particular job board.

### Sponsorship evidence layers

BreachIn should eventually distinguish between:

1. **Vacancy explicitly offers sponsorship**
   - The job advert itself confirms sponsorship, Skilled Worker sponsorship, Certificate of Sponsorship, or equivalent.

2. **Licensed sponsor — vacancy not confirmed**
   - Employer matches the official UK sponsor register.
   - The vacancy does not explicitly confirm sponsorship.

3. **Vacancy explicitly says no sponsorship**
   - The advert states that sponsorship is unavailable or that unrestricted right to work is required.

4. **No sponsor evidence found**
   - No reliable sponsorship evidence is available.

### Authoritative employer-level source

Use the official GOV.UK / UKVI **Register of Licensed Sponsors: Workers** as the authoritative employer-level sponsorship source.

A sponsor-register match is evidence that the organisation is licensed to sponsor workers. It is **not** proof that a specific vacancy offers sponsorship.

### Vacancy-level evidence

When implemented, sponsorship analysis should capture evidence from the actual vacancy text and retain an evidence/source reference.

Positive examples may include phrases such as:

- visa sponsorship available
- Skilled Worker sponsorship
- Certificate of Sponsorship
- CoS available
- we can sponsor

Negative examples may include:

- unable to offer sponsorship
- no visa sponsorship
- must already have right to work in the UK
- unrestricted right to work required
- we cannot sponsor this role

Do not reduce sponsorship to a simple unexplained yes/no value.

## Current User-Facing Functionality

The application currently includes:

- BreachIn landing page
- Find Opportunities page
- Job title / keyword input
- Location input
- Sponsorship filter
- Sample cybersecurity opportunity results
- Sponsorship indicators for sample data
- Sponsorship evidence display for sample data

Current data is sample data unless a future live source explicitly states otherwise.

## Development Priorities

Unless the user explicitly changes direction, work in this order:

1. Provider-neutral ingestion foundation — completed
2. SQLite-backed `IJobStore`
3. Scheduled ingestion foundation
4. First permitted live job source
5. GOV.UK sponsor-register ingestion and matching
6. Vacancy-level sponsorship evidence analysis
7. Better filters and job-detail pages
8. Save opportunities
9. CV upload
10. CV-to-job matching
11. Match score
12. Missing requirements
13. Prioritized CV improvement recommendations

Do not jump ahead to AI, authentication, complex cloud infrastructure, or CV processing before the core opportunity catalogue is reliable.

## Git Workflow

`main` is protected.

Never make feature changes directly on `main`.

### Standard workflow

1. Start from an up-to-date `main`.
2. Create a focused feature branch.
3. Make the change.
4. Build and test locally.
5. Commit with a clear message.
6. Push the feature branch.
7. Create a pull request targeting `main`.
8. Ensure required CI passes.
9. Stop before merge unless explicitly instructed to merge.
10. After merge, sync local `main` and delete the completed local branch.

### Branch naming

Use descriptive branch names such as:

```text
feature/sqlite-job-store
feature/sponsor-register
feature/greenhouse-source
feature/job-expiry
fix/deduplication
chore/update-ci
docs/update-readme
```

### Commit messages

Use concise, meaningful messages:

```text
Add SQLite job store
Add sponsor register matching
Fix opportunity deduplication
Add Greenhouse job source
Update contribution guidance
```

Avoid vague commits such as:

```text
update
changes
stuff
fix
test
```

## CI Requirements

GitHub Actions CI is configured for the repository.

Before proposing a pull request:

- Restore dependencies
- Build in Release configuration
- Run tests when test projects exist
- Fix compilation errors introduced by the change

A feature should not be considered complete if the build fails.

## Security Rules

Never commit:

- API keys
- Passwords
- Access tokens
- Client secrets
- Private keys
- Credentials
- Connection strings containing credentials
- Real CV data
- Personal information
- Production secrets

Use secure configuration mechanisms such as:

- .NET User Secrets for local development
- Environment variables
- Appropriate secret stores in deployed environments

Do not add real credentials to `appsettings.json`.

Follow `SECURITY.md` for vulnerability reporting.

## Open-Source Standards

Keep the repository welcoming and understandable to external contributors.

The repository includes or should preserve:

- `README.md`
- `LICENSE`
- `SECURITY.md`
- `CONTRIBUTING.md`
- `AGENTS.md`

The project uses the MIT License.

Do not introduce proprietary dependencies or data sources that would undermine the open-source nature of the project without explicit approval.

## Coding Principles

Prefer:

- Simple, explicit C#
- Dependency injection
- Async APIs for I/O
- Small focused services
- Testable abstractions
- Clear separation of concerns
- Meaningful names
- Minimal dependencies
- Beginner-readable implementations

Avoid:

- Premature microservices
- Unnecessary design patterns
- Complex generic abstractions
- Global mutable state
- Business logic embedded directly in Razor Pages
- Hard-coded external credentials
- Silent failure handling
- Unexplained sponsorship inference

When adding a new external source, keep source-specific logic behind `IJobSource`.

When changing persistence, keep storage logic behind `IJobStore`.

When implementing sponsorship logic, keep it separate from job-source retrieval.

## Error Handling

External integrations should fail gracefully.

For source failures:

- Do not crash the application.
- Log useful diagnostic information.
- Preserve already-ingested jobs when reasonable.
- Clearly distinguish stale/cached data from fresh data where relevant.
- Avoid presenting unverified information as confirmed.

## Data Integrity

When ingesting jobs:

- Normalize company names, titles, and locations consistently.
- Preserve the original source URL.
- Track discovery time.
- Track last-seen time.
- Avoid duplicate listings.
- Do not silently overwrite higher-quality evidence with lower-quality evidence.
- Preserve provenance for sponsorship claims.

## Scope Control

Before implementing a task, inspect the existing codebase first.

Do not rewrite working architecture unless there is a concrete reason.

For each task:

1. State the intended approach.
2. Make the smallest coherent change.
3. Build the project.
4. Fix errors caused by the change.
5. Summarize files created, modified, and deleted.
6. Explain any architectural impact.
7. Do not commit, push, merge, or deploy unless the task explicitly asks for it.

## Current Next Milestone

The next planned implementation milestone is:

**SQLite persistence behind `IJobStore`**

Goal:

- Replace the in-memory store with a persistent SQLite-backed implementation.
- Preserve the existing `IJobStore` abstraction.
- Keep the Opportunities page independent of storage details.
- Persist jobs across application restarts.
- Avoid adding external job APIs as part of the same change.
- Add migrations or initialization logic in a simple, maintainable way.
- Build and test before completing the task.

After SQLite is stable, move to scheduled ingestion and then the first permitted live job source.
