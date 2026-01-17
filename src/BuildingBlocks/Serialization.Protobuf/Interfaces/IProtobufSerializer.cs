using Bedrock.BuildingBlocks.Serialization.Abstractions;
using Bedrock.BuildingBlocks.Serialization.Protobuf.Models;

namespace Bedrock.BuildingBlocks.Serialization.Protobuf.Interfaces;

public interface IProtobufSerializer
    : IByteArraySerializer
{
    public Options Options { get; }
    public string GenerateProtoFileContent(string package);
}
