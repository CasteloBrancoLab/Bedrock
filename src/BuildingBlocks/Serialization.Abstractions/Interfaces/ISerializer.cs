namespace Bedrock.BuildingBlocks.Serialization.Abstractions.Interfaces;

/// <summary>
/// Base interface for all serializers providing generic serialization and deserialization operations.
/// </summary>
/// <typeparam name="TOutput">The output type of serialization (e.g., string for JSON, byte[] for binary formats).</typeparam>
public interface ISerializer<TOutput>
{
    /// <summary>
    /// Serializes the input object to the output format.
    /// </summary>
    /// <typeparam name="TInput">The type of the input object.</typeparam>
    /// <param name="input">The object to serialize.</param>
    /// <returns>The serialized output, or null if input is null.</returns>
    TOutput? Serialize<TInput>(TInput? input);

    /// <summary>
    /// Serializes the input object to the output format using explicit type information.
    /// </summary>
    /// <typeparam name="TInput">The type of the input object.</typeparam>
    /// <param name="input">The object to serialize.</param>
    /// <param name="type">The explicit type to use for serialization.</param>
    /// <returns>The serialized output, or null if input is null.</returns>
    TOutput? Serialize<TInput>(TInput? input, Type type);

    /// <summary>
    /// Asynchronously serializes the input object to the output format.
    /// </summary>
    /// <typeparam name="TInput">The type of the input object.</typeparam>
    /// <param name="input">The object to serialize.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The serialized output, or null if input is null.</returns>
    Task<TOutput?> SerializeAsync<TInput>(TInput? input, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously serializes the input object to the output format using explicit type information.
    /// </summary>
    /// <typeparam name="TInput">The type of the input object.</typeparam>
    /// <param name="input">The object to serialize.</param>
    /// <param name="type">The explicit type to use for serialization.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The serialized output, or null if input is null.</returns>
    Task<TOutput?> SerializeAsync<TInput>(TInput? input, Type type, CancellationToken cancellationToken);

    /// <summary>
    /// Serializes the input object directly to a stream.
    /// </summary>
    /// <typeparam name="TInput">The type of the input object.</typeparam>
    /// <param name="input">The object to serialize.</param>
    /// <param name="destination">The destination stream.</param>
    void SerializeToStream<TInput>(TInput? input, Stream destination);

    /// <summary>
    /// Serializes the input object directly to a stream using explicit type information.
    /// </summary>
    /// <typeparam name="TInput">The type of the input object.</typeparam>
    /// <param name="input">The object to serialize.</param>
    /// <param name="type">The explicit type to use for serialization.</param>
    /// <param name="destination">The destination stream.</param>
    void SerializeToStream<TInput>(TInput? input, Type type, Stream destination);

    /// <summary>
    /// Asynchronously serializes the input object directly to a stream.
    /// </summary>
    /// <typeparam name="TInput">The type of the input object.</typeparam>
    /// <param name="input">The object to serialize.</param>
    /// <param name="destination">The destination stream.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task SerializeToStreamAsync<TInput>(TInput? input, Stream destination, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously serializes the input object directly to a stream using explicit type information.
    /// </summary>
    /// <typeparam name="TInput">The type of the input object.</typeparam>
    /// <param name="input">The object to serialize.</param>
    /// <param name="type">The explicit type to use for serialization.</param>
    /// <param name="destination">The destination stream.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task SerializeToStreamAsync<TInput>(TInput? input, Type type, Stream destination, CancellationToken cancellationToken);

    /// <summary>
    /// Deserializes the input to the specified type.
    /// </summary>
    /// <typeparam name="TResult">The type to deserialize to.</typeparam>
    /// <param name="input">The serialized input.</param>
    /// <returns>The deserialized object, or null if input is null.</returns>
    TResult? Deserialize<TResult>(TOutput? input);

    /// <summary>
    /// Deserializes the input to the specified type using explicit type information.
    /// </summary>
    /// <typeparam name="TResult">The type to deserialize to.</typeparam>
    /// <param name="input">The serialized input.</param>
    /// <param name="type">The explicit type to deserialize to.</param>
    /// <returns>The deserialized object, or null if input is null.</returns>
    TResult? Deserialize<TResult>(TOutput? input, Type type);

    /// <summary>
    /// Deserializes the input to the specified type.
    /// </summary>
    /// <param name="input">The serialized input.</param>
    /// <param name="type">The type to deserialize to.</param>
    /// <returns>The deserialized object, or null if input is null.</returns>
    object? Deserialize(TOutput? input, Type type);

    /// <summary>
    /// Asynchronously deserializes the input to the specified type.
    /// </summary>
    /// <typeparam name="TResult">The type to deserialize to.</typeparam>
    /// <param name="input">The serialized input.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The deserialized object, or null if input is null.</returns>
    Task<TResult?> DeserializeAsync<TResult>(TOutput? input, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously deserializes the input to the specified type using explicit type information.
    /// </summary>
    /// <typeparam name="TResult">The type to deserialize to.</typeparam>
    /// <param name="input">The serialized input.</param>
    /// <param name="type">The explicit type to deserialize to.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The deserialized object, or null if input is null.</returns>
    Task<TResult?> DeserializeAsync<TResult>(TOutput? input, Type type, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously deserializes the input to the specified type.
    /// </summary>
    /// <param name="input">The serialized input.</param>
    /// <param name="type">The type to deserialize to.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The deserialized object, or null if input is null.</returns>
    Task<object?> DeserializeAsync(TOutput? input, Type type, CancellationToken cancellationToken);

    /// <summary>
    /// Deserializes from a stream to the specified type.
    /// </summary>
    /// <typeparam name="TResult">The type to deserialize to.</typeparam>
    /// <param name="source">The source stream.</param>
    /// <returns>The deserialized object.</returns>
    TResult? DeserializeFromStream<TResult>(Stream source);

    /// <summary>
    /// Deserializes from a stream to the specified type using explicit type information.
    /// </summary>
    /// <typeparam name="TResult">The type to deserialize to.</typeparam>
    /// <param name="source">The source stream.</param>
    /// <param name="type">The explicit type to deserialize to.</param>
    /// <returns>The deserialized object.</returns>
    TResult? DeserializeFromStream<TResult>(Stream source, Type type);

    /// <summary>
    /// Deserializes from a stream to the specified type.
    /// </summary>
    /// <param name="source">The source stream.</param>
    /// <param name="type">The type to deserialize to.</param>
    /// <returns>The deserialized object.</returns>
    object? DeserializeFromStream(Stream source, Type type);

    /// <summary>
    /// Asynchronously deserializes from a stream to the specified type.
    /// </summary>
    /// <typeparam name="TResult">The type to deserialize to.</typeparam>
    /// <param name="source">The source stream.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The deserialized object.</returns>
    Task<TResult?> DeserializeFromStreamAsync<TResult>(Stream source, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously deserializes from a stream to the specified type using explicit type information.
    /// </summary>
    /// <typeparam name="TResult">The type to deserialize to.</typeparam>
    /// <param name="source">The source stream.</param>
    /// <param name="type">The explicit type to deserialize to.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The deserialized object.</returns>
    Task<TResult?> DeserializeFromStreamAsync<TResult>(Stream source, Type type, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously deserializes from a stream to the specified type.
    /// </summary>
    /// <param name="source">The source stream.</param>
    /// <param name="type">The type to deserialize to.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The deserialized object.</returns>
    Task<object?> DeserializeFromStreamAsync(Stream source, Type type, CancellationToken cancellationToken);
}
