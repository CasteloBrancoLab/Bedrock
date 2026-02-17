using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.DataModels;

public class RefreshTokenDataModelTests : TestBase
{
    private static readonly Faker Faker = new();

    public RefreshTokenDataModelTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void RefreshTokenDataModel_ShouldExtendDataModelBase()
    {
        // Arrange
        LogArrange("Criando instancia de RefreshTokenDataModel");
        var dataModel = new RefreshTokenDataModel();

        // Act & Assert
        LogAssert("Verificando que herda de DataModelBase");
        dataModel.ShouldBeAssignableTo<DataModelBase>();
    }

    [Fact]
    public void RefreshTokenDataModel_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        LogArrange("Criando instancia com valores default");
        var dataModel = new RefreshTokenDataModel();

        // Assert
        LogAssert("Verificando valores default");
        dataModel.UserId.ShouldBe(Guid.Empty);
        dataModel.TokenHash.ShouldBeNull();
        dataModel.FamilyId.ShouldBe(Guid.Empty);
        dataModel.ExpiresAt.ShouldBe(default);
        dataModel.Status.ShouldBe((short)0);
        dataModel.RevokedAt.ShouldBeNull();
        dataModel.ReplacedByTokenId.ShouldBeNull();
    }

    [Fact]
    public void RefreshTokenDataModel_ShouldSetAndGetUserId()
    {
        // Arrange
        LogArrange("Criando data model com UserId");
        var dataModel = new RefreshTokenDataModel();
        Guid userId = Guid.NewGuid();

        // Act
        LogAct("Setando UserId");
        dataModel.UserId = userId;

        // Assert
        LogAssert("Verificando UserId");
        dataModel.UserId.ShouldBe(userId);
    }

    [Fact]
    public void RefreshTokenDataModel_ShouldSetAndGetTokenHash()
    {
        // Arrange
        LogArrange("Criando data model com TokenHash");
        var dataModel = new RefreshTokenDataModel();
        byte[] tokenHash = Faker.Random.Bytes(32);

        // Act
        LogAct("Setando TokenHash");
        dataModel.TokenHash = tokenHash;

        // Assert
        LogAssert("Verificando TokenHash");
        dataModel.TokenHash.ShouldBe(tokenHash);
    }

    [Fact]
    public void RefreshTokenDataModel_ShouldSetAndGetFamilyId()
    {
        // Arrange
        LogArrange("Criando data model com FamilyId");
        var dataModel = new RefreshTokenDataModel();
        Guid familyId = Guid.NewGuid();

        // Act
        LogAct("Setando FamilyId");
        dataModel.FamilyId = familyId;

        // Assert
        LogAssert("Verificando FamilyId");
        dataModel.FamilyId.ShouldBe(familyId);
    }

    [Fact]
    public void RefreshTokenDataModel_ShouldSetAndGetExpiresAt()
    {
        // Arrange
        LogArrange("Criando data model com ExpiresAt");
        var dataModel = new RefreshTokenDataModel();
        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddDays(7);

        // Act
        LogAct("Setando ExpiresAt");
        dataModel.ExpiresAt = expiresAt;

        // Assert
        LogAssert("Verificando ExpiresAt");
        dataModel.ExpiresAt.ShouldBe(expiresAt);
    }

    [Fact]
    public void RefreshTokenDataModel_ShouldSetAndGetStatus()
    {
        // Arrange
        LogArrange("Criando data model com Status");
        var dataModel = new RefreshTokenDataModel();

        // Act
        LogAct("Setando Status");
        dataModel.Status = 1;

        // Assert
        LogAssert("Verificando Status");
        dataModel.Status.ShouldBe((short)1);
    }

    [Fact]
    public void RefreshTokenDataModel_ShouldSetAndGetRevokedAt()
    {
        // Arrange
        LogArrange("Criando data model com RevokedAt");
        var dataModel = new RefreshTokenDataModel();
        DateTimeOffset revokedAt = DateTimeOffset.UtcNow;

        // Act
        LogAct("Setando RevokedAt");
        dataModel.RevokedAt = revokedAt;

        // Assert
        LogAssert("Verificando RevokedAt");
        dataModel.RevokedAt.ShouldBe(revokedAt);
    }

    [Fact]
    public void RefreshTokenDataModel_ShouldSetAndGetReplacedByTokenId()
    {
        // Arrange
        LogArrange("Criando data model com ReplacedByTokenId");
        var dataModel = new RefreshTokenDataModel();
        Guid replacedByTokenId = Guid.NewGuid();

        // Act
        LogAct("Setando ReplacedByTokenId");
        dataModel.ReplacedByTokenId = replacedByTokenId;

        // Assert
        LogAssert("Verificando ReplacedByTokenId");
        dataModel.ReplacedByTokenId.ShouldBe(replacedByTokenId);
    }

    [Fact]
    public void RefreshTokenDataModel_ShouldSetAllPropertiesSimultaneously()
    {
        // Arrange
        LogArrange("Criando data model com todas as propriedades");
        Guid userId = Guid.NewGuid();
        byte[] tokenHash = Faker.Random.Bytes(32);
        Guid familyId = Guid.NewGuid();
        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddDays(7);
        short status = 1;
        DateTimeOffset revokedAt = DateTimeOffset.UtcNow;
        Guid replacedByTokenId = Guid.NewGuid();

        // Act
        LogAct("Criando data model com inicializador de objeto");
        var dataModel = new RefreshTokenDataModel
        {
            UserId = userId,
            TokenHash = tokenHash,
            FamilyId = familyId,
            ExpiresAt = expiresAt,
            Status = status,
            RevokedAt = revokedAt,
            ReplacedByTokenId = replacedByTokenId
        };

        // Assert
        LogAssert("Verificando todas as propriedades");
        dataModel.UserId.ShouldBe(userId);
        dataModel.TokenHash.ShouldBe(tokenHash);
        dataModel.FamilyId.ShouldBe(familyId);
        dataModel.ExpiresAt.ShouldBe(expiresAt);
        dataModel.Status.ShouldBe(status);
        dataModel.RevokedAt.ShouldBe(revokedAt);
        dataModel.ReplacedByTokenId.ShouldBe(replacedByTokenId);
    }

    [Fact]
    public void RefreshTokenDataModel_ShouldInheritBaseProperties()
    {
        // Arrange
        LogArrange("Criando data model com propriedades base");
        Guid id = Guid.NewGuid();
        Guid tenantCode = Guid.NewGuid();
        string createdBy = Faker.Internet.UserName();
        DateTimeOffset createdAt = DateTimeOffset.UtcNow;
        long entityVersion = 1;

        // Act
        LogAct("Setando propriedades herdadas de DataModelBase");
        var dataModel = new RefreshTokenDataModel
        {
            Id = id,
            TenantCode = tenantCode,
            CreatedBy = createdBy,
            CreatedAt = createdAt,
            EntityVersion = entityVersion
        };

        // Assert
        LogAssert("Verificando propriedades base");
        dataModel.Id.ShouldBe(id);
        dataModel.TenantCode.ShouldBe(tenantCode);
        dataModel.CreatedBy.ShouldBe(createdBy);
        dataModel.CreatedAt.ShouldBe(createdAt);
        dataModel.EntityVersion.ShouldBe(entityVersion);
    }
}
