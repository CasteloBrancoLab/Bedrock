using System.Linq.Expressions;
using System.Reflection;

namespace Bedrock.BuildingBlocks.Core.Utils;

/// <summary>
/// Utility methods for working with LINQ expressions.
/// </summary>
public static class ExpressionUtils
{
    /// <summary>
    /// Extracts the <see cref="PropertyInfo"/> from a property selector expression.
    /// </summary>
    /// <typeparam name="T">The type containing the property.</typeparam>
    /// <typeparam name="TProp">The property type.</typeparam>
    /// <param name="expression">A lambda expression selecting a property (e.g., x => x.Name).</param>
    /// <returns>The <see cref="PropertyInfo"/> of the selected property.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="expression"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the expression does not select a property.</exception>
    public static PropertyInfo GetProperty<T, TProp>(Expression<Func<T, TProp>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        if (expression.Body is MemberExpression memberExpression)
        {
            return (PropertyInfo)memberExpression.Member;
        }

        if (expression.Body is UnaryExpression unaryExpression &&
            unaryExpression.Operand is MemberExpression unaryMemberExpression)
        {
            return (PropertyInfo)unaryMemberExpression.Member;
        }

        throw new ArgumentException(
            "The expression must select a property, e.g.: x => x.Name",
            nameof(expression));
    }
}
