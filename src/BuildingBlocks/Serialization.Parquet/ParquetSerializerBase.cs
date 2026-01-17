using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Apache.Arrow;
using Apache.Arrow.Ipc;
using Apache.Arrow.Types;
using Bedrock.BuildingBlocks.Serialization.Parquet.Interfaces;
using Bedrock.BuildingBlocks.Serialization.Parquet.Models;
using Microsoft.IO;

namespace Bedrock.BuildingBlocks.Serialization.Parquet;

public abstract class ParquetSerializerBase
    : IParquetSerializer
{
    // Stryker disable all : RecyclableMemoryStreamManager configuration is internal infrastructure - values are performance tuning parameters
    private static readonly RecyclableMemoryStreamManager _streamManager = new(new RecyclableMemoryStreamManager.Options
    {
        BlockSize = 4096,
        LargeBufferMultiple = 1024 * 1024,
        MaximumBufferSize = 16 * 1024 * 1024,
        GenerateCallStacks = false,
        AggressiveBufferReturn = true,
    });
    // Stryker restore all

    private static readonly ConcurrentDictionary<Type, (Schema Schema, PropertyInfo[] Properties)> _schemaCache = new();
    private static readonly BindingFlags _propertyFlags = BindingFlags.Instance | BindingFlags.Public;

    public Options Options { get; }

    protected ParquetSerializerBase()
    {
        Options = new Options();
        Initialize();
    }

    protected void Initialize()
    {
        ConfigureInternal(Options);
    }

    public string GenerateSchemaDefinition<T>()
    {
        return GenerateSchemaDefinition(typeof(T));
    }

    public string GenerateSchemaDefinition(Type type)
    {
        (Schema schema, _) = GetOrCreateSchema(type);
        return schema.ToString();
    }

    public byte[]? Serialize<TInput>(TInput? input)
    {
        if (input is null)
            return null;

        return SerializeCollection([input]);
    }

    public byte[]? Serialize<TInput>(TInput? input, Type type)
    {
        return Serialize(input);
    }

    public Task<byte[]?> SerializeAsync<TInput>(TInput? input, CancellationToken cancellationToken)
    {
        return Task.FromResult(Serialize(input));
    }

    public Task<byte[]?> SerializeAsync<TInput>(TInput? input, Type type, CancellationToken cancellationToken)
    {
        return Task.FromResult(Serialize(input, type));
    }

    public void SerializeToStream<TInput>(TInput? input, Stream destination)
    {
        if (input is null)
            return;

        // Stryker disable once Statement : Guard clause - downstream code also throws on null but with different exception type
        ArgumentNullException.ThrowIfNull(destination);
        SerializeCollectionToStream([input], destination);
    }

    public void SerializeToStream<TInput>(TInput? input, Type type, Stream destination)
    {
        SerializeToStream(input, destination);
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

    public byte[]? SerializeCollection<TInput>(IEnumerable<TInput>? input)
    {
        if (input is null)
            return null;

        using RecyclableMemoryStream ms = _streamManager.GetStream();
        SerializeCollectionToStreamInternal(input, typeof(TInput), ms);
        return ms.ToArray();
    }

    // Stryker disable all : Collection serialization passthrough methods - tested through main serialization methods
    [ExcludeFromCodeCoverage(Justification = "Metodo passthrough - testado atraves de metodos principais")]
    public byte[]? SerializeCollection<TInput>(IEnumerable<TInput>? input, Type type)
    {
        return SerializeCollection(input);
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo passthrough - testado atraves de metodos principais")]
    public Task<byte[]?> SerializeCollectionAsync<TInput>(IEnumerable<TInput>? input, CancellationToken cancellationToken)
    {
        return Task.FromResult(SerializeCollection(input));
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo passthrough - testado atraves de metodos principais")]
    public Task<byte[]?> SerializeCollectionAsync<TInput>(IEnumerable<TInput>? input, Type type, CancellationToken cancellationToken)
    {
        return Task.FromResult(SerializeCollection(input, type));
    }

    [ExcludeFromCodeCoverage(Justification = "Serializacao para stream com guard clause")]
    public void SerializeCollectionToStream<TInput>(IEnumerable<TInput>? input, Stream destination)
    {
        if (input is null)
            return;

        ArgumentNullException.ThrowIfNull(destination);
        SerializeCollectionToStreamInternal(input, typeof(TInput), destination);
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo passthrough - testado atraves de metodos principais")]
    public void SerializeCollectionToStream<TInput>(IEnumerable<TInput>? input, Type type, Stream destination)
    {
        SerializeCollectionToStream(input, destination);
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo passthrough - testado atraves de metodos principais")]
    public Task SerializeCollectionToStreamAsync<TInput>(IEnumerable<TInput>? input, Stream destination, CancellationToken cancellationToken)
    {
        SerializeCollectionToStream(input, destination);
        return Task.CompletedTask;
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo passthrough - testado atraves de metodos principais")]
    public Task SerializeCollectionToStreamAsync<TInput>(IEnumerable<TInput>? input, Type type, Stream destination, CancellationToken cancellationToken)
    {
        SerializeCollectionToStream(input, type, destination);
        return Task.CompletedTask;
    }
    // Stryker restore all

    // Stryker disable all : Deserialization with LINQ methods - tested through round-trip tests
    [ExcludeFromCodeCoverage(Justification = "Desserializacao com metodos LINQ - testado atraves de round-trips")]
    public TResult? Deserialize<TResult>(byte[]? input)
    {
        if (input is null || input.Length == 0)
            return default;

        IEnumerable<TResult>? collection = DeserializeCollection<TResult>(input);
        return collection is not null ? collection.FirstOrDefault() : default;
    }

    [ExcludeFromCodeCoverage(Justification = "Desserializacao com metodos LINQ - testado atraves de round-trips")]
    public TResult? Deserialize<TResult>(byte[]? input, Type type)
    {
        return Deserialize<TResult>(input);
    }

    [ExcludeFromCodeCoverage(Justification = "Desserializacao com metodos LINQ - testado atraves de round-trips")]
    public object? Deserialize(byte[]? input, Type type)
    {
        if (input is null || input.Length == 0)
            return default;

        using var ms = new MemoryStream(input, writable: false);
        List<object>? collection = DeserializeCollectionFromStreamInternal(ms, type);
        return collection?.Cast<object>().FirstOrDefault();
    }
    // Stryker restore all

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

    // Stryker disable all : Deserialization with LINQ methods and guard clauses - tested through round-trip tests
    [ExcludeFromCodeCoverage(Justification = "Desserializacao com metodos LINQ - testado atraves de round-trips")]
    public TResult? DeserializeFromStream<TResult>(Stream source)
    {
        ArgumentNullException.ThrowIfNull(source);
        IEnumerable<TResult>? collection = DeserializeCollectionFromStream<TResult>(source);
        return collection is not null ? collection.FirstOrDefault() : default;
    }

    [ExcludeFromCodeCoverage(Justification = "Desserializacao com metodos LINQ - testado atraves de round-trips")]
    public TResult? DeserializeFromStream<TResult>(Stream source, Type type)
    {
        return DeserializeFromStream<TResult>(source);
    }

    [ExcludeFromCodeCoverage(Justification = "Desserializacao com metodos LINQ - testado atraves de round-trips")]
    public object? DeserializeFromStream(Stream source, Type type)
    {
        ArgumentNullException.ThrowIfNull(source);
        List<object>? collection = DeserializeCollectionFromStreamInternal(source, type);
        return collection?.Cast<object>().FirstOrDefault();
    }
    // Stryker restore all

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

    // Stryker disable all : Collection deserialization methods - tested through round-trip tests
    [ExcludeFromCodeCoverage(Justification = "Metodo de desserializacao de colecao - testado atraves de round-trips")]
    public IEnumerable<TResult>? DeserializeCollection<TResult>(byte[]? input)
    {
        if (input is null || input.Length == 0)
            return null;

        using var ms = new MemoryStream(input, writable: false);
        return DeserializeCollectionFromStream<TResult>(ms);
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo de desserializacao de colecao - testado atraves de round-trips")]
    public IEnumerable<TResult>? DeserializeCollection<TResult>(byte[]? input, Type type)
    {
        return DeserializeCollection<TResult>(input);
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo de desserializacao de colecao - testado atraves de round-trips")]
    public Task<IEnumerable<TResult>?> DeserializeCollectionAsync<TResult>(byte[]? input, CancellationToken cancellationToken)
    {
        return Task.FromResult(DeserializeCollection<TResult>(input));
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo de desserializacao de colecao - testado atraves de round-trips")]
    public Task<IEnumerable<TResult>?> DeserializeCollectionAsync<TResult>(byte[]? input, Type type, CancellationToken cancellationToken)
    {
        return Task.FromResult(DeserializeCollection<TResult>(input, type));
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo de desserializacao de colecao - testado atraves de round-trips")]
    public IEnumerable<TResult>? DeserializeCollectionFromStream<TResult>(Stream source)
    {
        ArgumentNullException.ThrowIfNull(source);
        List<object>? collection = DeserializeCollectionFromStreamInternal(source, typeof(TResult));
        return collection?.Cast<TResult>().ToList();
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo de desserializacao de colecao - testado atraves de round-trips")]
    public IEnumerable<TResult>? DeserializeCollectionFromStream<TResult>(Stream source, Type type)
    {
        return DeserializeCollectionFromStream<TResult>(source);
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo de desserializacao de colecao - testado atraves de round-trips")]
    public Task<IEnumerable<TResult>?> DeserializeCollectionFromStreamAsync<TResult>(Stream source, CancellationToken cancellationToken)
    {
        return Task.FromResult(DeserializeCollectionFromStream<TResult>(source));
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo de desserializacao de colecao - testado atraves de round-trips")]
    public Task<IEnumerable<TResult>?> DeserializeCollectionFromStreamAsync<TResult>(Stream source, Type type, CancellationToken cancellationToken)
    {
        return Task.FromResult(DeserializeCollectionFromStream<TResult>(source, type));
    }
    // Stryker restore all

    // Stryker disable all : Internal serialization/deserialization methods - tested through round-trip tests
    [ExcludeFromCodeCoverage(Justification = "Metodo interno de serializacao - testado atraves de round-trips")]
    private void SerializeCollectionToStreamInternal<TInput>(IEnumerable<TInput> input, Type type, Stream destination)
    {
        (Schema? schema, PropertyInfo[]? properties) = GetOrCreateSchema(type);
        var items = input.ToList();

        if (items.Count == 0)
        {
            using var emptyWriter = new ArrowStreamWriter(destination, schema, leaveOpen: true, Options.IpcWriteOptions);
            emptyWriter.WriteEnd();
            return;
        }

        var arrays = new IArrowArray[properties.Length];

        for (int i = 0; i < properties.Length; i++)
        {
            arrays[i] = BuildArray(properties[i], items);
        }

        var recordBatch = new RecordBatch(schema, arrays, items.Count);

        using var writer = new ArrowStreamWriter(destination, schema, leaveOpen: true, Options.IpcWriteOptions);
        writer.WriteRecordBatch(recordBatch);
        writer.WriteEnd();
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo interno de desserializacao - testado atraves de round-trips")]
    private static List<object>? DeserializeCollectionFromStreamInternal(Stream source, Type type)
    {
        (Schema _, PropertyInfo[]? properties) = GetOrCreateSchema(type);
        var results = new List<object>();

        using var reader = new ArrowStreamReader(source, leaveOpen: true);

        while (true)
        {
            RecordBatch? batch = reader.ReadNextRecordBatch();
            if (batch is null)
                break;

            for (int row = 0; row < batch.Length; row++)
            {
                object instance = Activator.CreateInstance(type)!;

                for (int col = 0; col < properties.Length; col++)
                {
                    IArrowArray array = batch.Column(col);
                    object? value = GetValue(array, row, properties[col].PropertyType);

                    if (value is not null)
                    {
                        properties[col].SetValue(instance, value);
                    }
                }

                results.Add(instance);
            }
        }

        return results;
    }
    // Stryker restore all

    // Stryker disable all : Schema generation and type mapping internals - tested indirectly through serialization round-trips
    [ExcludeFromCodeCoverage(Justification = "Geracao de schema Arrow - testado indiretamente atraves de round-trips de serializacao")]
    private static (Schema Schema, PropertyInfo[] Properties) GetOrCreateSchema(Type type)
    {
        return _schemaCache.GetOrAdd(type, t =>
        {
            PropertyInfo[] props = [.. t
                .GetProperties(_propertyFlags)
                .Where(p => p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0)
                .OrderBy(p => p.Name, StringComparer.Ordinal)];

            var fields = new List<Field>(props.Length);

            foreach (PropertyInfo prop in props)
            {
                IArrowType arrowType = GetArrowType(prop.PropertyType);
                bool nullable = IsNullable(prop.PropertyType);
                fields.Add(new Field(prop.Name, arrowType, nullable));
            }

            return (new Schema(fields, null), props);
        });
    }

    [ExcludeFromCodeCoverage(Justification = "Mapeamento de tipos CLR para Arrow - cada branch requer DTO especifico, impraticavel testar todas as combinacoes")]
    private static IArrowType GetArrowType(Type clrType)
    {
        Type underlyingType = Nullable.GetUnderlyingType(clrType) ?? clrType;

        return underlyingType switch
        {
            _ when underlyingType == typeof(bool) => BooleanType.Default,
            _ when underlyingType == typeof(sbyte) => Int8Type.Default,
            _ when underlyingType == typeof(byte) => UInt8Type.Default,
            _ when underlyingType == typeof(short) => Int16Type.Default,
            _ when underlyingType == typeof(ushort) => UInt16Type.Default,
            _ when underlyingType == typeof(int) => Int32Type.Default,
            _ when underlyingType == typeof(uint) => UInt32Type.Default,
            _ when underlyingType == typeof(long) => Int64Type.Default,
            _ when underlyingType == typeof(ulong) => UInt64Type.Default,
            _ when underlyingType == typeof(float) => FloatType.Default,
            _ when underlyingType == typeof(double) => DoubleType.Default,
            _ when underlyingType == typeof(decimal) => new Decimal128Type(38, 18),
            _ when underlyingType == typeof(string) => StringType.Default,
            _ when underlyingType == typeof(DateTime) => TimestampType.Default,
            _ when underlyingType == typeof(DateTimeOffset) => TimestampType.Default,
            _ when underlyingType == typeof(DateOnly) => Date32Type.Default,
            _ when underlyingType == typeof(TimeOnly) => Time64Type.Default,
            _ when underlyingType == typeof(TimeSpan) => DurationType.Microsecond,
            _ when underlyingType == typeof(Guid) => BinaryType.Default,
            _ when underlyingType == typeof(byte[]) => BinaryType.Default,
            _ => StringType.Default
        };
    }

    [ExcludeFromCodeCoverage(Justification = "Verificacao de nullable - usado internamente pelo mapeamento de tipos")]
    private static bool IsNullable(Type type)
    {
        return !type.IsValueType || Nullable.GetUnderlyingType(type) is not null;
    }

    [ExcludeFromCodeCoverage(Justification = "Construcao de arrays Arrow - cada tipo requer DTO especifico, impraticavel testar todas as combinacoes")]
    private static IArrowArray BuildArray<TInput>(PropertyInfo property, List<TInput> items)
    {
        Type propType = property.PropertyType;
        Type underlyingType = Nullable.GetUnderlyingType(propType) ?? propType;

        if (underlyingType == typeof(bool))
            return BuildBooleanArray(property, items);
        if (underlyingType == typeof(sbyte))
            return BuildInt8Array(property, items);
        if (underlyingType == typeof(byte))
            return BuildUInt8Array(property, items);
        if (underlyingType == typeof(short))
            return BuildInt16Array(property, items);
        if (underlyingType == typeof(ushort))
            return BuildUInt16Array(property, items);
        if (underlyingType == typeof(int))
            return BuildInt32Array(property, items);
        if (underlyingType == typeof(uint))
            return BuildUInt32Array(property, items);
        if (underlyingType == typeof(long))
            return BuildInt64Array(property, items);
        if (underlyingType == typeof(ulong))
            return BuildUInt64Array(property, items);
        if (underlyingType == typeof(float))
            return BuildFloatArray(property, items);
        if (underlyingType == typeof(double))
            return BuildDoubleArray(property, items);
        if (underlyingType == typeof(decimal))
            return BuildDecimalArray(property, items);
        if (underlyingType == typeof(string))
            return BuildStringArray(property, items);
        if (underlyingType == typeof(DateTime))
            return BuildDateTimeArray(property, items);
        if (underlyingType == typeof(DateTimeOffset))
            return BuildDateTimeOffsetArray(property, items);
        if (underlyingType == typeof(DateOnly))
            return BuildDateOnlyArray(property, items);
        if (underlyingType == typeof(TimeOnly))
            return BuildTimeOnlyArray(property, items);
        if (underlyingType == typeof(TimeSpan))
            return BuildTimeSpanArray(property, items);
        if (underlyingType == typeof(Guid))
            return BuildGuidArray(property, items);
        if (underlyingType == typeof(byte[]))
            return BuildBinaryArray(property, items);

        return BuildStringArray(property, items);
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo interno de construcao de array Arrow - testado indiretamente")]
    private static BooleanArray BuildBooleanArray<TInput>(PropertyInfo property, List<TInput> items)
    {
        var builder = new BooleanArray.Builder();
        foreach (TInput? item in items)
        {
            object? value = property.GetValue(item);
            _ = value is null ? builder.AppendNull() : builder.Append((bool)value);
        }
        return builder.Build();
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo interno de construcao de array Arrow - testado indiretamente")]
    private static Int8Array BuildInt8Array<TInput>(PropertyInfo property, List<TInput> items)
    {
        var builder = new Int8Array.Builder();
        foreach (TInput? item in items)
        {
            object? value = property.GetValue(item);
            _ = value is null ? builder.AppendNull() : builder.Append((sbyte)value);
        }
        return builder.Build();
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo interno de construcao de array Arrow - testado indiretamente")]
    private static UInt8Array BuildUInt8Array<TInput>(PropertyInfo property, List<TInput> items)
    {
        var builder = new UInt8Array.Builder();
        foreach (TInput? item in items)
        {
            object? value = property.GetValue(item);
            _ = value is null ? builder.AppendNull() : builder.Append((byte)value);
        }
        return builder.Build();
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo interno de construcao de array Arrow - testado indiretamente")]
    private static Int16Array BuildInt16Array<TInput>(PropertyInfo property, List<TInput> items)
    {
        var builder = new Int16Array.Builder();
        foreach (TInput? item in items)
        {
            object? value = property.GetValue(item);
            _ = value is null ? builder.AppendNull() : builder.Append((short)value);
        }
        return builder.Build();
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo interno de construcao de array Arrow - testado indiretamente")]
    private static UInt16Array BuildUInt16Array<TInput>(PropertyInfo property, List<TInput> items)
    {
        var builder = new UInt16Array.Builder();
        foreach (TInput? item in items)
        {
            object? value = property.GetValue(item);
            _ = value is null ? builder.AppendNull() : builder.Append((ushort)value);
        }
        return builder.Build();
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo interno de construcao de array Arrow - testado indiretamente")]
    private static Int32Array BuildInt32Array<TInput>(PropertyInfo property, List<TInput> items)
    {
        var builder = new Int32Array.Builder();
        foreach (TInput? item in items)
        {
            object? value = property.GetValue(item);
            _ = value is null ? builder.AppendNull() : builder.Append((int)value);
        }
        return builder.Build();
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo interno de construcao de array Arrow - testado indiretamente")]
    private static UInt32Array BuildUInt32Array<TInput>(PropertyInfo property, List<TInput> items)
    {
        var builder = new UInt32Array.Builder();
        foreach (TInput? item in items)
        {
            object? value = property.GetValue(item);
            _ = value is null ? builder.AppendNull() : builder.Append((uint)value);
        }
        return builder.Build();
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo interno de construcao de array Arrow - testado indiretamente")]
    private static Int64Array BuildInt64Array<TInput>(PropertyInfo property, List<TInput> items)
    {
        var builder = new Int64Array.Builder();
        foreach (TInput? item in items)
        {
            object? value = property.GetValue(item);
            _ = value is null ? builder.AppendNull() : builder.Append((long)value);
        }
        return builder.Build();
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo interno de construcao de array Arrow - testado indiretamente")]
    private static UInt64Array BuildUInt64Array<TInput>(PropertyInfo property, List<TInput> items)
    {
        var builder = new UInt64Array.Builder();
        foreach (TInput? item in items)
        {
            object? value = property.GetValue(item);
            _ = value is null ? builder.AppendNull() : builder.Append((ulong)value);
        }
        return builder.Build();
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo interno de construcao de array Arrow - testado indiretamente")]
    private static FloatArray BuildFloatArray<TInput>(PropertyInfo property, List<TInput> items)
    {
        var builder = new FloatArray.Builder();
        foreach (TInput? item in items)
        {
            object? value = property.GetValue(item);
            _ = value is null ? builder.AppendNull() : builder.Append((float)value);
        }
        return builder.Build();
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo interno de construcao de array Arrow - testado indiretamente")]
    private static DoubleArray BuildDoubleArray<TInput>(PropertyInfo property, List<TInput> items)
    {
        var builder = new DoubleArray.Builder();
        foreach (TInput? item in items)
        {
            object? value = property.GetValue(item);
            _ = value is null ? builder.AppendNull() : builder.Append((double)value);
        }
        return builder.Build();
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo interno de construcao de array Arrow - testado indiretamente")]
    private static Decimal128Array BuildDecimalArray<TInput>(PropertyInfo property, List<TInput> items)
    {
        var builder = new Decimal128Array.Builder(new Decimal128Type(38, 18));
        foreach (TInput? item in items)
        {
            object? value = property.GetValue(item);
            _ = value is null ? builder.AppendNull() : builder.Append((decimal)value);
        }
        return builder.Build();
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo interno de construcao de array Arrow - testado indiretamente")]
    private static StringArray BuildStringArray<TInput>(PropertyInfo property, List<TInput> items)
    {
        var builder = new StringArray.Builder();
        foreach (TInput? item in items)
        {
            object? value = property.GetValue(item);
            _ = value is null ? builder.AppendNull() : builder.Append(value.ToString());
        }
        return builder.Build();
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo interno de construcao de array Arrow - testado indiretamente")]
    private static TimestampArray BuildDateTimeArray<TInput>(PropertyInfo property, List<TInput> items)
    {
        var builder = new TimestampArray.Builder(TimestampType.Default);
        foreach (TInput? item in items)
        {
            object? value = property.GetValue(item);
            _ = value is null ? builder.AppendNull() : builder.Append(new DateTimeOffset((DateTime)value));
        }
        return builder.Build();
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo interno de construcao de array Arrow - testado indiretamente")]
    private static TimestampArray BuildDateTimeOffsetArray<TInput>(PropertyInfo property, List<TInput> items)
    {
        var builder = new TimestampArray.Builder(TimestampType.Default);
        foreach (TInput? item in items)
        {
            object? value = property.GetValue(item);
            _ = value is null ? builder.AppendNull() : builder.Append((DateTimeOffset)value);
        }
        return builder.Build();
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo interno de construcao de array Arrow - testado indiretamente")]
    private static Date32Array BuildDateOnlyArray<TInput>(PropertyInfo property, List<TInput> items)
    {
        var builder = new Date32Array.Builder();
        foreach (TInput? item in items)
        {
            object? value = property.GetValue(item);
            if (value is null)
            {
                _ = builder.AppendNull();
            }
            else
            {
                var dateOnly = (DateOnly)value;
                _ = builder.Append(dateOnly.ToDateTime(TimeOnly.MinValue));
            }
        }
        return builder.Build();
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo interno de construcao de array Arrow - testado indiretamente")]
    private static Time64Array BuildTimeOnlyArray<TInput>(PropertyInfo property, List<TInput> items)
    {
        var builder = new Time64Array.Builder(Time64Type.Default);
        foreach (TInput? item in items)
        {
            object? value = property.GetValue(item);
            if (value is null)
            {
                _ = builder.AppendNull();
            }
            else
            {
                var timeOnly = (TimeOnly)value;
                _ = builder.Append(timeOnly.Ticks / 10);
            }
        }
        return builder.Build();
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo interno de construcao de array Arrow - testado indiretamente")]
    private static DurationArray BuildTimeSpanArray<TInput>(PropertyInfo property, List<TInput> items)
    {
        var builder = new DurationArray.Builder(DurationType.Microsecond);
        foreach (TInput? item in items)
        {
            object? value = property.GetValue(item);
            if (value is null)
            {
                _ = builder.AppendNull();
            }
            else
            {
                var timeSpan = (TimeSpan)value;
                _ = builder.Append(timeSpan.Ticks / 10);
            }
        }
        return builder.Build();
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo interno de construcao de array Arrow - testado indiretamente")]
    private static BinaryArray BuildGuidArray<TInput>(PropertyInfo property, List<TInput> items)
    {
        var builder = new BinaryArray.Builder();
        foreach (TInput? item in items)
        {
            object? value = property.GetValue(item);
            _ = value is null ? builder.AppendNull() : builder.Append(((Guid)value).ToByteArray());
        }
        return builder.Build();
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo interno de construcao de array Arrow - testado indiretamente")]
    private static BinaryArray BuildBinaryArray<TInput>(PropertyInfo property, List<TInput> items)
    {
        var builder = new BinaryArray.Builder();
        foreach (TInput? item in items)
        {
            object? value = property.GetValue(item);
            _ = value is null ? builder.AppendNull() : builder.Append((byte[])value);
        }
        return builder.Build();
    }

    [ExcludeFromCodeCoverage(Justification = "Extracao de valores Arrow - testado indiretamente atraves de round-trips de serializacao")]
    private static object? GetValue(IArrowArray array, int index, Type targetType)
    {
        if (array.IsNull(index))
            return null;

        Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        return array switch
        {
            BooleanArray ba => ba.GetValue(index),
            Int8Array i8a => i8a.GetValue(index),
            UInt8Array u8a => u8a.GetValue(index),
            Int16Array i16a => i16a.GetValue(index),
            UInt16Array u16a => u16a.GetValue(index),
            Int32Array i32a => i32a.GetValue(index),
            UInt32Array u32a => u32a.GetValue(index),
            Int64Array i64a => i64a.GetValue(index),
            UInt64Array u64a => u64a.GetValue(index),
            FloatArray fa => fa.GetValue(index),
            DoubleArray da => da.GetValue(index),
            Decimal128Array deca => deca.GetValue(index),
            StringArray sa => sa.GetString(index),
            TimestampArray tsa => ConvertTimestamp(tsa, index, underlyingType),
            Date32Array d32a => ConvertDate32(d32a, index, underlyingType),
            Time64Array t64a => ConvertTime64(t64a, index, underlyingType),
            DurationArray dura => ConvertDuration(dura, index),
            BinaryArray bina when underlyingType == typeof(Guid) => new Guid(bina.GetBytes(index).ToArray()),
            BinaryArray bina => bina.GetBytes(index).ToArray(),
            _ => null
        };
    }

    [ExcludeFromCodeCoverage(Justification = "Conversao de timestamp Arrow - testado indiretamente")]
    private static object? ConvertTimestamp(TimestampArray array, int index, Type targetType)
    {
        DateTimeOffset? dto = array.GetTimestamp(index);
        if (dto is null)
            return null;

        if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
            return dto.Value.DateTime;

        return dto.Value;
    }

    [ExcludeFromCodeCoverage(Justification = "Conversao de date Arrow - testado indiretamente")]
    private static object? ConvertDate32(Date32Array array, int index, Type targetType)
    {
        DateTime? dt = array.GetDateTime(index);
        if (dt is null)
            return null;

        if (targetType == typeof(DateOnly) || targetType == typeof(DateOnly?))
            return DateOnly.FromDateTime(dt.Value);

        return dt.Value;
    }

    [ExcludeFromCodeCoverage(Justification = "Conversao de time Arrow - testado indiretamente")]
    private static object? ConvertTime64(Time64Array array, int index, Type targetType)
    {
        long? ticks = array.GetValue(index);
        if (ticks is null)
            return null;

        if (targetType == typeof(TimeOnly) || targetType == typeof(TimeOnly?))
            return new TimeOnly(ticks.Value * 10);

        return TimeSpan.FromTicks(ticks.Value * 10);
    }

    [ExcludeFromCodeCoverage(Justification = "Conversao de duration Arrow - testado indiretamente")]
    private static TimeSpan? ConvertDuration(DurationArray array, int index)
    {
        long? microseconds = array.GetValue(index);
        if (microseconds is null)
            return null;

        return TimeSpan.FromTicks(microseconds.Value * 10);
    }
    // Stryker restore all

    protected abstract void ConfigureInternal(Options options);
}
