// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

namespace GraphQL.Linq.GraphApi;

/// <summary>
/// A dictionary that can be used to pass data for a specific entity type from the EF layer to the GraphQL layer.
/// </summary>
public class EfSource<T> : Dictionary<string, object?> where T : class
{
}
