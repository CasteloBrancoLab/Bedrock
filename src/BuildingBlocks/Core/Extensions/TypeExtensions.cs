using System.Reflection;

namespace Bedrock.BuildingBlocks.Core.Extensions;

/// <summary>
/// Extension methods for <see cref="Type"/>.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Gets all public constant string values defined in a type.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>An array of string values from all public constant fields of type string.</returns>
    public static string[] GetAllPublicConstantStringValues(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return [.. type
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)];
    }
}
