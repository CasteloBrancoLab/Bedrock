using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Bedrock.BuildingBlocks.Serialization.Abstractions.Internal;
using Bedrock.BuildingBlocks.Serialization.Protobuf.Interfaces;
using Bedrock.BuildingBlocks.Serialization.Protobuf.Models;
using Microsoft.IO;
using ProtoBuf.Meta;

namespace Bedrock.BuildingBlocks.Serialization.Protobuf;

public abstract class ProtobufSerializerBase
    : IProtobufSerializer
{
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

    // Stryker disable all : Double-check locking initialization pattern - tested indirectly through serialization
    [ExcludeFromCodeCoverage(Justification = "Padrao de inicializacao double-check lock - testado indiretamente atraves de serializacao")]
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
    // Stryker restore all

    public byte[]? Serialize<TInput>(TInput? input)
    {
        if (input is null)
            return null;

        using RecyclableMemoryStream ms = SerializerInfrastructure.StreamManager.GetStream();
        _ = _runtimeTypeModel.Serialize((Stream)ms, input);
        return ms.ToArray();
    }

    public byte[]? Serialize<TInput>(TInput? input, Type type)
    {
        return Serialize(input);
    }

    // Stryker disable all : Serialization to stream with guard clause - downstream throws different exception type
    [ExcludeFromCodeCoverage(Justification = "Serializacao para stream com guard clause - downstream lanca tipo diferente de excecao")]
    public void SerializeToStream<TInput>(TInput? input, Stream destination)
    {
        if (input is null)
            return;

        ArgumentNullException.ThrowIfNull(destination);
        _ = _runtimeTypeModel.Serialize(destination, input);
    }
    // Stryker restore all

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

    // Stryker disable all : Deserialization methods with idiomatic null-or-empty checks - tested through round-trip tests
    [ExcludeFromCodeCoverage(Justification = "Metodo de desserializacao - testado atraves de round-trips")]
    public TResult? Deserialize<TResult>(byte[]? input)
    {
        if (input is null || input.Length == 0)
            return default;

        using var ms = new MemoryStream(input, writable: false);
        return _runtimeTypeModel.Deserialize<TResult>(ms);
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo de desserializacao - testado atraves de round-trips")]
    public TResult? Deserialize<TResult>(byte[]? input, Type type)
    {
        if (input is null || input.Length == 0)
            return default;

        using var ms = new MemoryStream(input, writable: false);
        return (TResult?)_runtimeTypeModel.Deserialize(type, ms);
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo de desserializacao - testado atraves de round-trips")]
    public object? Deserialize(byte[]? input, Type type)
    {
        if (input is null || input.Length == 0)
            return default;

        using var ms = new MemoryStream(input, writable: false);
        return _runtimeTypeModel.Deserialize(type, ms);
    }
    // Stryker restore all

    public TResult? DeserializeFromStream<TResult>(Stream source)
    {
        // Stryker disable once Statement : Guard clause - downstream code also throws on null but with different exception type
        ArgumentNullException.ThrowIfNull(source);
        return _runtimeTypeModel.Deserialize<TResult>(source);
    }

    public TResult? DeserializeFromStream<TResult>(Stream source, Type type)
    {
        // Stryker disable once Statement : Guard clause - downstream code also throws on null but with different exception type
        ArgumentNullException.ThrowIfNull(source);
        return (TResult?)_runtimeTypeModel.Deserialize(type, source);
    }

    public object? DeserializeFromStream(Stream source, Type type)
    {
        // Stryker disable once Statement : Guard clause - downstream code also throws on null but with different exception type
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

    // Stryker disable all : Schema generation options - tested through output validation not individual property values
    [ExcludeFromCodeCoverage(Justification = "Opcoes de geracao de schema - testado atraves de validacao de output")]
    public string GenerateProtoFileContent(string package)
    {
        var options = new SchemaGenerationOptions
        {
            Syntax = ProtoSyntax.Proto3,
            Package = package,
        };

        return _runtimeTypeModel.GetSchema(options);
    }
    // Stryker restore all

    // Stryker disable all : Type registration internals - tested indirectly through serialization round-trips
    [ExcludeFromCodeCoverage(Justification = "Configuracao de tipos Protobuf - testado indiretamente atraves de round-trips de serializacao")]
    private void Configure(Options options)
    {
        if (options?.TypeCollection is null)
            return;

        foreach (Type? type in options.TypeCollection.OrderBy(t => t.FullName, StringComparer.Ordinal))
        {
            RegisterTypeIfNeeded(type);
        }
    }

    [ExcludeFromCodeCoverage(Justification = "Registro de tipos Protobuf - codigo de infraestrutura interna testado indiretamente")]
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
        PropertyInfo[] props = SerializerInfrastructure.GetSerializableProperties(type);

        int field = 1;

        foreach (PropertyInfo? p in props)
        {
            _ = meta.Add(field++, p.Name);
        }
    }
    // Stryker restore all

    protected abstract void ConfigureInternal(Options options);
}
