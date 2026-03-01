using Bedrock.BuildingBlocks.Messages;
using Bedrock.BuildingBlocks.Messages.Events;
using Bedrock.BuildingBlocks.Outbox.Messages;
using Bedrock.BuildingBlocks.Serialization.Abstractions.Interfaces;
using Bedrock.BuildingBlocks.Testing;
using Moq;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Outbox.Messages;

public class MessageOutboxSerializerTests : TestBase
{
    private readonly Mock<IStringSerializer> _serializerMock;
    private readonly MessageOutboxSerializer _sut;

    public MessageOutboxSerializerTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _serializerMock = new Mock<IStringSerializer>();
        _sut = new MessageOutboxSerializer(_serializerMock.Object);
    }

    [Fact]
    public void ContentType_ShouldReturnApplicationJson()
    {
        // Arrange
        LogArrange("Accessing ContentType property");

        // Act
        LogAct("Reading ContentType");
        var contentType = _sut.ContentType;

        // Assert
        LogAssert("Verifying content type is application/json");
        contentType.ShouldBe("application/json");
    }

    [Fact]
    public void Serialize_WithConcreteMessage_ShouldCallSerializerWithConcreteType()
    {
        // Arrange
        LogArrange("Creating a concrete message and configuring serializer mock");
        var metadata = CreateTestMetadata();
        var message = new TestEvent(metadata, "test-value");
        var expectedBytes = new byte[] { 0x7B, 0x7D }; // {}

        _serializerMock
            .Setup(static x => x.SerializeToUtf8Bytes(It.IsAny<MessageBase>(), typeof(TestEvent)))
            .Returns(expectedBytes);

        // Act
        LogAct("Serializing the message");
        var result = _sut.Serialize(message);

        // Assert
        LogAssert("Verifying serializer was called with concrete type and returned expected bytes");
        result.ShouldBe(expectedBytes);
        _serializerMock.Verify(
            static x => x.SerializeToUtf8Bytes(It.IsAny<MessageBase>(), typeof(TestEvent)),
            Times.Once);
    }

    [Fact]
    public void Serialize_WhenSerializerReturnsNull_ShouldThrowInvalidOperationException()
    {
        // Arrange
        LogArrange("Configuring serializer to return null");
        var metadata = CreateTestMetadata();
        var message = new TestEvent(metadata, "test-value");

        _serializerMock
            .Setup(static x => x.SerializeToUtf8Bytes(It.IsAny<MessageBase>(), It.IsAny<Type>()))
            .Returns((byte[]?)null);

        // Act
        LogAct("Serializing message that will produce null");
        var exception = Should.Throw<InvalidOperationException>(
            () => _sut.Serialize(message));

        // Assert
        LogAssert("Verifying exception message contains type name");
        exception.Message.ShouldContain(nameof(TestEvent));
    }

    [Fact]
    public void Serialize_ShouldPassPayloadToSerializer()
    {
        // Arrange
        LogArrange("Creating message and capturing serializer input");
        var metadata = CreateTestMetadata();
        var message = new TestEvent(metadata, "captured-value");
        MessageBase? capturedPayload = null;

        _serializerMock
            .Setup(static x => x.SerializeToUtf8Bytes(It.IsAny<MessageBase>(), It.IsAny<Type>()))
            .Callback<object?, Type>((payload, _) => capturedPayload = (MessageBase?)payload)
            .Returns([0x01]);

        // Act
        LogAct("Serializing message");
        _sut.Serialize(message);

        // Assert
        LogAssert("Verifying the exact message instance was passed to the serializer");
        capturedPayload.ShouldBeSameAs(message);
    }

    private static MessageMetadata CreateTestMetadata() => new(
        MessageId: Guid.NewGuid(),
        Timestamp: DateTimeOffset.UtcNow,
        SchemaName: "test-schema",
        CorrelationId: Guid.NewGuid(),
        TenantCode: Guid.NewGuid(),
        ExecutionUser: "test-user",
        ExecutionOrigin: "unit-test",
        BusinessOperationCode: "TEST-OP");

    private sealed record TestEvent(MessageMetadata Metadata, string Value)
        : EventBase(Metadata);
}
