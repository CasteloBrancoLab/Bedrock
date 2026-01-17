using Bedrock.BuildingBlocks.Serialization.Abstractions;
using Bedrock.BuildingBlocks.Serialization.Parquet.Models;

namespace Bedrock.BuildingBlocks.Serialization.Parquet.Interfaces;

public interface IParquetSerializer
    : IByteArraySerializer
{
    public Options Options { get; }

    public string GenerateSchemaDefinition<T>();
    public string GenerateSchemaDefinition(Type type);

    public byte[]? SerializeCollection<TInput>(IEnumerable<TInput>? input);
    public byte[]? SerializeCollection<TInput>(IEnumerable<TInput>? input, Type type);
    public Task<byte[]?> SerializeCollectionAsync<TInput>(IEnumerable<TInput>? input, CancellationToken cancellationToken);
    public Task<byte[]?> SerializeCollectionAsync<TInput>(IEnumerable<TInput>? input, Type type, CancellationToken cancellationToken);

    public void SerializeCollectionToStream<TInput>(IEnumerable<TInput>? input, Stream destination);
    public void SerializeCollectionToStream<TInput>(IEnumerable<TInput>? input, Type type, Stream destination);
    public Task SerializeCollectionToStreamAsync<TInput>(IEnumerable<TInput>? input, Stream destination, CancellationToken cancellationToken);
    public Task SerializeCollectionToStreamAsync<TInput>(IEnumerable<TInput>? input, Type type, Stream destination, CancellationToken cancellationToken);

    public IEnumerable<TResult>? DeserializeCollection<TResult>(byte[]? input);
    public IEnumerable<TResult>? DeserializeCollection<TResult>(byte[]? input, Type type);
    public Task<IEnumerable<TResult>?> DeserializeCollectionAsync<TResult>(byte[]? input, CancellationToken cancellationToken);
    public Task<IEnumerable<TResult>?> DeserializeCollectionAsync<TResult>(byte[]? input, Type type, CancellationToken cancellationToken);

    public IEnumerable<TResult>? DeserializeCollectionFromStream<TResult>(Stream source);
    public IEnumerable<TResult>? DeserializeCollectionFromStream<TResult>(Stream source, Type type);
    public Task<IEnumerable<TResult>?> DeserializeCollectionFromStreamAsync<TResult>(Stream source, CancellationToken cancellationToken);
    public Task<IEnumerable<TResult>?> DeserializeCollectionFromStreamAsync<TResult>(Stream source, Type type, CancellationToken cancellationToken);
}
