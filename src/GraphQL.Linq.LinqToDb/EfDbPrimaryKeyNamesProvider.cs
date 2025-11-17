// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using System.Collections.Concurrent;
using System.Linq.Expressions;
using GraphQL.Linq.GraphApi;
using LinqToDB;
using LinqToDB.Mapping;

namespace GraphQL.Linq.LinqToDb;

/// <summary>
/// Scans the requested type for public fields or properties that are marked with <see cref="PrimaryKeyAttribute"/>.
/// </summary>
public sealed class EfDbPrimaryKeyNamesProvider<TDbContext> : IEfDbPrimaryKeyNamesProvider<TDbContext>
{
    private readonly ConcurrentDictionary<Type, string[]> _concurrentDictionary = new ConcurrentDictionary<Type, string[]>();

    private readonly string? _configuration;

    /// <summary>
    /// Only matches primary keys that have a null or empty <see cref="PrimaryKeyAttribute.Configuration"/> property.
    /// </summary>
    public EfDbPrimaryKeyNamesProvider()
    {
        _configuration = null;
    }

    /// <summary>
    /// Only matches primary keys that have a null or empty <see cref="PrimaryKeyAttribute.Configuration"/> property
    /// or which matches the mapping schema configuration name provided by <paramref name="configuration"/>. 
    /// See <see cref="ProviderName"/> for standard names.
    /// </summary>
    public EfDbPrimaryKeyNamesProvider(string configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Returns a list of key names for the requested type.
    /// Pulls this list from <see cref="GetKeyNames(Type)"/> and caches it in a dictionary.
    /// </summary>
    public IEnumerable<string> GetPrimaryKeyNames<TSource>()
    {
        return _concurrentDictionary.GetOrAdd(typeof(TSource), type => GetKeyNames(type).ToArray());
    }

    /// <summary>
    /// Returns a list of key names for the requested type.
    /// </summary>
    private IEnumerable<string> GetKeyNames(Type type)
    {
        return
            from prop in type.GetMembers()
            where prop.MemberType == System.Reflection.MemberTypes.Field || prop.MemberType == System.Reflection.MemberTypes.Property
            where prop.GetCustomAttributes(typeof(PrimaryKeyAttribute), false).Any(x => x is PrimaryKeyAttribute primaryKeyAttribute &&
                (string.IsNullOrEmpty(primaryKeyAttribute.Configuration) || primaryKeyAttribute.Configuration == _configuration))
            select prop.Name;
    }

    /// <inheritdoc/>
    public Expression<Func<TSource, object>> GetDummyExpression<TSource>() => _ => Sql.AsSql(true);
}
