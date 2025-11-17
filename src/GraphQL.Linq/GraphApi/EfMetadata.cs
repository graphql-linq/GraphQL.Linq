// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

namespace GraphQL.Linq.GraphApi;

/// <summary>
/// Represents metadata information for Ef fields, including navigation, query, and expression data.
/// This metadata is used to identify and configure how fields relate to navigation, queries, and the relational structure.
/// </summary>
internal sealed class EfMetadata
{
    /// <summary>
    /// Indicates that this field is a navigation field (EfNavigationField)
    /// </summary>
    public bool Graph { get; set; }

    /// <summary>
    /// Indicates that this field is a navigation list field (EfNavigationListField)
    /// </summary>
    public bool Query { get; set; }

    /// <summary>
    /// Indicates that this field returns a single entry from a navigation list field (EfNavigationLinkField)
    /// </summary>
    public bool Single { get; set; }

    /// <summary>
    /// Sets the type of the field, entity type of a navigation field, or entity type of a navigation list field
    /// </summary>
    public Type Type { get; set; } = null!;

    /// <summary>
    /// Points to an instance of IEfConnectionResolver
    /// </summary>
    public object? ConnectionResolver { get; set; }

    /// <summary>
    /// Stores the expression of the Ef field
    /// </summary>
    public Delegate Expression { get; set; } = null!;
}
