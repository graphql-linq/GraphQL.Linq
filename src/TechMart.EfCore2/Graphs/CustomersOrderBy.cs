// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using System.ComponentModel;

namespace TechMart.Graphs;

[Description("Specifies the ordering options for customer queries.")]
public enum CustomersOrderBy
{
    [Description("Order by ID in ascending order.")]
    IdAsc,
    [Description("Order by ID in descending order.")]
    IdDesc,
    [Description("Order by full name in ascending order.")]
    NameAsc,
    [Description("Order by full name in descending order.")]
    NameDesc,
    [Description("Order by email in ascending order.")]
    EmailAsc,
    [Description("Order by email in descending order.")]
    EmailDesc,
    [Description("Order by creation date in ascending order.")]
    CreatedAtAsc,
    [Description("Order by creation date in descending order.")]
    CreatedAtDesc,
}
