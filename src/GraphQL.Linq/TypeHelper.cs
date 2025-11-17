// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using GraphQL.Types;

namespace GraphQL.Linq;

internal class TypeHelper
{
    public static bool GetNullable<TProperty>(LambdaExpression expression)
    {
        try {
            if (expression.Body is MemberExpression memberExpression) {
                var typeInfo = memberExpression.Member switch {
                    PropertyInfo propertyInfo => new TypeInformation(propertyInfo, false),
                    FieldInfo fieldInfo => new TypeInformation(fieldInfo, false),
                    _ => null
                };
                typeInfo?.ApplyAttributes();
                return typeInfo?.IsNullable ?? false;
            } else if (expression.Body is MethodCallExpression methodCallExpression) {
                var typeInfo = new TypeInformation(methodCallExpression.Method);
                typeInfo.ApplyAttributes();
                return typeInfo.IsNullable;
            } else {
                return typeof(TProperty).IsValueType && Nullable.GetUnderlyingType(typeof(TProperty)) != null;
            }
        } catch {
            return false;
        }
    }

    public static Type GetGraphType<TProperty>(string typeName, string fieldName, LambdaExpression expression, bool? nullable, Type? type, bool? forceId = false)
    {
        if (type != null)
            return type;

        try {
            if (type == null && nullable == null && GlobalSwitches.InferFieldNullabilityFromNRTAnnotations) {
                if (expression.Body is MemberExpression memberExpression) {
                    var typeInfo = memberExpression.Member switch {
                        PropertyInfo propertyInfo => new TypeInformation(propertyInfo, false),
                        FieldInfo fieldInfo => new TypeInformation(fieldInfo, false),
                        _ => throw new InvalidOperationException("Expression represents an unknown type of member.")
                    };
                    typeInfo.ApplyAttributes();
                    if (forceId ?? false)
                        typeInfo.GraphType = typeof(IdGraphType);
                    return typeInfo.ConstructGraphType();
                } else if (expression.Body is MethodCallExpression methodCallExpression) {
                    var typeInfo = new TypeInformation(methodCallExpression.Method);
                    typeInfo.ApplyAttributes();
                    if (forceId ?? false)
                        typeInfo.GraphType = typeof(IdGraphType);
                    return typeInfo.ConstructGraphType();
                } else {
                    nullable = typeof(TProperty).IsValueType && Nullable.GetUnderlyingType(typeof(TProperty)) != null;
                }
            }
            nullable ??= false;
            return forceId ?? false
                ? nullable.Value ? typeof(IdGraphType) : typeof(NonNullGraphType<IdGraphType>)
                : typeof(TProperty).GetGraphTypeFromType(nullable.Value, TypeMappingMode.OutputType);
        } catch (ArgumentOutOfRangeException exp) {
            throw new ArgumentException($"The GraphQL type for field '{typeName}.{fieldName}' could not be derived implicitly from expression '{expression}'. " + exp.Message, exp);
        }
    }
}
