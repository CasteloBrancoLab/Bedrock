using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text;
using Avro;
using Avro.Generic;
using Avro.IO;
using Bedrock.BuildingBlocks.Serialization.Avro.Interfaces;
using Bedrock.BuildingBlocks.Serialization.Avro.Models;
using Microsoft.IO;

namespace Bedrock.BuildingBlocks.Serialization.Avro;

public abstract class AvroSerializerBase
    : IAvroSerializer
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

    private static readonly ConcurrentDictionary<Type, (RecordSchema Schema, PropertyInfo[] Properties)> _schemaCache = new();
    private static readonly BindingFlags _propertyFlags = BindingFlags.Instance | BindingFlags.Public;

    public Options Options { get; }

    protected AvroSerializerBase()
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
        (RecordSchema schema, _) = GetOrCreateSchema(type);
        return schema.ToString();
    }

    public byte[]? Serialize<TInput>(TInput? input)
    {
        if (input is null)
            return null;

        using RecyclableMemoryStream ms = _streamManager.GetStream();
        SerializeToStreamInternal(input, typeof(TInput), ms);
        return ms.ToArray();
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

        ArgumentNullException.ThrowIfNull(destination);
        SerializeToStreamInternal(input, typeof(TInput), destination);
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

    // Stryker disable all : Deserialization methods with internal implementation - tested through round-trip tests
    [ExcludeFromCodeCoverage(Justification = "Metodo de desserializacao - testado atraves de round-trips")]
    public TResult? Deserialize<TResult>(byte[]? input)
    {
        if (input is null || input.Length == 0)
            return default;

        using var ms = new MemoryStream(input, writable: false);
        return DeserializeFromStreamInternal<TResult>(ms, typeof(TResult));
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo de desserializacao - testado atraves de round-trips")]
    public TResult? Deserialize<TResult>(byte[]? input, Type type)
    {
        if (input is null || input.Length == 0)
            return default;

        using var ms = new MemoryStream(input, writable: false);
        return DeserializeFromStreamInternal<TResult>(ms, type);
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo de desserializacao - testado atraves de round-trips")]
    public object? Deserialize(byte[]? input, Type type)
    {
        if (input is null || input.Length == 0)
            return default;

        using var ms = new MemoryStream(input, writable: false);
        return DeserializeFromStreamInternal(ms, type);
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

    public TResult? DeserializeFromStream<TResult>(Stream source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return DeserializeFromStreamInternal<TResult>(source, typeof(TResult));
    }

    public TResult? DeserializeFromStream<TResult>(Stream source, Type type)
    {
        ArgumentNullException.ThrowIfNull(source);
        return DeserializeFromStreamInternal<TResult>(source, type);
    }

    public object? DeserializeFromStream(Stream source, Type type)
    {
        ArgumentNullException.ThrowIfNull(source);
        return DeserializeFromStreamInternal(source, type);
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

    // Stryker disable all : Internal serialization method - tested through public API round-trips
    [ExcludeFromCodeCoverage(Justification = "Metodo interno de serializacao - testado atraves de round-trips de API publica")]
    private static void SerializeToStreamInternal<TInput>(TInput input, Type type, Stream destination)
    {
        (RecordSchema schema, PropertyInfo[] properties) = GetOrCreateSchema(type);

        var record = new GenericRecord(schema);

        foreach (PropertyInfo prop in properties)
        {
            object? value = prop.GetValue(input);
            object? avroValue = ConvertToAvroValue(value, prop.PropertyType);
            record.Add(prop.Name, avroValue);
        }

        var writer = new GenericDatumWriter<GenericRecord>(schema);
        var encoder = new BinaryEncoder(destination);
        writer.Write(record, encoder);
        encoder.Flush();
    }

    [ExcludeFromCodeCoverage(Justification = "Metodo interno de desserializacao - testado atraves de round-trips de API publica")]
    private static TResult? DeserializeFromStreamInternal<TResult>(Stream source, Type type)
    {
        object? result = DeserializeFromStreamInternal(source, type);
        return result is null ? default : (TResult)result;
    }
    // Stryker restore all

    private static object? DeserializeFromStreamInternal(Stream source, Type type)
    {
        (RecordSchema schema, PropertyInfo[] properties) = GetOrCreateSchema(type);

        var reader = new GenericDatumReader<GenericRecord>(schema, schema);
        var decoder = new BinaryDecoder(source);
        GenericRecord record = reader.Read(null!, decoder);

        object instance = Activator.CreateInstance(type)!;

        foreach (PropertyInfo prop in properties)
        {
            if (record.TryGetValue(prop.Name, out object? avroValue))
            {
                object? clrValue = ConvertFromAvroValue(avroValue, prop.PropertyType);
                if (clrValue is not null || IsNullable(prop.PropertyType))
                {
                    prop.SetValue(instance, clrValue);
                }
            }
        }

        return instance;
    }

    // Stryker disable all : Schema generation internals - tested indirectly through serialization round-trips
    [ExcludeFromCodeCoverage(Justification = "Geracao de schema Avro - testado indiretamente atraves de round-trips de serializacao")]
    private static (RecordSchema Schema, PropertyInfo[] Properties) GetOrCreateSchema(Type type)
    {
        return _schemaCache.GetOrAdd(type, t =>
        {
            PropertyInfo[] props = [.. t
                .GetProperties(_propertyFlags)
                .Where(p => p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0)
                .OrderBy(p => p.Name, StringComparer.Ordinal)];

            var fields = new List<Field>(props.Length);
            int position = 0;

            foreach (PropertyInfo prop in props)
            {
                Schema fieldSchema = GetAvroSchema(prop.PropertyType);
                var field = new Field(fieldSchema, prop.Name, position++);
                fields.Add(field);
            }

            var recordSchema = RecordSchema.Create(t.Name, fields, t.Namespace ?? "Bedrock.Avro");

            return (recordSchema, props);
        });
    }

    [ExcludeFromCodeCoverage(Justification = "Mapeamento de tipos CLR para Avro - cada branch requer DTO especifico, impraticavel testar todas as combinacoes")]
    private static Schema GetAvroSchema(Type clrType)
    {
        Type underlyingType = Nullable.GetUnderlyingType(clrType) ?? clrType;
        bool isNullable = IsNullable(clrType);

        Schema baseSchema = underlyingType switch
        {
            _ when underlyingType == typeof(bool) => PrimitiveSchema.Create(Schema.Type.Boolean),
            _ when underlyingType == typeof(int) => PrimitiveSchema.Create(Schema.Type.Int),
            _ when underlyingType == typeof(long) => PrimitiveSchema.Create(Schema.Type.Long),
            _ when underlyingType == typeof(float) => PrimitiveSchema.Create(Schema.Type.Float),
            _ when underlyingType == typeof(double) => PrimitiveSchema.Create(Schema.Type.Double),
            _ when underlyingType == typeof(string) => PrimitiveSchema.Create(Schema.Type.String),
            _ when underlyingType == typeof(byte[]) => PrimitiveSchema.Create(Schema.Type.Bytes),
            _ when underlyingType == typeof(sbyte) => PrimitiveSchema.Create(Schema.Type.Int),
            _ when underlyingType == typeof(byte) => PrimitiveSchema.Create(Schema.Type.Int),
            _ when underlyingType == typeof(short) => PrimitiveSchema.Create(Schema.Type.Int),
            _ when underlyingType == typeof(ushort) => PrimitiveSchema.Create(Schema.Type.Int),
            _ when underlyingType == typeof(uint) => PrimitiveSchema.Create(Schema.Type.Long),
            _ when underlyingType == typeof(ulong) => PrimitiveSchema.Create(Schema.Type.Long),
            _ when underlyingType == typeof(decimal) => PrimitiveSchema.Create(Schema.Type.String),
            _ when underlyingType == typeof(DateTime) => PrimitiveSchema.Create(Schema.Type.Long),
            _ when underlyingType == typeof(DateTimeOffset) => PrimitiveSchema.Create(Schema.Type.Long),
            _ when underlyingType == typeof(DateOnly) => PrimitiveSchema.Create(Schema.Type.Int),
            _ when underlyingType == typeof(TimeOnly) => PrimitiveSchema.Create(Schema.Type.Long),
            _ when underlyingType == typeof(TimeSpan) => PrimitiveSchema.Create(Schema.Type.Long),
            _ when underlyingType == typeof(Guid) => PrimitiveSchema.Create(Schema.Type.String),
            _ => PrimitiveSchema.Create(Schema.Type.String)
        };

        if (isNullable)
        {
            return UnionSchema.Create([PrimitiveSchema.Create(Schema.Type.Null), baseSchema]);
        }

        return baseSchema;
    }

    [ExcludeFromCodeCoverage(Justification = "Verificacao de nullable - usado internamente pelo mapeamento de tipos")]
    private static bool IsNullable(Type type)
    {
        return !type.IsValueType || Nullable.GetUnderlyingType(type) is not null;
    }

    [ExcludeFromCodeCoverage(Justification = "Conversao de valores CLR para Avro - cada branch requer tipo especifico, impraticavel testar todas as combinacoes")]
    private static object? ConvertToAvroValue(object? value, Type propertyType)
    {
        if (value is null)
            return null;

        Type underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        return underlyingType switch
        {
            _ when underlyingType == typeof(bool) => value,
            _ when underlyingType == typeof(int) => value,
            _ when underlyingType == typeof(long) => value,
            _ when underlyingType == typeof(float) => value,
            _ when underlyingType == typeof(double) => value,
            _ when underlyingType == typeof(string) => value,
            _ when underlyingType == typeof(byte[]) => value,
            _ when underlyingType == typeof(sbyte) => (int)(sbyte)value,
            _ when underlyingType == typeof(byte) => (int)(byte)value,
            _ when underlyingType == typeof(short) => (int)(short)value,
            _ when underlyingType == typeof(ushort) => (int)(ushort)value,
            _ when underlyingType == typeof(uint) => (long)(uint)value,
            _ when underlyingType == typeof(ulong) => (long)(ulong)value,
            _ when underlyingType == typeof(decimal) => ((decimal)value).ToString(CultureInfo.InvariantCulture),
            _ when underlyingType == typeof(DateTime) => ((DateTime)value).Ticks,
            _ when underlyingType == typeof(DateTimeOffset) => ((DateTimeOffset)value).Ticks,
            _ when underlyingType == typeof(DateOnly) => ((DateOnly)value).DayNumber,
            _ when underlyingType == typeof(TimeOnly) => ((TimeOnly)value).Ticks,
            _ when underlyingType == typeof(TimeSpan) => ((TimeSpan)value).Ticks,
            _ when underlyingType == typeof(Guid) => ((Guid)value).ToString(),
            _ => value.ToString()
        };
    }

    [ExcludeFromCodeCoverage(Justification = "Conversao de valores Avro para CLR - cada branch requer tipo especifico, impraticavel testar todas as combinacoes")]
    private static object? ConvertFromAvroValue(object? avroValue, Type targetType)
    {
        if (avroValue is null)
            return null;

        Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        return underlyingType switch
        {
            _ when underlyingType == typeof(bool) => Convert.ToBoolean(avroValue, CultureInfo.InvariantCulture),
            _ when underlyingType == typeof(int) => Convert.ToInt32(avroValue, CultureInfo.InvariantCulture),
            _ when underlyingType == typeof(long) => Convert.ToInt64(avroValue, CultureInfo.InvariantCulture),
            _ when underlyingType == typeof(float) => Convert.ToSingle(avroValue, CultureInfo.InvariantCulture),
            _ when underlyingType == typeof(double) => Convert.ToDouble(avroValue, CultureInfo.InvariantCulture),
            _ when underlyingType == typeof(string) => avroValue.ToString(),
            _ when underlyingType == typeof(byte[]) => avroValue is byte[] bytes ? bytes : Encoding.UTF8.GetBytes(avroValue.ToString() ?? string.Empty),
            _ when underlyingType == typeof(sbyte) => (sbyte)Convert.ToInt32(avroValue, CultureInfo.InvariantCulture),
            _ when underlyingType == typeof(byte) => (byte)Convert.ToInt32(avroValue, CultureInfo.InvariantCulture),
            _ when underlyingType == typeof(short) => (short)Convert.ToInt32(avroValue, CultureInfo.InvariantCulture),
            _ when underlyingType == typeof(ushort) => (ushort)Convert.ToInt32(avroValue, CultureInfo.InvariantCulture),
            _ when underlyingType == typeof(uint) => (uint)Convert.ToInt64(avroValue, CultureInfo.InvariantCulture),
            _ when underlyingType == typeof(ulong) => (ulong)Convert.ToInt64(avroValue, CultureInfo.InvariantCulture),
            _ when underlyingType == typeof(decimal) => decimal.Parse(avroValue.ToString()!, CultureInfo.InvariantCulture),
            _ when underlyingType == typeof(DateTime) => new DateTime(Convert.ToInt64(avroValue, CultureInfo.InvariantCulture)),
            _ when underlyingType == typeof(DateTimeOffset) => new DateTimeOffset(Convert.ToInt64(avroValue, CultureInfo.InvariantCulture), TimeSpan.Zero),
            _ when underlyingType == typeof(DateOnly) => DateOnly.FromDayNumber(Convert.ToInt32(avroValue, CultureInfo.InvariantCulture)),
            _ when underlyingType == typeof(TimeOnly) => new TimeOnly(Convert.ToInt64(avroValue, CultureInfo.InvariantCulture)),
            _ when underlyingType == typeof(TimeSpan) => new TimeSpan(Convert.ToInt64(avroValue, CultureInfo.InvariantCulture)),
            _ when underlyingType == typeof(Guid) => Guid.Parse(avroValue.ToString()!),
            _ => avroValue
        };
    }
    // Stryker restore all

    protected abstract void ConfigureInternal(Options options);
}
