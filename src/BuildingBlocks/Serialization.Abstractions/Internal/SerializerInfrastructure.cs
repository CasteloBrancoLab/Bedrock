using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.IO;

namespace Bedrock.BuildingBlocks.Serialization.Abstractions.Internal;

/// <summary>
/// Shared infrastructure for serializer implementations.
/// </summary>
/// <remarks>
/// This class provides common utilities used across different serializer implementations
/// to reduce code duplication and ensure consistent behavior.
/// </remarks>
[ExcludeFromCodeCoverage(Justification = "Infraestrutura interna compartilhada - testada indiretamente atraves de serializadores concretos")]
public static class SerializerInfrastructure
{
    // Stryker disable all : RecyclableMemoryStreamManager configuration is internal infrastructure - values are performance tuning parameters

    /// <summary>
    /// Shared RecyclableMemoryStreamManager instance for all serializers.
    /// </summary>
    /// <remarks>
    /// Using a single shared instance reduces memory pressure and improves buffer reuse
    /// across all serialization operations in the application.
    /// </remarks>
    public static readonly RecyclableMemoryStreamManager StreamManager = new(new RecyclableMemoryStreamManager.Options
    {
        BlockSize = 4096,
        LargeBufferMultiple = 1024 * 1024,
        MaximumBufferSize = 16 * 1024 * 1024,
        GenerateCallStacks = false,
        AggressiveBufferReturn = true,
    });

    // Stryker restore all

    /// <summary>
    /// Standard binding flags for discovering serializable properties.
    /// </summary>
    public const BindingFlags PropertyBindingFlags = BindingFlags.Instance | BindingFlags.Public;

    /// <summary>
    /// Cache for serializable properties by type.
    /// Types are immutable, so their properties never change - safe to cache indefinitely.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> s_propertyCache = new();

    /// <summary>
    /// Determines whether a type is nullable (reference type or Nullable&lt;T&gt;).
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type can be null; otherwise, false.</returns>
    public static bool IsNullable(Type type)
    {
        return !type.IsValueType || Nullable.GetUnderlyingType(type) is not null;
    }

    /// <summary>
    /// Gets the serializable properties for a type.
    /// </summary>
    /// <param name="type">The type to get properties from.</param>
    /// <returns>An array of serializable properties ordered by name.</returns>
    /// <remarks>
    /// Results are cached per type for zero-allocation on subsequent calls.
    /// Properties are filtered to include only readable and writable non-indexed properties.
    /// </remarks>
    public static PropertyInfo[] GetSerializableProperties(Type type)
    {
        return s_propertyCache.GetOrAdd(type, static t => BuildSerializablePropertiesArray(t));
    }

    private static PropertyInfo[] BuildSerializablePropertiesArray(Type type)
    {
        var properties = type.GetProperties(PropertyBindingFlags);
        var count = 0;

        // First pass: count valid properties
        foreach (var prop in properties)
        {
            if (prop.CanRead && prop.CanWrite && prop.GetIndexParameters().Length == 0)
            {
                count++;
            }
        }

        if (count == 0)
        {
            return [];
        }

        // Second pass: collect valid properties
        var result = new PropertyInfo[count];
        var index = 0;
        foreach (var prop in properties)
        {
            if (prop.CanRead && prop.CanWrite && prop.GetIndexParameters().Length == 0)
            {
                result[index++] = prop;
            }
        }

        // Sort in-place by name
        Array.Sort(result, static (a, b) => StringComparer.Ordinal.Compare(a.Name, b.Name));

        return result;
    }
}
