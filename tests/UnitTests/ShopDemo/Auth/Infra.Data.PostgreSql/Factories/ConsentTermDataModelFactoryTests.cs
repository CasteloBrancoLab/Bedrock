using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.ConsentTerms;
using ShopDemo.Auth.Domain.Entities.ConsentTerms.Enums;
using ShopDemo.Auth.Domain.Entities.ConsentTerms.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class ConsentTermDataModelFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public ConsentTermDataModelFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Theory]
    [InlineData(ConsentTermType.TermsOfUse, 1)]
    [InlineData(ConsentTermType.PrivacyPolicy, 2)]
    [InlineData(ConsentTermType.Marketing, 3)]
    public void Create_ShouldMapTypeAsShortCorrectly(ConsentTermType type, short expectedShortValue)
    {
        // Arrange
        LogArrange($"Creating ConsentTerm entity with type {type}");
        var entity = CreateTestEntity(type, "1.0", "Content", DateTimeOffset.UtcNow);

        // Act
        LogAct("Creating ConsentTermDataModel from ConsentTerm entity");
        var dataModel = ConsentTermDataModelFactory.Create(entity);

        // Assert
        LogAssert($"Verifying Type mapped to short value {expectedShortValue}");
        dataModel.Type.ShouldBe(expectedShortValue);
    }

    [Fact]
    public void Create_ShouldMapVersionCorrectly()
    {
        // Arrange
        LogArrange("Creating ConsentTerm entity with known Version");
        string expectedVersion = "2.1";
        var entity = CreateTestEntity(ConsentTermType.TermsOfUse, expectedVersion, "Content", DateTimeOffset.UtcNow);

        // Act
        LogAct("Creating ConsentTermDataModel from ConsentTerm entity");
        var dataModel = ConsentTermDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying Version mapping");
        dataModel.Version.ShouldBe(expectedVersion);
    }

    [Fact]
    public void Create_ShouldMapContentCorrectly()
    {
        // Arrange
        LogArrange("Creating ConsentTerm entity with known Content");
        string expectedContent = Faker.Lorem.Paragraph();
        var entity = CreateTestEntity(ConsentTermType.TermsOfUse, "1.0", expectedContent, DateTimeOffset.UtcNow);

        // Act
        LogAct("Creating ConsentTermDataModel from ConsentTerm entity");
        var dataModel = ConsentTermDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying Content mapping");
        dataModel.Content.ShouldBe(expectedContent);
    }

    [Fact]
    public void Create_ShouldMapPublishedAtCorrectly()
    {
        // Arrange
        LogArrange("Creating ConsentTerm entity with known PublishedAt");
        var expectedPublishedAt = DateTimeOffset.UtcNow.AddDays(-7);
        var entity = CreateTestEntity(ConsentTermType.TermsOfUse, "1.0", "Content", expectedPublishedAt);

        // Act
        LogAct("Creating ConsentTermDataModel from ConsentTerm entity");
        var dataModel = ConsentTermDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying PublishedAt mapping");
        dataModel.PublishedAt.ShouldBe(expectedPublishedAt);
    }

    [Fact]
    public void Create_ShouldMapBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating ConsentTerm entity with specific EntityInfo values");
        var entityId = Guid.NewGuid();
        var tenantCode = Guid.NewGuid();
        string createdBy = Faker.Person.FullName;
        var createdAt = DateTimeOffset.UtcNow.AddDays(-1);
        long entityVersion = Faker.Random.Long(1);
        string? lastChangedBy = Faker.Person.FullName;
        var lastChangedAt = DateTimeOffset.UtcNow;
        var lastChangedCorrelationId = Guid.NewGuid();
        string lastChangedExecutionOrigin = "TestOrigin";
        string lastChangedBusinessOperationCode = "TEST_OP";

        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(entityId),
            tenantInfo: TenantInfo.Create(tenantCode),
            createdAt: createdAt,
            createdBy: createdBy,
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "UnitTest",
            createdBusinessOperationCode: "TEST_CREATE",
            lastChangedAt: lastChangedAt,
            lastChangedBy: lastChangedBy,
            lastChangedCorrelationId: lastChangedCorrelationId,
            lastChangedExecutionOrigin: lastChangedExecutionOrigin,
            lastChangedBusinessOperationCode: lastChangedBusinessOperationCode,
            entityVersion: RegistryVersion.CreateFromExistingInfo(entityVersion));

        var entity = ConsentTerm.CreateFromExistingInfo(
            new CreateFromExistingInfoConsentTermInput(
                entityInfo,
                ConsentTermType.TermsOfUse,
                "1.0",
                "Content",
                DateTimeOffset.UtcNow));

        // Act
        LogAct("Creating ConsentTermDataModel from ConsentTerm entity");
        var dataModel = ConsentTermDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying base fields from EntityInfo");
        dataModel.Id.ShouldBe(entityId);
        dataModel.TenantCode.ShouldBe(tenantCode);
        dataModel.CreatedBy.ShouldBe(createdBy);
        dataModel.CreatedAt.ShouldBe(createdAt);
        dataModel.EntityVersion.ShouldBe(entityVersion);
        dataModel.LastChangedBy.ShouldBe(lastChangedBy);
        dataModel.LastChangedAt.ShouldBe(lastChangedAt);
        dataModel.LastChangedCorrelationId.ShouldBe(lastChangedCorrelationId);
        dataModel.LastChangedExecutionOrigin.ShouldBe(lastChangedExecutionOrigin);
        dataModel.LastChangedBusinessOperationCode.ShouldBe(lastChangedBusinessOperationCode);
    }

    #region Helper Methods

    private static ConsentTerm CreateTestEntity(
        ConsentTermType type,
        string termVersion,
        string content,
        DateTimeOffset publishedAt)
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

        return ConsentTerm.CreateFromExistingInfo(
            new CreateFromExistingInfoConsentTermInput(entityInfo, type, termVersion, content, publishedAt));
    }

    #endregion
}
