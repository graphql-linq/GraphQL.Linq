// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using System.Linq.Expressions;
using GraphQL.Linq.GraphApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.Linq.EntityFrameworkCore2;

/// <inheritdoc cref="IEfDbPrimaryKeyNamesProvider{TDbContext}"/>
public sealed class EfDbPrimaryKeyNamesProvider<TDbContext> : IEfDbPrimaryKeyNamesProvider<TDbContext>
    where TDbContext : DbContext
{
    /// <summary>
    /// A dictionary of the primary key names for each entity type
    /// </summary>
    private readonly Dictionary<Type, List<string>> _keyNames;

    /// <summary>
    /// Creates a new instance of the <see cref="EfDbPrimaryKeyNamesProvider{TDbContext}"/> class by creating a <typeparamref name="TDbContext"/> from a dedicated service scope.
    /// </summary>
    public EfDbPrimaryKeyNamesProvider(IServiceScopeFactory serviceScopeFactory)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        _keyNames = LoadKeyNames(dbContext.Model);
    }

    /// <summary>
    /// Loads the primary key names from the specified <see cref="IModel"/>.
    /// </summary>
    private static Dictionary<Type, List<string>> LoadKeyNames(IModel model)
    {
        var keyNames = new Dictionary<Type, List<string>>();
        if (model == null)
            throw new ArgumentNullException(nameof(model));
        foreach (var entityType in model.GetEntityTypes()) {
            var primaryKey = entityType.FindPrimaryKey();
            //This can happen for views
            if (primaryKey == null) {
                continue;
            }

            var names = primaryKey.Properties.Select(x => x.Name).ToList();
            keyNames.Add(entityType.ClrType, names);
        }
        return keyNames;
    }

    /// <inheritdoc/>
    public IEnumerable<string> GetPrimaryKeyNames<TSource>()
    {
        if (_keyNames.TryGetValue(typeof(TSource), out var names))
            return names;

        return Enumerable.Empty<string>();
    }

    /// <inheritdoc/>
    public Expression<Func<TSource, object>> GetDummyExpression<TSource>() => _ => true; // not effective
}
