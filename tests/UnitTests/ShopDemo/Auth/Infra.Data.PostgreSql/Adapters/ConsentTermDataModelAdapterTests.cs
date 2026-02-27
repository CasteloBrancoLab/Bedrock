using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.ConsentTerms;
using ShopDemo.Auth.Domain.Entities.ConsentTerms.Enums;
using ShopDemo.Auth.Domain.Entities.ConsentTerms.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Adapters;

public class ConsentTermDataModelAdapterTests : TestBase
{
    private static readonly Faker Faker = new();

    public ConsentTermDataModelAdapterTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Adapt_ShouldUpdateTypeFromEntity()
    {
        // Arrange
        LogArrange("Creating ConsentTermDataModel and ConsentTerm with different Types");
        var dataModel = CreateTestDataModel();
        dataModel.Type = (short)ConsentTermType.TermsOfUse;
        var entity = CreateTestEntity(ConsentTermType.PrivacyPolicy, "1.0", "Content", DateTimeOffset.UtcNow);

        // Act
        LogAct("Adapting data model from entity");
        ConsentTermDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying Type was updated");
        dataModel.Type.ShouldBe((short)ConsentTermType.PrivacyPolicy);
    }

    [Fact]
    public void Adapt_ShouldUpdateVersionFromEntity()
    {
        // Arrange
        LogArrange("Creating ConsentTermDataModel and ConsentTerm with different Versions");
        var dataModel = CreateTestDataModel();
        string expectedVersion = "5.0";
        var entity = CreateTestEntity(ConsentTermType.TermsOfUse, expectedVersion, "Content", DateTimeOffset.UtcNow);

        // Act
        LogAct("Adapting data model from entity");
        ConsentTermDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying Version was updated");
        dataModel.Version.ShouldBe(expectedVersion);
    }

    [Fact]
    public void Adapt_ShouldUpdateContentFromEntity()
    {
        // Arrange
        LogArrange("Creating ConsentTermDataModel and ConsentTerm with different Contents");
        var dataModel = CreateTestDataModel();
        string expectedContent = Faker.Lorem.Paragraph();
        var entity = CreateTestEntity(ConsentTermType.TermsOfUse, "1.0", expectedContent, DateTimeOffset.UtcNow);

        // Act
        LogAct("Adapting data model from entity");
        ConsentTermDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying Content was updated");
        dataModel.Content.ShouldBe(expectedContent);
    }

    [Fact]
    public void Adapt_ShouldUpdatePublishedAtFromEntity()
    {
        // Arrange
        LogArrange("Creating ConsentTermDataModel and ConsentTerm with different PublishedAt values");
        var dataModel = CreateTestDataModel();
        var expectedPublishedAt = DateTimeOffset.UtcNow.AddDays(-30);
        var entity = CreateTestEntity(ConsentTermType.TermsOfUse, "1.0", "Content", expectedPublishedAt);

        // Act
        LogAct("Adapting data model from entity");
        ConsentTermDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying PublishedAt was updated");
        dataModel.PublishedAt.ShouldBe(expectedPublishedAt);
    }

    [Fact]
    public void Adapt_ShouldUpdateBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating ConsentTermDataModel and ConsentTerm with different EntityInfo values");
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

        var entity = ConsentTerm.CreateFromExistingInfo(
            new CreateFromExistingInfoConsentTermInput(
                entityInfo,
                ConsentTermType.TermsOfUse,
                "1.0",
                "Content",
                DateTimeOffset.UtcNow));

        // Act
        LogAct("Adapting data model from entity");
        ConsentTermDataModelAdapter.Adapt(dataModel, entity);

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
        LogArrange("Creating ConsentTermDataModel and ConsentTerm");
        var dataModel = CreateTestDataModel();
        var entity = CreateTestEntity(ConsentTermType.TermsOfUse, "1.0", "Content", DateTimeOffset.UtcNow);

        // Act
        LogAct("Adapting data model from entity");
        var result = ConsentTermDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying the same instance is returned");
        result.ShouldBeSameAs(dataModel);
    }

    #region Helper Methods

    private static ConsentTermDataModel CreateTestDataModel()
    {
        return new ConsentTermDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            EntityVersion = 1,
            Type = (short)ConsentTermType.TermsOfUse,
            Version = "1.0",
            Content = "Initial content",
            PublishedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };
    }

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
