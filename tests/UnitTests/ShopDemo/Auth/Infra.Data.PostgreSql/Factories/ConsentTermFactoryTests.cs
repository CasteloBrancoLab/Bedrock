using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.ConsentTerms.Enums;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class ConsentTermFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public ConsentTermFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Theory]
    [InlineData((short)1, ConsentTermType.TermsOfUse)]
    [InlineData((short)2, ConsentTermType.PrivacyPolicy)]
    [InlineData((short)3, ConsentTermType.Marketing)]
    public void Create_ShouldMapTypeFromDataModel(short typeValue, ConsentTermType expectedType)
    {
        // Arrange
        LogArrange($"Creating ConsentTermDataModel with type value {typeValue}");
        var dataModel = CreateTestDataModel(typeValue, "1.0", "Content", DateTimeOffset.UtcNow);

        // Act
        LogAct("Creating ConsentTerm from ConsentTermDataModel");
        var entity = ConsentTermFactory.Create(dataModel);

        // Assert
        LogAssert($"Verifying Type mapped to {expectedType}");
        entity.Type.ShouldBe(expectedType);
    }

    [Fact]
    public void Create_ShouldMapVersionFromDataModel()
    {
        // Arrange
        LogArrange("Creating ConsentTermDataModel with specific Version");
        string expectedVersion = "3.0";
        var dataModel = CreateTestDataModel((short)ConsentTermType.TermsOfUse, expectedVersion, "Content", DateTimeOffset.UtcNow);

        // Act
        LogAct("Creating ConsentTerm from ConsentTermDataModel");
        var entity = ConsentTermFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying TermVersion mapping");
        entity.TermVersion.ShouldBe(expectedVersion);
    }

    [Fact]
    public void Create_ShouldMapContentFromDataModel()
    {
        // Arrange
        LogArrange("Creating ConsentTermDataModel with specific Content");
        string expectedContent = Faker.Lorem.Paragraph();
        var dataModel = CreateTestDataModel((short)ConsentTermType.TermsOfUse, "1.0", expectedContent, DateTimeOffset.UtcNow);

        // Act
        LogAct("Creating ConsentTerm from ConsentTermDataModel");
        var entity = ConsentTermFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying Content mapping");
        entity.Content.ShouldBe(expectedContent);
    }

    [Fact]
    public void Create_ShouldMapPublishedAtFromDataModel()
    {
        // Arrange
        LogArrange("Creating ConsentTermDataModel with specific PublishedAt");
        var expectedPublishedAt = DateTimeOffset.UtcNow.AddDays(-14);
        var dataModel = CreateTestDataModel((short)ConsentTermType.TermsOfUse, "1.0", "Content", expectedPublishedAt);

        // Act
        LogAct("Creating ConsentTerm from ConsentTermDataModel");
        var entity = ConsentTermFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying PublishedAt mapping");
        entity.PublishedAt.ShouldBe(expectedPublishedAt);
    }

    [Fact]
    public void Create_ShouldMapEntityInfoFieldsFromDataModel()
    {
        // Arrange
        LogArrange("Creating ConsentTermDataModel with specific base fields");
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

        var dataModel = new ConsentTermDataModel
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
            Type = (short)ConsentTermType.TermsOfUse,
            Version = "1.0",
            Content = "Content",
            PublishedAt = DateTimeOffset.UtcNow
        };

        // Act
        LogAct("Creating ConsentTerm from ConsentTermDataModel");
        var entity = ConsentTermFactory.Create(dataModel);

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
        LogArrange("Creating ConsentTermDataModel with null last-changed fields");
        var dataModel = new ConsentTermDataModel
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
            Type = (short)ConsentTermType.TermsOfUse,
            Version = "1.0",
            Content = "Content",
            PublishedAt = DateTimeOffset.UtcNow
        };

        // Act
        LogAct("Creating ConsentTerm from ConsentTermDataModel with nulls");
        var entity = ConsentTermFactory.Create(dataModel);

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
        LogArrange("Creating ConsentTermDataModel to verify CreatedCorrelationId is mapped");
        var dataModel = CreateTestDataModel((short)ConsentTermType.TermsOfUse, "1.0", "Content", DateTimeOffset.UtcNow);

        // Act
        LogAct("Creating ConsentTerm from ConsentTermDataModel");
        var entity = ConsentTermFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedCorrelationId matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedCorrelationId.ShouldBe(dataModel.CreatedCorrelationId);
    }

    [Fact]
    public void Create_ShouldMapCreatedExecutionOriginFromDataModel()
    {
        // Arrange
        LogArrange("Creating ConsentTermDataModel to verify CreatedExecutionOrigin is mapped");
        var dataModel = CreateTestDataModel((short)ConsentTermType.TermsOfUse, "1.0", "Content", DateTimeOffset.UtcNow);

        // Act
        LogAct("Creating ConsentTerm from ConsentTermDataModel");
        var entity = ConsentTermFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedExecutionOrigin matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedExecutionOrigin.ShouldBe(dataModel.CreatedExecutionOrigin);
    }

    [Fact]
    public void Create_ShouldMapCreatedBusinessOperationCodeFromDataModel()
    {
        // Arrange
        LogArrange("Creating ConsentTermDataModel to verify CreatedBusinessOperationCode is mapped");
        var dataModel = CreateTestDataModel((short)ConsentTermType.TermsOfUse, "1.0", "Content", DateTimeOffset.UtcNow);

        // Act
        LogAct("Creating ConsentTerm from ConsentTermDataModel");
        var entity = ConsentTermFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedBusinessOperationCode matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedBusinessOperationCode.ShouldBe(dataModel.CreatedBusinessOperationCode);
    }

    #region Helper Methods

    private static ConsentTermDataModel CreateTestDataModel(
        short type,
        string version,
        string content,
        DateTimeOffset publishedAt)
    {
        return new ConsentTermDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "UnitTest",
            CreatedBusinessOperationCode = "CREATE_CONSENT_TERM",
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedExecutionOrigin = null,
            LastChangedCorrelationId = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            Type = type,
            Version = version,
            Content = content,
            PublishedAt = publishedAt
        };
    }

    #endregion
}
