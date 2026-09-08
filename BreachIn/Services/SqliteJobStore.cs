using System.Globalization;
using BreachIn.Models;
using Microsoft.Data.Sqlite;

namespace BreachIn.Services;

public sealed class SqliteJobStore : IJobStore
{
    private const int CurrentSchemaVersion = 1;
    private readonly string _connectionString;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private bool _isInitialized;

    public SqliteJobStore(string connectionString, string contentRootPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentRootPath);

        var connectionStringBuilder = new SqliteConnectionStringBuilder(connectionString);

        if (!string.Equals(connectionStringBuilder.DataSource, ":memory:", StringComparison.OrdinalIgnoreCase)
            && !Path.IsPathRooted(connectionStringBuilder.DataSource))
        {
            connectionStringBuilder.DataSource = Path.GetFullPath(
                Path.Combine(contentRootPath, connectionStringBuilder.DataSource));
        }

        var databaseDirectory = Path.GetDirectoryName(connectionStringBuilder.DataSource);
        if (!string.IsNullOrWhiteSpace(databaseDirectory))
        {
            Directory.CreateDirectory(databaseDirectory);
        }

        _connectionString = connectionStringBuilder.ToString();
    }

    public async Task SaveAsync(
        IReadOnlyCollection<Opportunity> opportunities,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(opportunities);
        await EnsureInitializedAsync(cancellationToken);

        if (opportunities.Count == 0)
        {
            return;
        }

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction =
            (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);

        foreach (var opportunity in opportunities)
        {
            await SaveOpportunityAsync(connection, transaction, opportunity, cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Opportunity>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT Id,
                   JobTitle,
                   Company,
                   Location,
                   Salary,
                   SponsorshipStatus,
                   SponsorshipEvidence,
                   JobType,
                   Description,
                   Source,
                   SourceUrl,
                   PostedDate,
                   DiscoveredDate,
                   LastSeenDate
            FROM Opportunities
            ORDER BY DiscoveredDate DESC, Id;
            """;

        var opportunities = new List<Opportunity>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            opportunities.Add(ReadOpportunity(reader));
        }

        return opportunities;
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_isInitialized)
        {
            return;
        }

        await _initializationLock.WaitAsync(cancellationToken);

        try
        {
            if (_isInitialized)
            {
                return;
            }

            await using var connection = await OpenConnectionAsync(cancellationToken);
            await using var transaction =
                (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText =
                $"""
                CREATE TABLE IF NOT EXISTS Opportunities (
                    Id TEXT NOT NULL PRIMARY KEY,
                    JobTitle TEXT NOT NULL,
                    Company TEXT NOT NULL,
                    Location TEXT NOT NULL,
                    Salary TEXT NOT NULL,
                    SponsorshipStatus TEXT NOT NULL,
                    SponsorshipEvidence TEXT NOT NULL,
                    JobType TEXT NOT NULL,
                    Description TEXT NOT NULL,
                    Source TEXT NOT NULL,
                    SourceUrl TEXT NULL,
                    PostedDate TEXT NULL,
                    DiscoveredDate TEXT NOT NULL,
                    LastSeenDate TEXT NOT NULL
                );

                PRAGMA user_version = {CurrentSchemaVersion};
                """;

            await command.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            _isInitialized = true;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    private async Task<SqliteConnection> OpenConnectionAsync(
        CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(_connectionString);

        try
        {
            await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA busy_timeout = 5000;";
            await command.ExecuteNonQueryAsync(cancellationToken);
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    private static async Task SaveOpportunityAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        Opportunity opportunity,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(opportunity.Id);

        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            """
            INSERT INTO Opportunities (
                Id, JobTitle, Company, Location, Salary, SponsorshipStatus,
                SponsorshipEvidence, JobType, Description, Source, SourceUrl,
                PostedDate, DiscoveredDate, LastSeenDate)
            VALUES (
                $id, $jobTitle, $company, $location, $salary, $sponsorshipStatus,
                $sponsorshipEvidence, $jobType, $description, $source, $sourceUrl,
                $postedDate, $discoveredDate, $lastSeenDate)
            ON CONFLICT(Id) DO UPDATE SET
                JobTitle = excluded.JobTitle,
                Company = excluded.Company,
                Location = excluded.Location,
                Salary = excluded.Salary,
                SponsorshipStatus = excluded.SponsorshipStatus,
                SponsorshipEvidence = excluded.SponsorshipEvidence,
                JobType = excluded.JobType,
                Description = excluded.Description,
                Source = excluded.Source,
                SourceUrl = excluded.SourceUrl,
                PostedDate = excluded.PostedDate,
                LastSeenDate = excluded.LastSeenDate;
            """;

        command.Parameters.AddWithValue("$id", opportunity.Id);
        command.Parameters.AddWithValue("$jobTitle", opportunity.JobTitle);
        command.Parameters.AddWithValue("$company", opportunity.Company);
        command.Parameters.AddWithValue("$location", opportunity.Location);
        command.Parameters.AddWithValue("$salary", opportunity.Salary);
        command.Parameters.AddWithValue("$sponsorshipStatus", opportunity.SponsorshipStatus);
        command.Parameters.AddWithValue("$sponsorshipEvidence", opportunity.SponsorshipEvidence);
        command.Parameters.AddWithValue("$jobType", opportunity.JobType);
        command.Parameters.AddWithValue("$description", opportunity.Description);
        command.Parameters.AddWithValue("$source", opportunity.Source);
        command.Parameters.AddWithValue(
            "$sourceUrl",
            (object?)opportunity.SourceUrl ?? DBNull.Value);
        command.Parameters.AddWithValue(
            "$postedDate",
            opportunity.PostedDate is { } postedDate
                ? postedDate.ToString("O")
                : DBNull.Value);
        command.Parameters.AddWithValue("$discoveredDate", opportunity.DiscoveredDate.ToString("O"));
        command.Parameters.AddWithValue("$lastSeenDate", opportunity.LastSeenDate.ToString("O"));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static Opportunity ReadOpportunity(SqliteDataReader reader) =>
        new()
        {
            Id = reader.GetString(0),
            JobTitle = reader.GetString(1),
            Company = reader.GetString(2),
            Location = reader.GetString(3),
            Salary = reader.GetString(4),
            SponsorshipStatus = reader.GetString(5),
            SponsorshipEvidence = reader.GetString(6),
            JobType = reader.GetString(7),
            Description = reader.GetString(8),
            Source = reader.GetString(9),
            SourceUrl = reader.IsDBNull(10) ? null : reader.GetString(10),
            PostedDate = reader.IsDBNull(11) ? null : ParseDate(reader.GetString(11)),
            DiscoveredDate = ParseDate(reader.GetString(12)),
            LastSeenDate = ParseDate(reader.GetString(13))
        };

    private static DateTimeOffset ParseDate(string value) =>
        DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
}
