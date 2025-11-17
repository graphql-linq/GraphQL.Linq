// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

namespace TechMart.EfCore2.Graphs;

public class UserGraphType : EfObjectGraphType<TechMartDbContext, User>
{
    public UserGraphType()
    {
        Name = "User";
        Description = "Represents a user (employee/staff) in the TechMart system.";
        EfIdField(x => x.Id)
            .Description("The unique identifier of the user.");
        EfField(x => x.Username)
            .Description("The username for login.");
        EfField(x => x.Email)
            .Description("The email address of the user.");
        EfField(x => x.Role)
            .Description("The role of the user (Admin, Manager, Sales, Support).");
        EfField(x => x.IsActive)
            .Description("Indicates whether the user account is active.");
    }
}
