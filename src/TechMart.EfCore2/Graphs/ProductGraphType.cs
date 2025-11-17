// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

namespace TechMart.EfCore2.Graphs;

public class ProductGraphType : EfObjectGraphType<TechMartDbContext, Product>
{
    public ProductGraphType()
    {
        Name = "Product";
        Description = "Represents a product in the TechMart catalog.";
        EfIdField(x => x.Id)
            .Description("The unique identifier of the product.");
        EfField(x => x.Name)
            .Description("The name of the product.");
        EfField(x => x.Description, nullable: true)
            .Description("The description of the product.");
        EfField(x => x.Price)
            .Description("The price of the product.");
        EfField(x => x.StockQuantity)
            .Description("The available stock quantity of the product.");
        EfField("Categories", x => x.Id)
            .DelayLoadList(context => context.DbContext.ProductCategories, x => x.ProductId, x => x.Category)
            .Description("The category to which the product belongs.");
    }
}
