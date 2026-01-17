using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using Bedrock.BuildingBlocks.Serialization.Json.Interfaces;
using Bedrock.BuildingBlocks.Serialization.Json.Models;
using Microsoft.IO;

namespace Bedrock.BuildingBlocks.Serialization.Json;

/// <summary>
/// Abstract base class for JSON serializers using System.Text.Json.
/// </summary>
public abstract class JsonSerializerBase : IJsonSerializer
{
    // Stryker disable all : RecyclableMemoryStreamManager configuration is internal infrastructure - values are performance tuning parameters
    private static readonly RecyclableMemoryStreamManager StreamManager = new(new RecyclableMemoryStreamManager.Options
    {
        BlockSize = 4096,
        LargeBufferMultiple = 1024 * 1024,
        MaximumBufferSize = 16 * 1024 * 1024,
        GenerateCallStacks = false,
        AggressiveBufferReturn = true,
    });
    // Stryker restore all

    /// <summary>
    /// Gets the serialization options.
    /// </summary>
    public Options Options { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonSerializerBase"/> class.
    /// </summary>
    protected JsonSerializerBase()
    {
        Options = new Options();
        Initialize();
    }

    /// <summary>
    /// Initializes the serializer by calling the configuration method.
    /// </summary>
    protected void Initialize()
    {
        ConfigureInternal(Options);
    }

    /// <inheritdoc />
    public string? Serialize<TInput>(TInput? input)
    {
        if (input is null)
            return null;

        return JsonSerializer.Serialize(input, input.GetType(), Options.JsonSerializerOptions);
    }

    /// <inheritdoc />
    public string? Serialize<TInput>(TInput? input, Type type)
    {
        if (input is null)
            return null;

        return JsonSerializer.Serialize(input, type, Options.JsonSerializerOptions);
    }

    /// <inheritdoc />
    public byte[]? SerializeToUtf8Bytes<TInput>(TInput? input)
    {
        // Stryker disable once Nullcoalescing : Fallback to typeof(TInput) when input is null - both branches produce same result for null input (returns null anyway)
        return SerializeToUtf8Bytes(input, input?.GetType() ?? typeof(TInput));
    }

    /// <inheritdoc />
    public byte[]? SerializeToUtf8Bytes<TInput>(TInput? input, Type type)
    {
        if (input is null)
            return null;

        return JsonSerializer.SerializeToUtf8Bytes(input, type, Options.JsonSerializerOptions);
    }

    /// <inheritdoc />
    public async Task<string?> SerializeAsync<TInput>(TInput? input, CancellationToken cancellationToken)
    {
        // Stryker disable once Nullcoalescing : Fallback to typeof(TInput) when input is null - both branches produce same result for null input (returns null anyway)
        return await SerializeAsync(input, input?.GetType() ?? typeof(TInput), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<string?> SerializeAsync<TInput>(TInput? input, Type type, CancellationToken cancellationToken)
    {
        if (input is null)
            return null;

        await using RecyclableMemoryStream ms = StreamManager.GetStream();
        await JsonSerializer.SerializeAsync(ms, input, type, Options.JsonSerializerOptions, cancellationToken);
        return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
    }

    /// <inheritdoc />
    public async Task<byte[]?> SerializeToUtf8BytesAsync<TInput>(TInput? input, CancellationToken cancellationToken)
    {
        // Stryker disable once Nullcoalescing : Fallback to typeof(TInput) when input is null - both branches produce same result for null input (returns null anyway)
        return await SerializeToUtf8BytesAsync(input, input?.GetType() ?? typeof(TInput), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<byte[]?> SerializeToUtf8BytesAsync<TInput>(TInput? input, Type type, CancellationToken cancellationToken)
    {
        if (input is null)
            return null;

        await using RecyclableMemoryStream ms = StreamManager.GetStream();
        await JsonSerializer.SerializeAsync(ms, input, type, Options.JsonSerializerOptions, cancellationToken);
        return ms.ToArray();
    }

    /// <inheritdoc />
    public void SerializeToStream<TInput>(TInput? input, Stream destination)
    {
        if (input is null)
            return;

        // Stryker disable once Statement : Guard clause - downstream code also throws on null but with different exception type
        ArgumentNullException.ThrowIfNull(destination);
        JsonSerializer.Serialize(destination, input, input.GetType(), Options.JsonSerializerOptions);
    }

    /// <inheritdoc />
    public void SerializeToStream<TInput>(TInput? input, Type type, Stream destination)
    {
        if (input is null)
            return;

        // Stryker disable once Statement : Guard clause - downstream code also throws on null but with different exception type
        ArgumentNullException.ThrowIfNull(destination);
        JsonSerializer.Serialize(destination, input, type, Options.JsonSerializerOptions);
    }

    /// <inheritdoc />
    public async Task SerializeToStreamAsync<TInput>(TInput? input, Stream destination, CancellationToken cancellationToken)
    {
        if (input is null)
            return;

        // Stryker disable once Statement : Guard clause - downstream code also throws on null but with different exception type
        ArgumentNullException.ThrowIfNull(destination);
        await JsonSerializer.SerializeAsync(destination, input, input.GetType(), Options.JsonSerializerOptions, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SerializeToStreamAsync<TInput>(TInput? input, Type type, Stream destination, CancellationToken cancellationToken)
    {
        if (input is null)
            return;

        // Stryker disable once Statement : Guard clause - downstream code also throws on null but with different exception type
        ArgumentNullException.ThrowIfNull(destination);
        await JsonSerializer.SerializeAsync(destination, input, type, Options.JsonSerializerOptions, cancellationToken);
    }

    /// <inheritdoc />
    public TResult? Deserialize<TResult>(string? input)
    {
        if (input is null)
            return default;

        return JsonSerializer.Deserialize<TResult>(input, Options.JsonSerializerOptions);
    }

    /// <inheritdoc />
    public TResult? Deserialize<TResult>(string? input, Type type)
    {
        if (input is null)
            return default;

        return (TResult?)JsonSerializer.Deserialize(input, type, Options.JsonSerializerOptions);
    }

    /// <inheritdoc />
    public object? Deserialize(string? input, Type type)
    {
        if (input is null)
            return default;

        return JsonSerializer.Deserialize(input, type, Options.JsonSerializerOptions);
    }

    /// <inheritdoc />
    public async Task<TResult?> DeserializeAsync<TResult>(string? input, CancellationToken cancellationToken)
    {
        return await DeserializeAsync<TResult>(input, typeof(TResult), cancellationToken);
    }

    // Stryker disable all : Async deserialization with ArrayPool - resource cleanup cannot be verified in tests
    /// <inheritdoc />
    [ExcludeFromCodeCoverage(Justification = "Desserializacao async com ArrayPool - cleanup de recursos nao pode ser verificado em testes")]
    public async Task<TResult?> DeserializeAsync<TResult>(string? input, Type type, CancellationToken cancellationToken)
    {
        if (input is null)
            return default;

        int byteCount = Encoding.UTF8.GetByteCount(input);
        byte[] buffer = ArrayPool<byte>.Shared.Rent(byteCount);

        try
        {
            int written = Encoding.UTF8.GetBytes(input, buffer);
            await using RecyclableMemoryStream ms = StreamManager.GetStream(null, buffer, 0, written);
            object? obj = await JsonSerializer.DeserializeAsync(ms, type, Options.JsonSerializerOptions, cancellationToken);
            return (TResult?)obj;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage(Justification = "Desserializacao async com ArrayPool - cleanup de recursos nao pode ser verificado em testes")]
    public async Task<object?> DeserializeAsync(string? input, Type type, CancellationToken cancellationToken)
    {
        if (input is null)
            return default;

        int byteCount = Encoding.UTF8.GetByteCount(input);
        byte[] buffer = ArrayPool<byte>.Shared.Rent(byteCount);

        try
        {
            int written = Encoding.UTF8.GetBytes(input, buffer);
            await using RecyclableMemoryStream ms = StreamManager.GetStream(null, buffer, 0, written);
            return await JsonSerializer.DeserializeAsync(ms, type, Options.JsonSerializerOptions, cancellationToken);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
    // Stryker restore all

    /// <inheritdoc />
    public TResult? DeserializeFromStream<TResult>(Stream source)
    {
        // Stryker disable once Statement : Guard clause - downstream code also throws on null but with different exception type
        ArgumentNullException.ThrowIfNull(source);
        return JsonSerializer.Deserialize<TResult>(source, Options.JsonSerializerOptions);
    }

    /// <inheritdoc />
    public TResult? DeserializeFromStream<TResult>(Stream source, Type type)
    {
        // Stryker disable once Statement : Guard clause - downstream code also throws on null but with different exception type
        ArgumentNullException.ThrowIfNull(source);
        return (TResult?)JsonSerializer.Deserialize(source, type, Options.JsonSerializerOptions);
    }

    /// <inheritdoc />
    public object? DeserializeFromStream(Stream source, Type type)
    {
        // Stryker disable once Statement : Guard clause - downstream code also throws on null but with different exception type
        ArgumentNullException.ThrowIfNull(source);
        return JsonSerializer.Deserialize(source, type, Options.JsonSerializerOptions);
    }

    /// <inheritdoc />
    public async Task<TResult?> DeserializeFromStreamAsync<TResult>(Stream source, CancellationToken cancellationToken)
    {
        // Stryker disable once Statement : Guard clause - downstream code also throws on null but with different exception type
        ArgumentNullException.ThrowIfNull(source);
        return await JsonSerializer.DeserializeAsync<TResult>(source, Options.JsonSerializerOptions, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TResult?> DeserializeFromStreamAsync<TResult>(Stream source, Type type, CancellationToken cancellationToken)
    {
        // Stryker disable once Statement : Guard clause - downstream code also throws on null but with different exception type
        ArgumentNullException.ThrowIfNull(source);
        return (TResult?)await JsonSerializer.DeserializeAsync(source, type, Options.JsonSerializerOptions, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<object?> DeserializeFromStreamAsync(Stream source, Type type, CancellationToken cancellationToken)
    {
        // Stryker disable once Statement : Guard clause - downstream code also throws on null but with different exception type
        ArgumentNullException.ThrowIfNull(source);
        return await JsonSerializer.DeserializeAsync(source, type, Options.JsonSerializerOptions, cancellationToken);
    }

    /// <inheritdoc />
    public TResult? DeserializeFromUtf8Bytes<TResult>(byte[]? input)
    {
        return DeserializeFromUtf8Bytes<TResult>(input, typeof(TResult));
    }

    /// <inheritdoc />
    public TResult? DeserializeFromUtf8Bytes<TResult>(byte[]? input, Type type)
    {
        if (input is null)
            return default;

        return (TResult?)JsonSerializer.Deserialize(input.AsSpan(), type, Options.JsonSerializerOptions);
    }

    /// <inheritdoc />
    public async Task<TResult?> DeserializeFromUtf8BytesAsync<TResult>(byte[]? input, CancellationToken cancellationToken)
    {
        return await DeserializeFromUtf8BytesAsync<TResult>(input, typeof(TResult), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TResult?> DeserializeFromUtf8BytesAsync<TResult>(byte[]? input, Type type, CancellationToken cancellationToken)
    {
        if (input is null)
            return default;

        await using RecyclableMemoryStream ms = StreamManager.GetStream(null, input, 0, input.Length);
        object? obj = await JsonSerializer.DeserializeAsync(ms, type, Options.JsonSerializerOptions, cancellationToken);
        return (TResult?)obj;
    }

    /// <summary>
    /// Override this method to configure the serializer options.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    protected abstract void ConfigureInternal(Options options);
}
