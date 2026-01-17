using System.Collections.Concurrent;
using System.Reflection;
using Bedrock.BuildingBlocks.Serialization.Protobuf.Interfaces;
using Bedrock.BuildingBlocks.Serialization.Protobuf.Models;
using Microsoft.IO;
using ProtoBuf.Meta;

namespace Bedrock.BuildingBlocks.Serialization.Protobuf;

public abstract class ProtobufSerializerBase
    : IProtobufSerializer
{
    private static readonly RecyclableMemoryStreamManager _streamManager = new(new RecyclableMemoryStreamManager.Options
    {
        BlockSize = 4096,
        LargeBufferMultiple = 1024 * 1024,
        MaximumBufferSize = 16 * 1024 * 1024,
        GenerateCallStacks = false,
        AggressiveBufferReturn = true,
    });

    private readonly Lock _modelInitLock = new();
    private readonly ConcurrentDictionary<Type, byte> _registeredTypes = new();
    private volatile bool _initialized;
    private RuntimeTypeModel _runtimeTypeModel = null!;

    public string Name { get; }
    public Options Options { get; }

    protected ProtobufSerializerBase(string name)
    {
        Name = name;
        Options = new Options();
        Initialize();
    }

    protected void Initialize()
    {
        if (_initialized)
            return;

        lock (_modelInitLock)
        {
            if (_initialized)
                return;

            _runtimeTypeModel = RuntimeTypeModel.Create(name: Name);

            ConfigureInternal(Options);
            Configure(Options);

            _runtimeTypeModel.CompileInPlace();
            _initialized = true;
        }
    }

    public byte[]? Serialize<TInput>(TInput? input)
    {
        if (input is null)
            return null;

        using RecyclableMemoryStream ms = _streamManager.GetStream();
        _ = _runtimeTypeModel.Serialize((Stream)ms, input);
        return ms.ToArray();
    }

    public byte[]? Serialize<TInput>(TInput? input, Type type)
    {
        return Serialize(input);
    }

    public void SerializeToStream<TInput>(TInput? input, Stream destination)
    {
        if (input is null)
            return;

        ArgumentNullException.ThrowIfNull(destination);
        _ = _runtimeTypeModel.Serialize(destination, input);
    }

    public void SerializeToStream<TInput>(TInput? input, Type type, Stream destination)
    {
        SerializeToStream(input, destination);
    }

    public Task<byte[]?> SerializeAsync<TInput>(TInput? input, CancellationToken cancellationToken)
    {
        return Task.FromResult(Serialize(input));
    }

    public Task<byte[]?> SerializeAsync<TInput>(TInput? input, Type type, CancellationToken cancellationToken)
    {
        return Task.FromResult(Serialize(input, type));
    }

    public Task SerializeToStreamAsync<TInput>(TInput? input, Stream destination, CancellationToken cancellationToken)
    {
        SerializeToStream(input, destination);
        return Task.CompletedTask;
    }

    public Task SerializeToStreamAsync<TInput>(TInput? input, Type type, Stream destination, CancellationToken cancellationToken)
    {
        SerializeToStream(input, type, destination);
        return Task.CompletedTask;
    }

    public TResult? Deserialize<TResult>(byte[]? input)
    {
        if (input is null || input.Length == 0)
            return default;

        using var ms = new MemoryStream(input, writable: false);
        return _runtimeTypeModel.Deserialize<TResult>(ms);
    }

    public TResult? Deserialize<TResult>(byte[]? input, Type type)
    {
        if (input is null || input.Length == 0)
            return default;

        using var ms = new MemoryStream(input, writable: false);
        return (TResult?)_runtimeTypeModel.Deserialize(type, ms);
    }

    public object? Deserialize(byte[]? input, Type type)
    {
        if (input is null || input.Length == 0)
            return default;

        using var ms = new MemoryStream(input, writable: false);
        return _runtimeTypeModel.Deserialize(type, ms);
    }

    public TResult? DeserializeFromStream<TResult>(Stream source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return _runtimeTypeModel.Deserialize<TResult>(source);
    }

    public TResult? DeserializeFromStream<TResult>(Stream source, Type type)
    {
        ArgumentNullException.ThrowIfNull(source);
        return (TResult?)_runtimeTypeModel.Deserialize(type, source);
    }

    public object? DeserializeFromStream(Stream source, Type type)
    {
        ArgumentNullException.ThrowIfNull(source);
        return _runtimeTypeModel.Deserialize(type, source);
    }

    public Task<TResult?> DeserializeAsync<TResult>(byte[]? input, CancellationToken cancellationToken)
    {
        return Task.FromResult(Deserialize<TResult>(input));
    }

    public Task<TResult?> DeserializeAsync<TResult>(byte[]? input, Type type, CancellationToken cancellationToken)
    {
        return Task.FromResult(Deserialize<TResult>(input, type));
    }

    public Task<object?> DeserializeAsync(byte[]? input, Type type, CancellationToken cancellationToken)
    {
        return Task.FromResult(Deserialize(input, type));
    }

    public Task<TResult?> DeserializeFromStreamAsync<TResult>(Stream source, CancellationToken cancellationToken)
    {
        return Task.FromResult(DeserializeFromStream<TResult>(source));
    }

    public Task<TResult?> DeserializeFromStreamAsync<TResult>(Stream source, Type type, CancellationToken cancellationToken)
    {
        return Task.FromResult(DeserializeFromStream<TResult>(source, type));
    }

    public Task<object?> DeserializeFromStreamAsync(Stream source, Type type, CancellationToken cancellationToken)
    {
        return Task.FromResult(DeserializeFromStream(source, type));
    }

    public string GenerateProtoFileContent(string package)
    {
        var options = new SchemaGenerationOptions
        {
            Syntax = ProtoSyntax.Proto3,
            Package = package,
        };

        return _runtimeTypeModel.GetSchema(options);
    }

    private static readonly BindingFlags _propertyFlags =
        BindingFlags.Instance | BindingFlags.Public;

    private void Configure(Options options)
    {
        if (options?.TypeCollection is null)
            return;

        foreach (Type? type in options.TypeCollection.OrderBy(t => t.FullName, StringComparer.Ordinal))
        {
            RegisterTypeIfNeeded(type);
        }
    }

    private void RegisterTypeIfNeeded(Type type)
    {
        if (type is null)
            return;

        if (!_registeredTypes.TryAdd(type, 1))
            return;

        RuntimeTypeModel model = _runtimeTypeModel;

        if (model.CanSerialize(type))
            return;

        if (type.IsEnum)
        {
            _ = model.Add(type, applyDefaultBehaviour: false);
            return;
        }

        MetaType meta = model.Add(type, applyDefaultBehaviour: false);

        PropertyInfo[] props = [.. type
            .GetProperties(_propertyFlags)
            .Where(p => p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0)
            .OrderBy(p => p.Name, StringComparer.Ordinal)];

        int field = 1;

        foreach (PropertyInfo? p in props)
        {
            _ = meta.Add(field++, p.Name);
        }
    }

    protected abstract void ConfigureInternal(Options options);
}
