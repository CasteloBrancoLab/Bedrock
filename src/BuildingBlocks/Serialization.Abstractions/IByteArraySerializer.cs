namespace Bedrock.BuildingBlocks.Serialization.Abstractions;

/// <summary>
/// Interface for binary serializers that output byte arrays (e.g., Avro, Protobuf, Parquet).
/// </summary>
public interface IByteArraySerializer : ISerializer<byte[]>
{
}
