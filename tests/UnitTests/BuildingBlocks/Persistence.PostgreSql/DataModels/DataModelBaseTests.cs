using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Persistence.PostgreSql.DataModels;

public class DataModelBaseTests : TestBase
{
    public DataModelBaseTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Id_ShouldBeSettableAndGettable()
    {
        // Arrange
        LogArrange("Creating DataModelBase instance");
        DataModelBase model = new();
        Guid expectedId = Guid.NewGuid();

        // Act
        LogAct("Setting Id property");
        model.Id = expectedId;

        // Assert
        LogAssert("Verifying Id is set correctly");
        model.Id.ShouldBe(expectedId);
    }

    [Fact]
    public void TenantCode_ShouldBeSettableAndGettable()
    {
        // Arrange
        LogArrange("Creating DataModelBase instance");
        DataModelBase model = new();
        Guid expectedTenantCode = Guid.NewGuid();

        // Act
        LogAct("Setting TenantCode property");
        model.TenantCode = expectedTenantCode;

        // Assert
        LogAssert("Verifying TenantCode is set correctly");
        model.TenantCode.ShouldBe(expectedTenantCode);
    }

    [Fact]
    public void CreatedBy_ShouldBeSettableAndGettable()
    {
        // Arrange
        LogArrange("Creating DataModelBase instance");
        DataModelBase model = new();
        const string expectedCreatedBy = "test-user";

        // Act
        LogAct("Setting CreatedBy property");
        model.CreatedBy = expectedCreatedBy;

        // Assert
        LogAssert("Verifying CreatedBy is set correctly");
        model.CreatedBy.ShouldBe(expectedCreatedBy);
    }

    [Fact]
    public void CreatedAt_ShouldBeSettableAndGettable()
    {
        // Arrange
        LogArrange("Creating DataModelBase instance");
        DataModelBase model = new();
        DateTimeOffset expectedCreatedAt = DateTimeOffset.UtcNow;

        // Act
        LogAct("Setting CreatedAt property");
        model.CreatedAt = expectedCreatedAt;

        // Assert
        LogAssert("Verifying CreatedAt is set correctly");
        model.CreatedAt.ShouldBe(expectedCreatedAt);
    }

    [Fact]
    public void CreatedCorrelationId_ShouldBeSettableAndGettable()
    {
        // Arrange
        LogArrange("Creating DataModelBase instance");
        DataModelBase model = new();
        Guid expectedCorrelationId = Guid.NewGuid();

        // Act
        LogAct("Setting CreatedCorrelationId property");
        model.CreatedCorrelationId = expectedCorrelationId;

        // Assert
        LogAssert("Verifying CreatedCorrelationId is set correctly");
        model.CreatedCorrelationId.ShouldBe(expectedCorrelationId);
    }

    [Fact]
    public void CreatedExecutionOrigin_ShouldBeSettableAndGettable()
    {
        // Arrange
        LogArrange("Creating DataModelBase instance");
        DataModelBase model = new();
        const string expectedOrigin = "API";

        // Act
        LogAct("Setting CreatedExecutionOrigin property");
        model.CreatedExecutionOrigin = expectedOrigin;

        // Assert
        LogAssert("Verifying CreatedExecutionOrigin is set correctly");
        model.CreatedExecutionOrigin.ShouldBe(expectedOrigin);
    }

    [Fact]
    public void CreatedBusinessOperationCode_ShouldBeSettableAndGettable()
    {
        // Arrange
        LogArrange("Creating DataModelBase instance");
        DataModelBase model = new();
        const string expectedCode = "CREATE_ORDER";

        // Act
        LogAct("Setting CreatedBusinessOperationCode property");
        model.CreatedBusinessOperationCode = expectedCode;

        // Assert
        LogAssert("Verifying CreatedBusinessOperationCode is set correctly");
        model.CreatedBusinessOperationCode.ShouldBe(expectedCode);
    }

    [Fact]
    public void LastChangedBy_ShouldBeNullableAndSettable()
    {
        // Arrange
        LogArrange("Creating DataModelBase instance");
        DataModelBase model = new();

        // Act & Assert - null case
        LogAct("Verifying LastChangedBy is null by default");
        model.LastChangedBy.ShouldBeNull();

        // Act - set value
        LogAct("Setting LastChangedBy property");
        model.LastChangedBy = "modifier-user";

        // Assert
        LogAssert("Verifying LastChangedBy is set correctly");
        model.LastChangedBy.ShouldBe("modifier-user");
    }

    [Fact]
    public void LastChangedAt_ShouldBeNullableAndSettable()
    {
        // Arrange
        LogArrange("Creating DataModelBase instance");
        DataModelBase model = new();
        DateTimeOffset expectedLastChangedAt = DateTimeOffset.UtcNow;

        // Act & Assert - null case
        LogAct("Verifying LastChangedAt is null by default");
        model.LastChangedAt.ShouldBeNull();

        // Act - set value
        LogAct("Setting LastChangedAt property");
        model.LastChangedAt = expectedLastChangedAt;

        // Assert
        LogAssert("Verifying LastChangedAt is set correctly");
        model.LastChangedAt.ShouldBe(expectedLastChangedAt);
    }

    [Fact]
    public void LastChangedExecutionOrigin_ShouldBeNullableAndSettable()
    {
        // Arrange
        LogArrange("Creating DataModelBase instance");
        DataModelBase model = new();

        // Act & Assert - null case
        LogAct("Verifying LastChangedExecutionOrigin is null by default");
        model.LastChangedExecutionOrigin.ShouldBeNull();

        // Act - set value
        LogAct("Setting LastChangedExecutionOrigin property");
        model.LastChangedExecutionOrigin = "API";

        // Assert
        LogAssert("Verifying LastChangedExecutionOrigin is set correctly");
        model.LastChangedExecutionOrigin.ShouldBe("API");
    }

    [Fact]
    public void LastChangedCorrelationId_ShouldBeNullableAndSettable()
    {
        // Arrange
        LogArrange("Creating DataModelBase instance");
        DataModelBase model = new();
        Guid expectedCorrelationId = Guid.NewGuid();

        // Act & Assert - null case
        LogAct("Verifying LastChangedCorrelationId is null by default");
        model.LastChangedCorrelationId.ShouldBeNull();

        // Act - set value
        LogAct("Setting LastChangedCorrelationId property");
        model.LastChangedCorrelationId = expectedCorrelationId;

        // Assert
        LogAssert("Verifying LastChangedCorrelationId is set correctly");
        model.LastChangedCorrelationId.ShouldBe(expectedCorrelationId);
    }

    [Fact]
    public void LastChangedBusinessOperationCode_ShouldBeNullableAndSettable()
    {
        // Arrange
        LogArrange("Creating DataModelBase instance");
        DataModelBase model = new();

        // Act & Assert - null case
        LogAct("Verifying LastChangedBusinessOperationCode is null by default");
        model.LastChangedBusinessOperationCode.ShouldBeNull();

        // Act - set value
        LogAct("Setting LastChangedBusinessOperationCode property");
        model.LastChangedBusinessOperationCode = "UPDATE_ORDER";

        // Assert
        LogAssert("Verifying LastChangedBusinessOperationCode is set correctly");
        model.LastChangedBusinessOperationCode.ShouldBe("UPDATE_ORDER");
    }

    [Fact]
    public void EntityVersion_ShouldBeSettableAndGettable()
    {
        // Arrange
        LogArrange("Creating DataModelBase instance");
        DataModelBase model = new();
        long expectedVersion = 123456789L;

        // Act
        LogAct("Setting EntityVersion property");
        model.EntityVersion = expectedVersion;

        // Assert
        LogAssert("Verifying EntityVersion is set correctly");
        model.EntityVersion.ShouldBe(expectedVersion);
    }

    [Fact]
    public void AllProperties_ShouldBeSettableInObjectInitializer()
    {
        // Arrange
        LogArrange("Preparing test data");
        Guid id = Guid.NewGuid();
        Guid tenantCode = Guid.NewGuid();
        Guid createdCorrelationId = Guid.NewGuid();
        DateTimeOffset createdAt = DateTimeOffset.UtcNow;
        DateTimeOffset lastChangedAt = DateTimeOffset.UtcNow.AddHours(1);
        Guid lastChangedCorrelationId = Guid.NewGuid();

        // Act
        LogAct("Creating DataModelBase with object initializer");
        DataModelBase model = new()
        {
            Id = id,
            TenantCode = tenantCode,
            CreatedBy = "creator",
            CreatedAt = createdAt,
            CreatedCorrelationId = createdCorrelationId,
            CreatedExecutionOrigin = "API",
            CreatedBusinessOperationCode = "CREATE_ORDER",
            LastChangedBy = "modifier",
            LastChangedAt = lastChangedAt,
            LastChangedExecutionOrigin = "CLI",
            LastChangedCorrelationId = lastChangedCorrelationId,
            LastChangedBusinessOperationCode = "UPDATE_ORDER",
            EntityVersion = 999L
        };

        // Assert
        LogAssert("Verifying all properties are set correctly");
        model.Id.ShouldBe(id);
        model.TenantCode.ShouldBe(tenantCode);
        model.CreatedBy.ShouldBe("creator");
        model.CreatedAt.ShouldBe(createdAt);
        model.CreatedCorrelationId.ShouldBe(createdCorrelationId);
        model.CreatedExecutionOrigin.ShouldBe("API");
        model.CreatedBusinessOperationCode.ShouldBe("CREATE_ORDER");
        model.LastChangedBy.ShouldBe("modifier");
        model.LastChangedAt.ShouldBe(lastChangedAt);
        model.LastChangedExecutionOrigin.ShouldBe("CLI");
        model.LastChangedCorrelationId.ShouldBe(lastChangedCorrelationId);
        model.LastChangedBusinessOperationCode.ShouldBe("UPDATE_ORDER");
        model.EntityVersion.ShouldBe(999L);
    }
}
