// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

namespace TechMart.EfCore2.Graphs;

public class CategoryGraphType : EfObjectGraphType<TechMartDbContext, Category>
{
    public CategoryGraphType()
    {
        Name = "Category";
        Description = "Represents a product category in the TechMart catalog.";
        EfIdField(x => x.Id)
            .Description("The unique identifier of the category.");
        EfField(x => x.Name)
            .Description("The name of the category.");
        EfField(x => x.Description, nullable: true)
            .Description("The description of the category.");
        EfField(x => x.IsActive)
            .Description("Indicates whether the category is active and visible.");
    }
}
