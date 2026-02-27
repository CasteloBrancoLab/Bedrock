using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class TokenExchangeFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public TokenExchangeFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapUserIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating TokenExchangeDataModel with specific userId");
        var expectedUserId = Guid.NewGuid();
        var dataModel = CreateTestDataModel();
        dataModel.UserId = expectedUserId;

        // Act
        LogAct("Creating TokenExchange from TokenExchangeDataModel");
        var entity = TokenExchangeFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying UserId mapping");
        entity.UserId.Value.ShouldBe(expectedUserId);
    }

    [Fact]
    public void Create_ShouldMapSubjectTokenJtiFromDataModel()
    {
        // Arrange
        LogArrange("Creating TokenExchangeDataModel with specific subjectTokenJti");
        string expectedSubjectTokenJti = Guid.NewGuid().ToString();
        var dataModel = CreateTestDataModel(subjectTokenJti: expectedSubjectTokenJti);

        // Act
        LogAct("Creating TokenExchange from TokenExchangeDataModel");
        var entity = TokenExchangeFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying SubjectTokenJti mapping");
        entity.SubjectTokenJti.ShouldBe(expectedSubjectTokenJti);
    }

    [Fact]
    public void Create_ShouldMapRequestedAudienceFromDataModel()
    {
        // Arrange
        LogArrange("Creating TokenExchangeDataModel with specific requestedAudience");
        string expectedRequestedAudience = "payment-service";
        var dataModel = CreateTestDataModel(requestedAudience: expectedRequestedAudience);

        // Act
        LogAct("Creating TokenExchange from TokenExchangeDataModel");
        var entity = TokenExchangeFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying RequestedAudience mapping");
        entity.RequestedAudience.ShouldBe(expectedRequestedAudience);
    }

    [Fact]
    public void Create_ShouldMapIssuedTokenJtiFromDataModel()
    {
        // Arrange
        LogArrange("Creating TokenExchangeDataModel with specific issuedTokenJti");
        string expectedIssuedTokenJti = Guid.NewGuid().ToString();
        var dataModel = CreateTestDataModel(issuedTokenJti: expectedIssuedTokenJti);

        // Act
        LogAct("Creating TokenExchange from TokenExchangeDataModel");
        var entity = TokenExchangeFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying IssuedTokenJti mapping");
        entity.IssuedTokenJti.ShouldBe(expectedIssuedTokenJti);
    }

    [Fact]
    public void Create_ShouldMapIssuedAtFromDataModel()
    {
        // Arrange
        LogArrange("Creating TokenExchangeDataModel with specific issuedAt");
        var expectedIssuedAt = DateTimeOffset.UtcNow.AddMinutes(-10);
        var dataModel = CreateTestDataModel();
        dataModel.IssuedAt = expectedIssuedAt;

        // Act
        LogAct("Creating TokenExchange from TokenExchangeDataModel");
        var entity = TokenExchangeFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying IssuedAt mapping");
        entity.IssuedAt.ShouldBe(expectedIssuedAt);
    }

    [Fact]
    public void Create_ShouldMapExpiresAtFromDataModel()
    {
        // Arrange
        LogArrange("Creating TokenExchangeDataModel with specific expiresAt");
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddHours(2);
        var dataModel = CreateTestDataModel();
        dataModel.ExpiresAt = expectedExpiresAt;

        // Act
        LogAct("Creating TokenExchange from TokenExchangeDataModel");
        var entity = TokenExchangeFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying ExpiresAt mapping");
        entity.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Fact]
    public void Create_ShouldMapEntityInfoFieldsFromDataModel()
    {
        // Arrange
        LogArrange("Creating TokenExchangeDataModel with specific base fields");
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

        var dataModel = new TokenExchangeDataModel
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
            UserId = Guid.NewGuid(),
            SubjectTokenJti = Guid.NewGuid().ToString(),
            RequestedAudience = "api-service",
            IssuedTokenJti = Guid.NewGuid().ToString(),
            IssuedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
        };

        // Act
        LogAct("Creating TokenExchange from TokenExchangeDataModel");
        var entity = TokenExchangeFactory.Create(dataModel);

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
        LogArrange("Creating TokenExchangeDataModel with null last-changed fields");
        var dataModel = new TokenExchangeDataModel
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
            UserId = Guid.NewGuid(),
            SubjectTokenJti = Guid.NewGuid().ToString(),
            RequestedAudience = "api-service",
            IssuedTokenJti = Guid.NewGuid().ToString(),
            IssuedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
        };

        // Act
        LogAct("Creating TokenExchange from TokenExchangeDataModel with nulls");
        var entity = TokenExchangeFactory.Create(dataModel);

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
        LogArrange("Creating TokenExchangeDataModel to verify createdCorrelationId is mapped from data model");
        var dataModel = CreateTestDataModel();

        // Act
        LogAct("Creating TokenExchange from TokenExchangeDataModel");
        var entity = TokenExchangeFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedCorrelationId matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedCorrelationId.ShouldBe(dataModel.CreatedCorrelationId);
    }

    [Fact]
    public void Create_ShouldMapCreatedExecutionOriginFromDataModel()
    {
        // Arrange
        LogArrange("Creating TokenExchangeDataModel to verify createdExecutionOrigin is mapped from data model");
        var dataModel = CreateTestDataModel();

        // Act
        LogAct("Creating TokenExchange from TokenExchangeDataModel");
        var entity = TokenExchangeFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedExecutionOrigin matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedExecutionOrigin.ShouldBe(dataModel.CreatedExecutionOrigin);
    }

    [Fact]
    public void Create_ShouldMapCreatedBusinessOperationCodeFromDataModel()
    {
        // Arrange
        LogArrange("Creating TokenExchangeDataModel to verify createdBusinessOperationCode is mapped from data model");
        var dataModel = CreateTestDataModel();

        // Act
        LogAct("Creating TokenExchange from TokenExchangeDataModel");
        var entity = TokenExchangeFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedBusinessOperationCode matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedBusinessOperationCode.ShouldBe(dataModel.CreatedBusinessOperationCode);
    }

    #region Helper Methods

    private static TokenExchangeDataModel CreateTestDataModel(
        string? subjectTokenJti = null,
        string? requestedAudience = null,
        string? issuedTokenJti = null)
    {
        return new TokenExchangeDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "UnitTest",
            CreatedBusinessOperationCode = "CREATE_TOKEN_EXCHANGE",
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedExecutionOrigin = null,
            LastChangedCorrelationId = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            UserId = Guid.NewGuid(),
            SubjectTokenJti = subjectTokenJti ?? Guid.NewGuid().ToString(),
            RequestedAudience = requestedAudience ?? "api-service",
            IssuedTokenJti = issuedTokenJti ?? Guid.NewGuid().ToString(),
            IssuedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
        };
    }

    #endregion
}
