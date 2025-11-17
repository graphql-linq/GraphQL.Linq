// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Builders;
using GraphQL.DataLoader;
using GraphQL.Resolvers;
using GraphQL.Types;

namespace GraphQL.Linq;

/// <summary>
/// Provides extension methods for FieldBuilders.
/// </summary>
public static partial class FieldBuilderExtensions
{
    /// <summary>
    /// Executes additional code after the existing resolver completes.
    /// This is typically useful for post-processing the result of the existing resolver.
    /// </summary>
    public static FieldBuilder<TSource, TReturn> ThenResolve<TSource, TObject, TReturn>(
        this FieldBuilder<TSource, TObject> fieldBuilder,
        Func<TObject, TReturn> resolver,
        bool? nullable = null,
        Type? graphType = null)
        => ThenResolve(fieldBuilder, (context, source) => resolver(source), nullable, graphType);

    /// <inheritdoc cref="ThenResolve{TSource, TObject, TReturn}(FieldBuilder{TSource, TObject}, Func{TObject, TReturn}, bool?, Type?)"/>
    public static FieldBuilder<TSource, TReturn> ThenResolve<TSource, TObject, TReturn>(
        this FieldBuilder<TSource, TObject> fieldBuilder,
        Func<IResolveFieldContext<TSource>, TObject, TReturn> resolver,
        bool? nullable = null,
        Type? graphType = null)
        => ThenResolve(fieldBuilder, (context, source) => Task.FromResult(resolver(context, source)), nullable, graphType);

    /// <inheritdoc cref="ThenResolve{TSource, TObject, TReturn}(FieldBuilder{TSource, TObject}, Func{IResolveFieldContext{TSource}, TObject, TReturn}, bool?, Type?)"/>
    public static FieldBuilder<TSource, TReturn> ThenResolve<TSource, TObject, TReturn>(
        this FieldBuilder<TSource, TObject> fieldBuilder,
        Func<IResolveFieldContext<TSource>, TObject, Task<TReturn>> resolver,
        bool? nullable = null,
        Type? graphType = null)
    {
        nullable ??= !typeof(NonNullGraphType).IsAssignableFrom(fieldBuilder.FieldType.Type);
        graphType ??= typeof(TReturn).GetGraphTypeFromType(nullable.Value, TypeMappingMode.OutputType);

        fieldBuilder.FieldType.Type = graphType;

        var oldResolver = fieldBuilder.FieldType.Resolver!;
        fieldBuilder.FieldType.Resolver = new FuncFieldResolver<TSource, object?>(async (context) => {
            var source = await oldResolver.ResolveAsync(context);
            return await Return(context, source, resolver).ConfigureAwait(false);
        });
        return fieldBuilder.Returns<TReturn>();

        static async Task<object?> Return(IResolveFieldContext<TSource> context, object? source, Func<IResolveFieldContext<TSource>, TObject, Task<TReturn>> func)
        {
            if (source is IDataLoaderResult dataLoaderResult) {
                return new SimpleDataLoader<object?>(async (cancellationToken) => {
                    var result = await dataLoaderResult.GetResultAsync(cancellationToken).ConfigureAwait(false);
                    return await Return(context, result, func).ConfigureAwait(false);
                });
            }
            return await func(context, (TObject)source!).ConfigureAwait(false);
        }
    }

    /// <inheritdoc cref="ThenResolve{TSource, TObject, TReturn}(FieldBuilder{TSource, TObject}, Func{TObject, TReturn}, bool?, Type?)"/>
    public static EfFieldBuilder<TDbContext, TSource, TReturn> ThenResolve<TDbContext, TSource, TObject, TReturn>(
        EfFieldBuilder<TDbContext, TSource, TObject> fieldBuilder,
        Func<TObject, TReturn> resolver,
        bool? nullable = null,
        Type? graphType = null)
        => ThenResolve(fieldBuilder, (context, source) => resolver(source), nullable, graphType);

    /// <inheritdoc cref="ThenResolve{TDbContext, TSource, TObject, TReturn}(EfFieldBuilder{TDbContext, TSource, TObject}, Func{TObject, TReturn}, bool?, Type?)"/>
    public static EfFieldBuilder<TDbContext, TSource, TReturn> ThenResolve<TDbContext, TSource, TObject, TReturn>(
        EfFieldBuilder<TDbContext, TSource, TObject> fieldBuilder,
        Func<IResolveFieldContext<TSource>, TObject, TReturn> resolver,
        bool? nullable = null,
        Type? graphType = null)
        => ThenResolve(fieldBuilder, (context, source) => Task.FromResult(resolver(context, source)), nullable, graphType);

    /// <inheritdoc cref="ThenResolve{TDbContext, TSource, TObject, TReturn}(EfFieldBuilder{TDbContext, TSource, TObject}, Func{IResolveFieldContext{TSource}, TObject, TReturn}, bool?, Type?)"/>
    public static EfFieldBuilder<TDbContext, TSource, TReturn> ThenResolve<TDbContext, TSource, TObject, TReturn>(
        EfFieldBuilder<TDbContext, TSource, TObject> fieldBuilder,
        Func<IResolveFieldContext<TSource>, TObject, Task<TReturn>> resolver,
        bool? nullable = null,
        Type? graphType = null)
    {
        return ThenResolve((FieldBuilder<TSource, TObject>)fieldBuilder, resolver, nullable, graphType)
            .AsEfFieldBuilder<TDbContext, TSource, TReturn>();
    }
}
