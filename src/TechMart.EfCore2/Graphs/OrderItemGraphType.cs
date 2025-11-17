// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

namespace TechMart.EfCore2.Graphs;

public class OrderItemGraphType : EfObjectGraphType<TechMartDbContext, OrderItem>
{
    public OrderItemGraphType()
    {
        Name = "OrderItem";
        Description = "Represents an individual item within an order.";
        EfIdField(x => x.Id)
            .Description("The unique identifier of the order item.");
        EfIdField(x => x.OrderId)
            .Description("The ID of the parent order.");
        EfIdField(x => x.ProductId)
            .Description("The ID of the product ordered.");
        EfField(x => x.Quantity)
            .Description("The quantity of the product ordered.");
        EfField(x => x.UnitPrice)
            .Description("The price per unit at the time of order (snapshot).");
        EfField("Total", x => x.Quantity * x.UnitPrice)
            .Description("The total price for this order item (Quantity * UnitPrice).");
        EfNavigationField(x => x.Product)
            .Description("The product associated with this order item.");
    }
}
