// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DbHelpers;

/// <summary>
/// Generic database creator that creates a database on construction and drops it on disposal.
/// </summary>
/// <typeparam name="T">The type of DbConnection to use (e.g., SqlConnection)</typeparam>
[ExcludeFromCodeCoverage]
public class DbCreator<T> : IDbCreator where T : DbConnection, new()
{
    private static int _instanceCounter;
    private readonly string _connectionString;
    private readonly string _dataFilePath;
    private readonly string _logFilePath;
    private bool _disposed;

    /// <summary>
    /// Gets the connection string for the created database.
    /// </summary>
    public string ConnectionString { get; }

    /// <summary>
    /// Gets the name of the created database.
    /// </summary>
    public string DatabaseName { get; }

    /// <summary>
    /// Creates a new database with a generated name using LocalDB.
    /// </summary>
    public DbCreator() : this($"TechMartTempDb_{_instanceCounter++}")
    {
    }

    /// <summary>
    /// Creates a new database with a specified name using LocalDB.
    /// </summary>
    public DbCreator(string dbName)
    {
        ValidateDbName(dbName);
        _connectionString = @"server=(localdb)\MSSQLLocalDB;Connection Timeout=120";
        DatabaseName = dbName;

        // Generate temporary file path
        var tempPath = Path.GetTempPath();
        var tempFileName = Path.Combine(tempPath, DatabaseName);
        _dataFilePath = tempFileName + ".mdf";
        _logFilePath = tempFileName + "_log.ldf";

        // Create the database
        CreateDatabase();

        // Build connection string for the created database
        ConnectionString = BuildDatabaseConnectionString();
    }

    private void CreateDatabase()
    {
        using var connection = new T();
        connection.ConnectionString = _connectionString;
        connection.Open();

        // Drop database if it exists
        var dropSql = $"""
            IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{EscapeSql(DatabaseName)}')
            BEGIN
                ALTER DATABASE [{DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [{DatabaseName}];
            END
            """;

        using (var dropCommand = connection.CreateCommand()) {
            dropCommand.CommandText = dropSql;
            dropCommand.ExecuteNonQuery();
        }

        // Create the database
        var createSql = $"""
            CREATE DATABASE [{DatabaseName}] ON PRIMARY (NAME={DatabaseName},FILENAME = '{EscapeSql(_dataFilePath)}') LOG ON (NAME={DatabaseName}_log,FILENAME='{EscapeSql(_logFilePath)}');
            """;

        using var createCommand = connection.CreateCommand();
        createCommand.CommandText = createSql;
        createCommand.ExecuteNonQuery();
    }

    private void DropDatabase()
    {
        if (_disposed)
            return;

        try {
            using var connection = new T();
            connection.ConnectionString = _connectionString;
            connection.Open();

            // Drop database
            var dropSql = $"""
                IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{EscapeSql(DatabaseName)}')
                BEGIN
                    ALTER DATABASE [{DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    DROP DATABASE [{DatabaseName}];
                END
                """;

            using var dropCommand = connection.CreateCommand();
            dropCommand.CommandText = dropSql;
            dropCommand.ExecuteNonQuery();
        } catch {
            // Suppress exceptions during disposal
        }
    }

    private string BuildDatabaseConnectionString()
    {
        // Parse the master connection string and add the database name
        var builder = new DbConnectionStringBuilder {
            ConnectionString = _connectionString
        };

        // Add or update the database/initial catalog
        if (builder.ContainsKey("Database"))
            builder["Database"] = DatabaseName;
        else if (builder.ContainsKey("Initial Catalog"))
            builder["Initial Catalog"] = DatabaseName;
        else
            builder.Add("Database", DatabaseName);

        // Set connection timeout to 120 seconds
        if (builder.ContainsKey("Connection Timeout"))
            builder["Connection Timeout"] = 120;
        else
            builder.Add("Connection Timeout", 120);

        return builder.ConnectionString;
    }

    private static string EscapeSql(string value)
    {
        return value.Replace("'", "''");
    }

    /// <summary>
    /// Disposes the database creator and drops the created database.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        DropDatabase();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Validates that the supplied name is only alphanumeric or underscore characters.
    /// </summary>
    private static void ValidateDbName(string dbName)
    {
        if (string.IsNullOrWhiteSpace(dbName))
            throw new ArgumentException("Database name cannot be null or whitespace.", nameof(dbName));

        foreach (var c in dbName) {
            if (c is not (>= '0' and <= '9' or >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_')) {
                throw new ArgumentException("Database name contains invalid characters.", nameof(dbName));
            }
        }
    }

    /// <summary>
    /// Finalizer to ensure database cleanup.
    /// </summary>
    ~DbCreator()
    {
        Dispose();
    }
}
