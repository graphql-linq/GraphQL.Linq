// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL;
using GraphQL.Types;

namespace TechMart.Graphs;

/// <summary>
/// Attribute that validates integer parameters to ensure they fall within a specified minimum and maximum range.
/// </summary>
/// <remarks>
/// This attribute can only be applied to parameters of type <see cref="int"/>. When applied, it adds
/// validation to ensure the parameter value is between the specified minimum and maximum values (inclusive).
/// </remarks>
[AttributeUsage(AttributeTargets.Parameter)]
public class MinMaxAttribute : GraphQLAttribute
{
    private readonly int _min;
    private readonly int _max;

    /// <summary>
    /// Initializes a new instance of the <see cref="MinMaxAttribute"/> class with the specified minimum and maximum values.
    /// </summary>
    /// <param name="min">The minimum allowed value (inclusive).</param>
    /// <param name="max">The maximum allowed value (inclusive).</param>
    public MinMaxAttribute(int min, int max)
    {
        _min = min;
        _max = max;
    }

    /// <inheritdoc/>
    public override void Modify(TypeInformation typeInformation)
    {
        if (typeInformation.Type != typeof(int)) {
            throw new InvalidOperationException("MinMaxAttribute can only be applied to parameters of type int.");
        }
    }

    /// <inheritdoc/>
    public override void Modify(QueryArgument queryArgument)
    {
        queryArgument.Validate(value => {
            if (value is int intValue) {
                if (intValue < _min || intValue > _max) {
                    throw new InvalidOperationException($"Value must be between {_min} and {_max}.");
                }
            }
        });
    }
}
