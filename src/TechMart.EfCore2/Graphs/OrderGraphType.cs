// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

namespace TechMart.EfCore2.Graphs;

public class OrderGraphType : EfObjectGraphType<TechMartDbContext, Order>
{
    public OrderGraphType()
    {
        Name = "Order";
        Description = "Represents a customer order in the TechMart system.";
        EfIdField(x => x.Id)
            .Description("The unique identifier of the order.");
        EfField(x => x.OrderNumber)
            .Description("The human-readable order number.");
        EfIdField(x => x.CustomerId)
            .Description("The ID of the customer who placed the order.");
        EfField(x => x.OrderStatus)
            .Description("The current status of the order.");
        EfField(x => x.OrderDate)
            .Description("The date and time when the order was placed.");
        EfField("Total", x => x.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice))
            .Description("The total amount for the order.");
        EfField(x => x.ShippedDate, nullable: true)
            .Description("The date and time when the order was shipped.");
        EfField("OrderItems", x => x.Id)
            .DelayLoadList(context => context.DbContext.OrderItems, x => x.OrderId)
            .Description("The items included in the order.");
        EfNavigationField(x => x.Customer)
            .Description("The customer who placed the order.");
    }
}
