using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.ImpersonationSessions.Enums;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class ImpersonationSessionFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public ImpersonationSessionFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapOperatorUserIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating ImpersonationSessionDataModel with specific OperatorUserId");
        var expectedOperatorUserId = Guid.NewGuid();
        var dataModel = CreateTestDataModel(expectedOperatorUserId, Guid.NewGuid(), DateTimeOffset.UtcNow.AddHours(1), (short)ImpersonationSessionStatus.Active, null);

        // Act
        LogAct("Creating ImpersonationSession from ImpersonationSessionDataModel");
        var entity = ImpersonationSessionFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying OperatorUserId mapping");
        entity.OperatorUserId.Value.ShouldBe(expectedOperatorUserId);
    }

    [Fact]
    public void Create_ShouldMapTargetUserIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating ImpersonationSessionDataModel with specific TargetUserId");
        var expectedTargetUserId = Guid.NewGuid();
        var dataModel = CreateTestDataModel(Guid.NewGuid(), expectedTargetUserId, DateTimeOffset.UtcNow.AddHours(1), (short)ImpersonationSessionStatus.Active, null);

        // Act
        LogAct("Creating ImpersonationSession from ImpersonationSessionDataModel");
        var entity = ImpersonationSessionFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying TargetUserId mapping");
        entity.TargetUserId.Value.ShouldBe(expectedTargetUserId);
    }

    [Fact]
    public void Create_ShouldMapExpiresAtFromDataModel()
    {
        // Arrange
        LogArrange("Creating ImpersonationSessionDataModel with specific ExpiresAt");
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddHours(8);
        var dataModel = CreateTestDataModel(Guid.NewGuid(), Guid.NewGuid(), expectedExpiresAt, (short)ImpersonationSessionStatus.Active, null);

        // Act
        LogAct("Creating ImpersonationSession from ImpersonationSessionDataModel");
        var entity = ImpersonationSessionFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying ExpiresAt mapping");
        entity.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Theory]
    [InlineData((short)1, ImpersonationSessionStatus.Active)]
    [InlineData((short)2, ImpersonationSessionStatus.Ended)]
    public void Create_ShouldMapStatusFromDataModel(short statusValue, ImpersonationSessionStatus expectedStatus)
    {
        // Arrange
        LogArrange($"Creating ImpersonationSessionDataModel with status value {statusValue}");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddHours(1), statusValue, null);

        // Act
        LogAct("Creating ImpersonationSession from ImpersonationSessionDataModel");
        var entity = ImpersonationSessionFactory.Create(dataModel);

        // Assert
        LogAssert($"Verifying Status mapped to {expectedStatus}");
        entity.Status.ShouldBe(expectedStatus);
    }

    [Fact]
    public void Create_ShouldMapEndedAtFromDataModel()
    {
        // Arrange
        LogArrange("Creating ImpersonationSessionDataModel with specific EndedAt");
        DateTimeOffset? expectedEndedAt = DateTimeOffset.UtcNow.AddMinutes(-20);
        var dataModel = CreateTestDataModel(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddHours(1), (short)ImpersonationSessionStatus.Ended, expectedEndedAt);

        // Act
        LogAct("Creating ImpersonationSession from ImpersonationSessionDataModel");
        var entity = ImpersonationSessionFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying EndedAt mapping");
        entity.EndedAt.ShouldBe(expectedEndedAt);
    }

    [Fact]
    public void Create_ShouldMapNullEndedAtFromDataModel()
    {
        // Arrange
        LogArrange("Creating ImpersonationSessionDataModel with null EndedAt");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddHours(1), (short)ImpersonationSessionStatus.Active, null);

        // Act
        LogAct("Creating ImpersonationSession from ImpersonationSessionDataModel");
        var entity = ImpersonationSessionFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying null EndedAt mapping");
        entity.EndedAt.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapEntityInfoFieldsFromDataModel()
    {
        // Arrange
        LogArrange("Creating ImpersonationSessionDataModel with specific base fields");
        var expectedId = Guid.NewGuid();
        var expectedTenantCode = Guid.NewGuid();
        string expectedCreatedBy = Faker.Person.FullName;
        var expectedCreatedAt = DateTimeOffset.UtcNow.AddDays(-5);
        long expectedVersion = Faker.Random.Long(1);
        string? expectedLastChangedBy = Faker.Person.FullName;
        var expectedLastChangedAt = DateTimeOffset.UtcNow;
        var expectedLastChangedCorrelationId = Guid.NewGuid();
        string expectedLastChangedExecutionOrigin = "TestOrigin";
        string expectedLastChangedBusinessOperationCode = "TEST_OP";

        var dataModel = new ImpersonationSessionDataModel
        {
            Id = expectedId,
            TenantCode = expectedTenantCode,
            CreatedBy = expectedCreatedBy,
            CreatedAt = expectedCreatedAt,
            LastChangedBy = expectedLastChangedBy,
            LastChangedAt = expectedLastChangedAt,
            LastChangedCorrelationId = expectedLastChangedCorrelationId,
            LastChangedExecutionOrigin = expectedLastChangedExecutionOrigin,
            LastChangedBusinessOperationCode = expectedLastChangedBusinessOperationCode,
            EntityVersion = expectedVersion,
            OperatorUserId = Guid.NewGuid(),
            TargetUserId = Guid.NewGuid(),
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
            Status = (short)ImpersonationSessionStatus.Active,
            EndedAt = null
        };

        // Act
        LogAct("Creating ImpersonationSession from ImpersonationSessionDataModel");
        var entity = ImpersonationSessionFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying EntityInfo fields");
        entity.EntityInfo.Id.Value.ShouldBe(expectedId);
        entity.EntityInfo.TenantInfo.Code.ShouldBe(expectedTenantCode);
        entity.EntityInfo.EntityChangeInfo.CreatedBy.ShouldBe(expectedCreatedBy);
        entity.EntityInfo.EntityChangeInfo.CreatedAt.ShouldBe(expectedCreatedAt);
        entity.EntityInfo.EntityVersion.Value.ShouldBe(expectedVersion);
        entity.EntityInfo.EntityChangeInfo.LastChangedBy.ShouldBe(expectedLastChangedBy);
        entity.EntityInfo.EntityChangeInfo.LastChangedAt.ShouldBe(expectedLastChangedAt);
        entity.EntityInfo.EntityChangeInfo.LastChangedCorrelationId.ShouldBe(expectedLastChangedCorrelationId);
        entity.EntityInfo.EntityChangeInfo.LastChangedExecutionOrigin.ShouldBe(expectedLastChangedExecutionOrigin);
        entity.EntityInfo.EntityChangeInfo.LastChangedBusinessOperationCode.ShouldBe(expectedLastChangedBusinessOperationCode);
    }

    [Fact]
    public void Create_WithNullLastChangedFields_ShouldMapCorrectly()
    {
        // Arrange
        LogArrange("Creating ImpersonationSessionDataModel with null last-changed fields");
        var dataModel = new ImpersonationSessionDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "creator",
            CreatedAt = DateTimeOffset.UtcNow,
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedCorrelationId = null,
            LastChangedExecutionOrigin = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            OperatorUserId = Guid.NewGuid(),
            TargetUserId = Guid.NewGuid(),
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
            Status = (short)ImpersonationSessionStatus.Active,
            EndedAt = null
        };

        // Act
        LogAct("Creating ImpersonationSession from ImpersonationSessionDataModel with nulls");
        var entity = ImpersonationSessionFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying nullable fields are null");
        entity.EntityInfo.EntityChangeInfo.LastChangedBy.ShouldBeNull();
        entity.EntityInfo.EntityChangeInfo.LastChangedAt.ShouldBeNull();
        entity.EntityInfo.EntityChangeInfo.LastChangedCorrelationId.ShouldBeNull();
        entity.EntityInfo.EntityChangeInfo.LastChangedExecutionOrigin.ShouldBeNull();
        entity.EntityInfo.EntityChangeInfo.LastChangedBusinessOperationCode.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapCreatedCorrelationIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating ImpersonationSessionDataModel to verify CreatedCorrelationId is mapped");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddHours(1), (short)ImpersonationSessionStatus.Active, null);

        // Act
        LogAct("Creating ImpersonationSession from ImpersonationSessionDataModel");
        var entity = ImpersonationSessionFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedCorrelationId matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedCorrelationId.ShouldBe(dataModel.CreatedCorrelationId);
    }

    [Fact]
    public void Create_ShouldMapCreatedExecutionOriginFromDataModel()
    {
        // Arrange
        LogArrange("Creating ImpersonationSessionDataModel to verify CreatedExecutionOrigin is mapped");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddHours(1), (short)ImpersonationSessionStatus.Active, null);

        // Act
        LogAct("Creating ImpersonationSession from ImpersonationSessionDataModel");
        var entity = ImpersonationSessionFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedExecutionOrigin matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedExecutionOrigin.ShouldBe(dataModel.CreatedExecutionOrigin);
    }

    [Fact]
    public void Create_ShouldMapCreatedBusinessOperationCodeFromDataModel()
    {
        // Arrange
        LogArrange("Creating ImpersonationSessionDataModel to verify CreatedBusinessOperationCode is mapped");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddHours(1), (short)ImpersonationSessionStatus.Active, null);

        // Act
        LogAct("Creating ImpersonationSession from ImpersonationSessionDataModel");
        var entity = ImpersonationSessionFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedBusinessOperationCode matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedBusinessOperationCode.ShouldBe(dataModel.CreatedBusinessOperationCode);
    }

    #region Helper Methods

    private static ImpersonationSessionDataModel CreateTestDataModel(
        Guid operatorUserId,
        Guid targetUserId,
        DateTimeOffset expiresAt,
        short status,
        DateTimeOffset? endedAt)
    {
        return new ImpersonationSessionDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "UnitTest",
            CreatedBusinessOperationCode = "CREATE_IMPERSONATION_SESSION",
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedExecutionOrigin = null,
            LastChangedCorrelationId = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            OperatorUserId = operatorUserId,
            TargetUserId = targetUserId,
            ExpiresAt = expiresAt,
            Status = status,
            EndedAt = endedAt
        };
    }

    #endregion
}
