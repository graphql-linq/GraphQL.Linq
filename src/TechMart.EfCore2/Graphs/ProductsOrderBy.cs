// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using System.ComponentModel;

namespace TechMart.Graphs;

[Description("Specifies the ordering options for product queries.")]
public enum ProductsOrderBy
{
    [Description("Order by ID in ascending order.")]
    IdAsc,
    [Description("Order by ID in descending order.")]
    IdDesc,
    [Description("Order by name in ascending order.")]
    NameAsc,
    [Description("Order by name in descending order.")]
    NameDesc,
    [Description("Order by price in ascending order.")]
    PriceAsc,
    [Description("Order by price in descending order.")]
    PriceDesc,
}
