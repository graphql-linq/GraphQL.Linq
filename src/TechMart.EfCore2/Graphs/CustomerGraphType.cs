// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

namespace TechMart.EfCore2.Graphs;

public class CustomerGraphType : EfObjectGraphType<TechMartDbContext, Customer>
{
    public CustomerGraphType()
    {
        Name = "Customer";
        Description = "Represents a customer in the TechMart system.";
        EfIdField(x => x.Id)
            .Description("The unique identifier of the customer.");
        EfField(x => x.Email)
            .Description("The email address of the customer.");
        EfField(x => x.FullName)
            .Description("The full name of the customer.");
        EfField(x => x.PhoneNumber, nullable: true)
            .Description("The phone number of the customer.");
        EfField(x => x.LoyaltyPoints)
            .Description("The accumulated loyalty points of the customer.");
        EfField(x => x.CreatedAt)
            .Description("The date and time when the customer account was created.");
    }
}
