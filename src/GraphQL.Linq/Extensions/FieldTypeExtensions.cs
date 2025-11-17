// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Linq.GraphApi;
using GraphQL.Types;

namespace GraphQL.Linq;

internal static class FieldTypeExtensions
{
    /// <summary>
    /// Sets the EF metadata for the specified field type.
    /// </summary>
    internal static void SetEfMetadata(this FieldType fieldType, EfMetadata? data)
    {
        if (fieldType == null)
            throw new ArgumentNullException(nameof(fieldType));

        if (data == null)
            fieldType.Metadata.Remove("_EF_Metadata");
        else
            fieldType.Metadata["_EF_Metadata"] = data;
    }

    /// <summary>
    /// Gets the EF metadata for the specified field type.
    /// </summary>
    internal static EfMetadata? GetEfMetadata(this FieldType fieldType)
    {
        if (fieldType == null)
            throw new ArgumentNullException(nameof(fieldType));

        return fieldType.GetMetadata<EfMetadata>("_EF_Metadata");
    }
}
