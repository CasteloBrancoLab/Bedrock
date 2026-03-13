using Bedrock.BuildingBlocks.Messages;
using Bedrock.BuildingBlocks.Outbox.Interfaces;
using Bedrock.BuildingBlocks.Outbox.Models;
using Bedrock.BuildingBlocks.Testing;
using Moq;
using ShopDemo.Auth.Infra.Data.PostgreSql.Outbox;
using ShopDemo.Auth.Infra.Data.PostgreSql.Outbox.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Outbox;

public class AuthOutboxPostgreSqlWriterTests : TestBase
{
    private readonly Mock<IAuthOutboxPostgreSqlRepository> _repositoryMock;
    private readonly Mock<IOutboxSerializer<MessageBase>> _serializerMock;
    private readonly AuthOutboxPostgreSqlWriter _sut;

    public AuthOutboxPostgreSqlWriterTests(ITestOutputHelper output) : base(output)
    {
        _repositoryMock = new Mock<IAuthOutboxPostgreSqlRepository>();
        _serializerMock = new Mock<IOutboxSerializer<MessageBase>>();

        _serializerMock.Setup(x => x.ContentType).Returns("application/json");
        _serializerMock.Setup(x => x.Serialize(It.IsAny<MessageBase>())).Returns([0x01, 0x02]);

        _sut = new AuthOutboxPostgreSqlWriter(_repositoryMock.Object, _serializerMock.Object, TimeProvider.System);
    }

    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        LogAssert("Verifying AuthOutboxPostgreSqlWriter was created");
        _sut.ShouldNotBeNull();
    }

    [Fact]
    public async Task EnqueueAsync_ShouldDelegateToRepository()
    {
        // Arrange
        LogArrange("Setting up a test message");
        var message = new TestMessage(new MessageMetadata(
            Guid.NewGuid(), DateTimeOffset.UtcNow, string.Empty,
            Guid.NewGuid(), Guid.NewGuid(), "test.user", "UnitTest", "TEST_OP"));

        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<OutboxEntry>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        LogAct("Calling EnqueueAsync");
        await _sut.EnqueueAsync(message, CancellationToken.None);

        // Assert
        LogAssert("Verifying repository AddAsync was called once");
        _repositoryMock.Verify(
            x => x.AddAsync(It.IsAny<OutboxEntry>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private sealed record TestMessage(MessageMetadata Metadata) : MessageBase(Metadata);
}
