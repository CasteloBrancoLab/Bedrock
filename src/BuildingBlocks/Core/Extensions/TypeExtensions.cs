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

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        // First pass: count matching fields to allocate exact-size array
        var count = 0;
        foreach (var field in fields)
        {
            if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
            {
                count++;
            }
        }

        // Second pass: populate array (when count is 0, new string[0] returns Array.Empty<string>())
        var result = new string[count];
        var index = 0;
        foreach (var field in fields)
        {
            if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
            {
                result[index++] = (string)field.GetRawConstantValue()!;
            }
        }

        return result;
    }
}
