using Bedrock.BuildingBlocks.Outbox.Messages;
using Bedrock.BuildingBlocks.Serialization.Abstractions.Interfaces;
using Bedrock.BuildingBlocks.Testing;
using Moq;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Outbox.Messages;

public class MessageOutboxDeserializerTests : TestBase
{
    private readonly Mock<IStringSerializer> _serializerMock;
    private readonly MessageOutboxDeserializer _sut;

    public MessageOutboxDeserializerTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _serializerMock = new Mock<IStringSerializer>();
        _sut = new MessageOutboxDeserializer(_serializerMock.Object);
    }

    [Fact]
    public void Deserialize_WithValidPayloadType_ShouldResolveTypeAndDeserialize()
    {
        // Arrange
        LogArrange("Setting up deserializer with a known type name");
        var data = new byte[] { 0x7B, 0x7D };
        var payloadType = typeof(DeserializerTestDto).AssemblyQualifiedName!;
        var expected = new DeserializerTestDto("test-value");

        _serializerMock
            .Setup(static x => x.DeserializeFromUtf8Bytes<object>(It.IsAny<byte[]>(), typeof(DeserializerTestDto)))
            .Returns(expected);

        // Act
        LogAct("Deserializing data with valid payload type");
        var result = _sut.Deserialize(data, payloadType, "application/json");

        // Assert
        LogAssert("Verifying deserialized object matches expected");
        result.ShouldBe(expected);
        _serializerMock.Verify(
            static x => x.DeserializeFromUtf8Bytes<object>(It.IsAny<byte[]>(), typeof(DeserializerTestDto)),
            Times.Once);
    }

    [Fact]
    public void Deserialize_WithUnresolvableType_ShouldThrowInvalidOperationException()
    {
        // Arrange
        LogArrange("Setting up with an unresolvable type name");
        var data = new byte[] { 0x01 };
        var invalidType = "NonExistent.Namespace.FakeType, NonExistent.Assembly";

        // Act
        LogAct("Attempting to deserialize with invalid payload type");
        var exception = Should.Throw<InvalidOperationException>(
            () => _sut.Deserialize(data, invalidType, "application/json"));

        // Assert
        LogAssert("Verifying exception message contains the unresolvable type name");
        exception.Message.ShouldContain(invalidType);
        exception.Message.ShouldContain("Cannot resolve type");
    }

    [Fact]
    public void Deserialize_ShouldPassDataToSerializer()
    {
        // Arrange
        LogArrange("Setting up to capture data passed to serializer");
        var data = new byte[] { 0x01, 0x02, 0x03 };
        var payloadType = typeof(DeserializerTestDto).AssemblyQualifiedName!;
        byte[]? capturedData = null;

        _serializerMock
            .Setup(static x => x.DeserializeFromUtf8Bytes<object>(It.IsAny<byte[]>(), It.IsAny<Type>()))
            .Callback<byte[]?, Type>((d, _) => capturedData = d)
            .Returns(new DeserializerTestDto("value"));

        // Act
        LogAct("Deserializing with specific data bytes");
        _sut.Deserialize(data, payloadType, "application/json");

        // Assert
        LogAssert("Verifying exact data bytes were passed to the serializer");
        capturedData.ShouldBe(data);
    }

    [Fact]
    public void Deserialize_WhenSerializerReturnsNull_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up serializer to return null");
        var data = new byte[] { 0x01 };
        var payloadType = typeof(DeserializerTestDto).AssemblyQualifiedName!;

        _serializerMock
            .Setup(static x => x.DeserializeFromUtf8Bytes<object>(It.IsAny<byte[]>(), It.IsAny<Type>()))
            .Returns((object?)null);

        // Act
        LogAct("Deserializing when serializer returns null");
        var result = _sut.Deserialize(data, payloadType, "application/json");

        // Assert
        LogAssert("Verifying null is returned");
        result.ShouldBeNull();
    }

    public sealed record DeserializerTestDto(string Value);
}
