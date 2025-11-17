// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using PublicApiGenerator;

/// <summary>
/// See more info about API approval tests here <see href="https://github.com/PublicApiGenerator/PublicApiGenerator"/>.
/// </summary>
[TestClass]
public class ApiApprovalTests
{
    [TestMethod]
    [DataRow(typeof(GraphQL.Linq.EfFieldHelpers))]
    [DataRow(typeof(GraphQL.Linq.EntityFrameworkCore2.Helpers))]
    [DataRow(typeof(GraphQL.Linq.EntityFrameworkCore3.Helpers))]
    [DataRow(typeof(GraphQL.Linq.EntityFrameworkCore6.Helpers))]
    [DataRow(typeof(GraphQL.Linq.EntityFrameworkCore8.Helpers))]
    [DataRow(typeof(GraphQL.Linq.EntityFrameworkCore10.Helpers))]
    [DataRow(typeof(GraphQL.Linq.LinqToDb.Helpers))]
    [DataRow(typeof(GraphQL.Linq.LinqToDb6.Helpers))]
    public void PublicApi(Type type)
    {
        var assembly = type.Assembly;
        var api = ApiGenerator.GeneratePublicApi(assembly, new ApiGeneratorOptions {
            IncludeAssemblyAttributes = false,
        }) + "\r\n";
        var dllName = assembly.GetName().Name!;
        api.ShouldMatchApproved(o => o.Discriminator = dllName);
    }
}
