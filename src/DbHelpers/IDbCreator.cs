// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

namespace DbHelpers;

/// <summary>
/// Interface for database creator that manages database lifecycle.
/// </summary>
public interface IDbCreator : IDisposable
{
    /// <summary>
    /// Gets the connection string for the created database.
    /// </summary>
    string ConnectionString { get; }

    /// <summary>
    /// Gets the name of the created database.
    /// </summary>
    string DatabaseName { get; }
}
