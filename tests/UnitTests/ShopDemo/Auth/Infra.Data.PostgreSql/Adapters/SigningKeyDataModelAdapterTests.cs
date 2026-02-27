using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.SigningKeys;
using ShopDemo.Auth.Domain.Entities.SigningKeys.Enums;
using ShopDemo.Auth.Domain.Entities.SigningKeys.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Adapters;

public class SigningKeyDataModelAdapterTests : TestBase
{
    private static readonly Faker Faker = new();

    public SigningKeyDataModelAdapterTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Adapt_ShouldUpdateKidFromEntity()
    {
        // Arrange
        LogArrange("Creating SigningKeyDataModel and SigningKey with different kid values");
        var dataModel = CreateTestDataModel();
        string expectedKid = Guid.NewGuid().ToString();
        var entity = CreateTestEntity(kid: expectedKid);

        // Act
        LogAct("Adapting data model from entity");
        SigningKeyDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying Kid was updated");
        dataModel.Kid.ShouldBe(expectedKid);
    }

    [Fact]
    public void Adapt_ShouldUpdateAlgorithmFromEntity()
    {
        // Arrange
        LogArrange("Creating SigningKeyDataModel and SigningKey with different algorithms");
        var dataModel = CreateTestDataModel();
        string expectedAlgorithm = "ES256";
        var entity = CreateTestEntity(algorithm: expectedAlgorithm);

        // Act
        LogAct("Adapting data model from entity");
        SigningKeyDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying Algorithm was updated");
        dataModel.Algorithm.ShouldBe(expectedAlgorithm);
    }

    [Fact]
    public void Adapt_ShouldUpdateStatusFromEntity()
    {
        // Arrange
        LogArrange("Creating SigningKeyDataModel and SigningKey with different statuses");
        var dataModel = CreateTestDataModel();
        dataModel.Status = (short)SigningKeyStatus.Active;
        var entity = CreateTestEntity(status: SigningKeyStatus.Rotated);

        // Act
        LogAct("Adapting data model from entity");
        SigningKeyDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying Status was updated");
        dataModel.Status.ShouldBe((short)SigningKeyStatus.Rotated);
    }

    [Fact]
    public void Adapt_ShouldUpdateExpiresAtFromEntity()
    {
        // Arrange
        LogArrange("Creating SigningKeyDataModel and SigningKey with different expiresAt values");
        var dataModel = CreateTestDataModel();
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddDays(180);
        var entity = CreateTestEntity(expiresAt: expectedExpiresAt);

        // Act
        LogAct("Adapting data model from entity");
        SigningKeyDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying ExpiresAt was updated");
        dataModel.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Fact]
    public void Adapt_ShouldUpdateBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating SigningKeyDataModel and SigningKey with different EntityInfo values");
        var dataModel = CreateTestDataModel();
        var expectedId = Guid.NewGuid();
        var expectedTenantCode = Guid.NewGuid();
        string expectedCreatedBy = Faker.Person.FullName;
        var expectedCreatedAt = DateTimeOffset.UtcNow.AddDays(-2);
        long expectedVersion = Faker.Random.Long(1);

        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(expectedId),
            tenantInfo: TenantInfo.Create(expectedTenantCode),
            createdAt: expectedCreatedAt,
            createdBy: expectedCreatedBy,
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "UnitTest",
            createdBusinessOperationCode: "TEST_OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(expectedVersion));

        var entity = SigningKey.CreateFromExistingInfo(
            new CreateFromExistingInfoSigningKeyInput(
                entityInfo,
                Kid.CreateFromExistingInfo(Guid.NewGuid().ToString()),
                "RS256",
                "public-key",
                "encrypted-private-key",
                SigningKeyStatus.Active,
                null,
                DateTimeOffset.UtcNow.AddDays(90)));

        // Act
        LogAct("Adapting data model from entity");
        SigningKeyDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying base fields were updated from EntityInfo");
        dataModel.Id.ShouldBe(expectedId);
        dataModel.TenantCode.ShouldBe(expectedTenantCode);
        dataModel.CreatedBy.ShouldBe(expectedCreatedBy);
        dataModel.CreatedAt.ShouldBe(expectedCreatedAt);
        dataModel.EntityVersion.ShouldBe(expectedVersion);
    }

    [Fact]
    public void Adapt_ShouldReturnTheSameDataModelInstance()
    {
        // Arrange
        LogArrange("Creating SigningKeyDataModel and SigningKey");
        var dataModel = CreateTestDataModel();
        var entity = CreateTestEntity();

        // Act
        LogAct("Adapting data model from entity");
        var result = SigningKeyDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying the same instance is returned");
        result.ShouldBeSameAs(dataModel);
    }

    #region Helper Methods

    private static SigningKeyDataModel CreateTestDataModel()
    {
        return new SigningKeyDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            EntityVersion = 1,
            Kid = Guid.NewGuid().ToString(),
            Algorithm = "RS256",
            PublicKey = "initial-public-key",
            EncryptedPrivateKey = "initial-encrypted-private-key",
            Status = (short)SigningKeyStatus.Active,
            RotatedAt = null,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(90)
        };
    }

    private static SigningKey CreateTestEntity(
        string? kid = null,
        string? algorithm = null,
        SigningKeyStatus status = SigningKeyStatus.Active,
        DateTimeOffset? expiresAt = null)
    {
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo: TenantInfo.Create(Guid.NewGuid()),
            createdAt: DateTimeOffset.UtcNow,
            createdBy: "test-creator",
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "UnitTest",
            createdBusinessOperationCode: "TEST_OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));

        return SigningKey.CreateFromExistingInfo(
            new CreateFromExistingInfoSigningKeyInput(
                entityInfo,
                Kid.CreateFromExistingInfo(kid ?? Guid.NewGuid().ToString()),
                algorithm ?? "RS256",
                "public-key",
                "encrypted-private-key",
                status,
                null,
                expiresAt ?? DateTimeOffset.UtcNow.AddDays(90)));
    }

    #endregion
}
