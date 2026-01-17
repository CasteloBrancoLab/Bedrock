namespace Bedrock.BuildingBlocks.Serialization.Abstractions;

/// <summary>
/// Interface for text-based serializers that output strings (e.g., JSON, XML).
/// Extends <see cref="ISerializer{TOutput}"/> with UTF-8 byte operations for performance.
/// </summary>
public interface IStringSerializer : ISerializer<string>
{
    /// <summary>
    /// Serializes the input object directly to UTF-8 bytes.
    /// </summary>
    /// <typeparam name="TInput">The type of the input object.</typeparam>
    /// <param name="input">The object to serialize.</param>
    /// <returns>The serialized UTF-8 bytes, or null if input is null.</returns>
    byte[]? SerializeToUtf8Bytes<TInput>(TInput? input);

    /// <summary>
    /// Serializes the input object directly to UTF-8 bytes using explicit type information.
    /// </summary>
    /// <typeparam name="TInput">The type of the input object.</typeparam>
    /// <param name="input">The object to serialize.</param>
    /// <param name="type">The explicit type to use for serialization.</param>
    /// <returns>The serialized UTF-8 bytes, or null if input is null.</returns>
    byte[]? SerializeToUtf8Bytes<TInput>(TInput? input, Type type);

    /// <summary>
    /// Asynchronously serializes the input object directly to UTF-8 bytes.
    /// </summary>
    /// <typeparam name="TInput">The type of the input object.</typeparam>
    /// <param name="input">The object to serialize.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The serialized UTF-8 bytes, or null if input is null.</returns>
    Task<byte[]?> SerializeToUtf8BytesAsync<TInput>(TInput? input, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously serializes the input object directly to UTF-8 bytes using explicit type information.
    /// </summary>
    /// <typeparam name="TInput">The type of the input object.</typeparam>
    /// <param name="input">The object to serialize.</param>
    /// <param name="type">The explicit type to use for serialization.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The serialized UTF-8 bytes, or null if input is null.</returns>
    Task<byte[]?> SerializeToUtf8BytesAsync<TInput>(TInput? input, Type type, CancellationToken cancellationToken);

    /// <summary>
    /// Deserializes from UTF-8 bytes to the specified type.
    /// </summary>
    /// <typeparam name="TResult">The type to deserialize to.</typeparam>
    /// <param name="input">The UTF-8 bytes to deserialize.</param>
    /// <returns>The deserialized object, or null if input is null.</returns>
    TResult? DeserializeFromUtf8Bytes<TResult>(byte[]? input);

    /// <summary>
    /// Deserializes from UTF-8 bytes to the specified type using explicit type information.
    /// </summary>
    /// <typeparam name="TResult">The type to deserialize to.</typeparam>
    /// <param name="input">The UTF-8 bytes to deserialize.</param>
    /// <param name="type">The explicit type to deserialize to.</param>
    /// <returns>The deserialized object, or null if input is null.</returns>
    TResult? DeserializeFromUtf8Bytes<TResult>(byte[]? input, Type type);

    /// <summary>
    /// Asynchronously deserializes from UTF-8 bytes to the specified type.
    /// </summary>
    /// <typeparam name="TResult">The type to deserialize to.</typeparam>
    /// <param name="input">The UTF-8 bytes to deserialize.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The deserialized object, or null if input is null.</returns>
    Task<TResult?> DeserializeFromUtf8BytesAsync<TResult>(byte[]? input, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously deserializes from UTF-8 bytes to the specified type using explicit type information.
    /// </summary>
    /// <typeparam name="TResult">The type to deserialize to.</typeparam>
    /// <param name="input">The UTF-8 bytes to deserialize.</param>
    /// <param name="type">The explicit type to deserialize to.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The deserialized object, or null if input is null.</returns>
    Task<TResult?> DeserializeFromUtf8BytesAsync<TResult>(byte[]? input, Type type, CancellationToken cancellationToken);
}
