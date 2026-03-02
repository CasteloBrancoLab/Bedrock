using Bedrock.BuildingBlocks.Outbox.Models;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Outbox.Models;

public class OutboxEntryTests : TestBase
{
    public OutboxEntryTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void Constructor_WithAllRequiredProperties_ShouldCreateInstance()
    {
        // Arrange
        LogArrange("Creating OutboxEntry with all required properties");
        var id = Guid.NewGuid();
        var tenantCode = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var payload = new byte[] { 0x01, 0x02, 0x03 };
        var createdAt = DateTimeOffset.UtcNow;

        // Act
        LogAct("Constructing OutboxEntry");
        var entry = new OutboxEntry
        {
            Id = id,
            TenantCode = tenantCode,
            CorrelationId = correlationId,
            PayloadType = "TestType",
            ContentType = "application/json",
            Payload = payload,
            CreatedAt = createdAt,
            Status = OutboxEntryStatus.Pending,
            ProcessedAt = null,
            RetryCount = 0,
            IsProcessing = false,
            ProcessingExpiration = null
        };

        // Assert
        LogAssert("Verifying all properties are set correctly");
        entry.Id.ShouldBe(id);
        entry.TenantCode.ShouldBe(tenantCode);
        entry.CorrelationId.ShouldBe(correlationId);
        entry.PayloadType.ShouldBe("TestType");
        entry.ContentType.ShouldBe("application/json");
        entry.Payload.ShouldBe(payload);
        entry.CreatedAt.ShouldBe(createdAt);
        entry.Status.ShouldBe(OutboxEntryStatus.Pending);
        entry.ProcessedAt.ShouldBeNull();
        entry.RetryCount.ShouldBe((byte)0);
        entry.IsProcessing.ShouldBeFalse();
        entry.ProcessingExpiration.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithLeaseProperties_ShouldSetProcessingFields()
    {
        // Arrange
        LogArrange("Creating OutboxEntry with lease (processing) properties");
        var expiration = DateTimeOffset.UtcNow.AddMinutes(5);

        // Act
        LogAct("Constructing OutboxEntry with IsProcessing=true and expiration");
        var entry = new OutboxEntry
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid(),
            PayloadType = "TestType",
            ContentType = "application/json",
            Payload = [0x01],
            CreatedAt = DateTimeOffset.UtcNow,
            Status = OutboxEntryStatus.Processing,
            IsProcessing = true,
            ProcessingExpiration = expiration
        };

        // Assert
        LogAssert("Verifying lease properties are set");
        entry.IsProcessing.ShouldBeTrue();
        entry.ProcessingExpiration.ShouldBe(expiration);
        entry.Status.ShouldBe(OutboxEntryStatus.Processing);
    }

    [Fact]
    public void WithExpression_ShouldCreateModifiedCopy()
    {
        // Arrange
        LogArrange("Creating original OutboxEntry");
        var original = new OutboxEntry
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid(),
            PayloadType = "TestType",
            ContentType = "application/json",
            Payload = [0x01],
            CreatedAt = DateTimeOffset.UtcNow,
            Status = OutboxEntryStatus.Pending,
            RetryCount = 0,
            IsProcessing = false,
            ProcessingExpiration = null
        };

        // Act
        LogAct("Creating copy with modified status and retry count");
        var modified = original with
        {
            Status = OutboxEntryStatus.Failed,
            RetryCount = 3,
            ProcessedAt = DateTimeOffset.UtcNow
        };

        // Assert
        LogAssert("Verifying original is unchanged and copy has new values");
        original.Status.ShouldBe(OutboxEntryStatus.Pending);
        original.RetryCount.ShouldBe((byte)0);
        modified.Status.ShouldBe(OutboxEntryStatus.Failed);
        modified.RetryCount.ShouldBe((byte)3);
        modified.ProcessedAt.ShouldNotBeNull();
        modified.Id.ShouldBe(original.Id);
    }

    [Fact]
    public void Equality_SameValues_ShouldBeEqual()
    {
        // Arrange
        LogArrange("Creating two OutboxEntry instances with identical values");
        var id = Guid.NewGuid();
        var tenantCode = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var payload = new byte[] { 0x01, 0x02 };
        var createdAt = DateTimeOffset.UtcNow;

        var entry1 = new OutboxEntry
        {
            Id = id,
            TenantCode = tenantCode,
            CorrelationId = correlationId,
            PayloadType = "TestType",
            ContentType = "application/json",
            Payload = payload,
            CreatedAt = createdAt,
            Status = OutboxEntryStatus.Pending
        };

        var entry2 = new OutboxEntry
        {
            Id = id,
            TenantCode = tenantCode,
            CorrelationId = correlationId,
            PayloadType = "TestType",
            ContentType = "application/json",
            Payload = payload,
            CreatedAt = createdAt,
            Status = OutboxEntryStatus.Pending
        };

        // Act & Assert
        LogAct("Comparing entries");
        LogAssert("Verifying record equality");
        entry1.ShouldBe(entry2);
    }
}
