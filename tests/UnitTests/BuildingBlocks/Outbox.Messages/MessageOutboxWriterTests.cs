using Bedrock.BuildingBlocks.Core.TimeProviders;
using Bedrock.BuildingBlocks.Messages;
using Bedrock.BuildingBlocks.Messages.Events;
using Bedrock.BuildingBlocks.Outbox.Interfaces;
using Bedrock.BuildingBlocks.Outbox.Messages;
using Bedrock.BuildingBlocks.Outbox.Models;
using Bedrock.BuildingBlocks.Testing;
using Moq;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Outbox.Messages;

public class MessageOutboxWriterTests : TestBase
{
    private static readonly DateTimeOffset FixedTime = new(2026, 2, 28, 12, 0, 0, TimeSpan.Zero);

    private readonly Mock<IOutboxRepository> _repositoryMock;
    private readonly Mock<IOutboxSerializer<MessageBase>> _serializerMock;
    private readonly CustomTimeProvider _timeProvider;
    private readonly MessageOutboxWriter _sut;

    public MessageOutboxWriterTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _repositoryMock = new Mock<IOutboxRepository>();
        _serializerMock = new Mock<IOutboxSerializer<MessageBase>>();
        _timeProvider = new CustomTimeProvider(utcNowFunc: _ => FixedTime, localTimeZone: null);

        _serializerMock.Setup(static x => x.ContentType).Returns("application/json");

        _sut = new MessageOutboxWriter(
            _repositoryMock.Object,
            _serializerMock.Object,
            _timeProvider);
    }

    [Fact]
    public async Task EnqueueAsync_ShouldCreateEntryWithMetadataFromMessage()
    {
        // Arrange
        LogArrange("Creating message with specific metadata");
        var tenantCode = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var metadata = CreateTestMetadata(tenantCode: tenantCode, correlationId: correlationId);
        var message = new TestEvent(metadata, "value");
        OutboxEntry? capturedEntry = null;

        _serializerMock
            .Setup(static x => x.Serialize(It.IsAny<MessageBase>()))
            .Returns([0x01]);

        _repositoryMock
            .Setup(static x => x.AddAsync(It.IsAny<OutboxEntry>(), It.IsAny<CancellationToken>()))
            .Callback<OutboxEntry, CancellationToken>((entry, _) => capturedEntry = entry)
            .Returns(Task.CompletedTask);

        // Act
        LogAct("Enqueuing message");
        await _sut.EnqueueAsync(message, CancellationToken.None);

        // Assert
        LogAssert("Verifying entry has correct metadata from message");
        capturedEntry.ShouldNotBeNull();
        capturedEntry.TenantCode.ShouldBe(tenantCode);
        capturedEntry.CorrelationId.ShouldBe(correlationId);
    }

    [Fact]
    public async Task EnqueueAsync_ShouldSetPayloadTypeFromSchemaName()
    {
        // Arrange
        LogArrange("Creating message — SchemaName is set by MessageBase constructor");
        var metadata = CreateTestMetadata();
        var message = new TestEvent(metadata, "value");
        OutboxEntry? capturedEntry = null;

        _serializerMock
            .Setup(static x => x.Serialize(It.IsAny<MessageBase>()))
            .Returns([0x01]);

        _repositoryMock
            .Setup(static x => x.AddAsync(It.IsAny<OutboxEntry>(), It.IsAny<CancellationToken>()))
            .Callback<OutboxEntry, CancellationToken>((entry, _) => capturedEntry = entry)
            .Returns(Task.CompletedTask);

        // Act
        LogAct("Enqueuing message");
        await _sut.EnqueueAsync(message, CancellationToken.None);

        // Assert
        LogAssert("Verifying PayloadType equals message SchemaName (concrete type full name)");
        capturedEntry.ShouldNotBeNull();
        capturedEntry.PayloadType.ShouldBe(message.Metadata.SchemaName);
    }

    [Fact]
    public async Task EnqueueAsync_ShouldSetContentTypeFromSerializer()
    {
        // Arrange
        LogArrange("Configuring serializer with custom content type");
        var metadata = CreateTestMetadata();
        var message = new TestEvent(metadata, "value");
        OutboxEntry? capturedEntry = null;

        _serializerMock
            .Setup(static x => x.Serialize(It.IsAny<MessageBase>()))
            .Returns([0x01]);

        _repositoryMock
            .Setup(static x => x.AddAsync(It.IsAny<OutboxEntry>(), It.IsAny<CancellationToken>()))
            .Callback<OutboxEntry, CancellationToken>((entry, _) => capturedEntry = entry)
            .Returns(Task.CompletedTask);

        // Act
        LogAct("Enqueuing message");
        await _sut.EnqueueAsync(message, CancellationToken.None);

        // Assert
        LogAssert("Verifying ContentType comes from serializer");
        capturedEntry.ShouldNotBeNull();
        capturedEntry.ContentType.ShouldBe("application/json");
    }

    [Fact]
    public async Task EnqueueAsync_ShouldSerializePayloadViaSerializer()
    {
        // Arrange
        LogArrange("Setting up serializer to return specific bytes");
        var metadata = CreateTestMetadata();
        var message = new TestEvent(metadata, "value");
        var expectedPayload = new byte[] { 0x01, 0x02, 0x03 };
        OutboxEntry? capturedEntry = null;

        _serializerMock
            .Setup(static x => x.Serialize(It.IsAny<MessageBase>()))
            .Returns(expectedPayload);

        _repositoryMock
            .Setup(static x => x.AddAsync(It.IsAny<OutboxEntry>(), It.IsAny<CancellationToken>()))
            .Callback<OutboxEntry, CancellationToken>((entry, _) => capturedEntry = entry)
            .Returns(Task.CompletedTask);

        // Act
        LogAct("Enqueuing message");
        await _sut.EnqueueAsync(message, CancellationToken.None);

        // Assert
        LogAssert("Verifying entry payload is the serialized bytes");
        capturedEntry.ShouldNotBeNull();
        capturedEntry.Payload.ShouldBe(expectedPayload);
    }

    [Fact]
    public async Task EnqueueAsync_ShouldSetInitialStatusToPending()
    {
        // Arrange
        LogArrange("Creating message for enqueue");
        var metadata = CreateTestMetadata();
        var message = new TestEvent(metadata, "value");
        OutboxEntry? capturedEntry = null;

        _serializerMock
            .Setup(static x => x.Serialize(It.IsAny<MessageBase>()))
            .Returns([0x01]);

        _repositoryMock
            .Setup(static x => x.AddAsync(It.IsAny<OutboxEntry>(), It.IsAny<CancellationToken>()))
            .Callback<OutboxEntry, CancellationToken>((entry, _) => capturedEntry = entry)
            .Returns(Task.CompletedTask);

        // Act
        LogAct("Enqueuing message");
        await _sut.EnqueueAsync(message, CancellationToken.None);

        // Assert
        LogAssert("Verifying initial state: Pending, RetryCount=0, no processing");
        capturedEntry.ShouldNotBeNull();
        capturedEntry.Status.ShouldBe(OutboxEntryStatus.Pending);
        capturedEntry.RetryCount.ShouldBe((byte)0);
        capturedEntry.IsProcessing.ShouldBeFalse();
        capturedEntry.ProcessingExpiration.ShouldBeNull();
        capturedEntry.ProcessedAt.ShouldBeNull();
    }

    [Fact]
    public async Task EnqueueAsync_ShouldSetCreatedAtFromTimeProvider()
    {
        // Arrange
        LogArrange("Setting up FakeTimeProvider at fixed time");
        var metadata = CreateTestMetadata();
        var message = new TestEvent(metadata, "value");
        OutboxEntry? capturedEntry = null;

        _serializerMock
            .Setup(static x => x.Serialize(It.IsAny<MessageBase>()))
            .Returns([0x01]);

        _repositoryMock
            .Setup(static x => x.AddAsync(It.IsAny<OutboxEntry>(), It.IsAny<CancellationToken>()))
            .Callback<OutboxEntry, CancellationToken>((entry, _) => capturedEntry = entry)
            .Returns(Task.CompletedTask);

        // Act
        LogAct("Enqueuing message");
        await _sut.EnqueueAsync(message, CancellationToken.None);

        // Assert
        LogAssert("Verifying CreatedAt matches TimeProvider timestamp");
        capturedEntry.ShouldNotBeNull();
        capturedEntry.CreatedAt.ShouldBe(_timeProvider.GetUtcNow());
    }

    [Fact]
    public async Task EnqueueAsync_ShouldGenerateNonEmptyId()
    {
        // Arrange
        LogArrange("Creating message for enqueue");
        var metadata = CreateTestMetadata();
        var message = new TestEvent(metadata, "value");
        OutboxEntry? capturedEntry = null;

        _serializerMock
            .Setup(static x => x.Serialize(It.IsAny<MessageBase>()))
            .Returns([0x01]);

        _repositoryMock
            .Setup(static x => x.AddAsync(It.IsAny<OutboxEntry>(), It.IsAny<CancellationToken>()))
            .Callback<OutboxEntry, CancellationToken>((entry, _) => capturedEntry = entry)
            .Returns(Task.CompletedTask);

        // Act
        LogAct("Enqueuing message");
        await _sut.EnqueueAsync(message, CancellationToken.None);

        // Assert
        LogAssert("Verifying Id is a non-empty Guid (UUIDv7)");
        capturedEntry.ShouldNotBeNull();
        capturedEntry.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task EnqueueAsync_ShouldCallRepositoryAddAsync()
    {
        // Arrange
        LogArrange("Setting up repository to track calls");
        var metadata = CreateTestMetadata();
        var message = new TestEvent(metadata, "value");

        _serializerMock
            .Setup(static x => x.Serialize(It.IsAny<MessageBase>()))
            .Returns([0x01]);

        _repositoryMock
            .Setup(static x => x.AddAsync(It.IsAny<OutboxEntry>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        LogAct("Enqueuing message");
        await _sut.EnqueueAsync(message, CancellationToken.None);

        // Assert
        LogAssert("Verifying repository.AddAsync was called exactly once");
        _repositoryMock.Verify(
            static x => x.AddAsync(It.IsAny<OutboxEntry>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EnqueueAsync_ShouldPassCancellationTokenToRepository()
    {
        // Arrange
        LogArrange("Creating CancellationToken to track propagation");
        var metadata = CreateTestMetadata();
        var message = new TestEvent(metadata, "value");
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        CancellationToken capturedToken = default;

        _serializerMock
            .Setup(static x => x.Serialize(It.IsAny<MessageBase>()))
            .Returns([0x01]);

        _repositoryMock
            .Setup(static x => x.AddAsync(It.IsAny<OutboxEntry>(), It.IsAny<CancellationToken>()))
            .Callback<OutboxEntry, CancellationToken>((_, ct) => capturedToken = ct)
            .Returns(Task.CompletedTask);

        // Act
        LogAct("Enqueuing message with specific CancellationToken");
        await _sut.EnqueueAsync(message, token);

        // Assert
        LogAssert("Verifying CancellationToken was propagated to repository");
        capturedToken.ShouldBe(token);
    }

    private static MessageMetadata CreateTestMetadata(
        Guid? tenantCode = null,
        Guid? correlationId = null) => new(
        MessageId: Guid.NewGuid(),
        Timestamp: DateTimeOffset.UtcNow,
        SchemaName: "test-schema",
        CorrelationId: correlationId ?? Guid.NewGuid(),
        TenantCode: tenantCode ?? Guid.NewGuid(),
        ExecutionUser: "test-user",
        ExecutionOrigin: "unit-test",
        BusinessOperationCode: "TEST-OP");

    private sealed record TestEvent(MessageMetadata Metadata, string Value)
        : EventBase(Metadata);
}
