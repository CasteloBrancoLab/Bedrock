using Bedrock.BuildingBlocks.Serialization.Abstractions;
using Bedrock.BuildingBlocks.Serialization.Avro.Models;

namespace Bedrock.BuildingBlocks.Serialization.Avro.Interfaces;

public interface IAvroSerializer
    : IByteArraySerializer
{
    public Options Options { get; }

    public string GenerateSchemaDefinition<T>();
    public string GenerateSchemaDefinition(Type type);
}
