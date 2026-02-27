using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Mappers;

public class TokenExchangeDataModelMapperTests : TestBase
{
    public TokenExchangeDataModelMapperTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void TokenExchangeDataModelMapper_ShouldInheritFromDataModelMapperBase()
    {
        // Arrange
        LogArrange("Checking TokenExchangeDataModelMapper type hierarchy");

        // Act
        LogAct("Inspecting type inheritance");
        var mapperType = typeof(TokenExchangeDataModelMapper);

        // Assert
        LogAssert("Verifying TokenExchangeDataModelMapper inherits from DataModelMapperBase<TokenExchangeDataModel>");
        mapperType.BaseType.ShouldBe(typeof(DataModelMapperBase<TokenExchangeDataModel>));
    }

    [Fact]
    public void TokenExchangeDataModelMapper_ShouldBeSealed()
    {
        // Arrange
        LogArrange("Checking TokenExchangeDataModelMapper type modifiers");

        // Act
        LogAct("Inspecting type modifiers");
        var mapperType = typeof(TokenExchangeDataModelMapper);

        // Assert
        LogAssert("Verifying TokenExchangeDataModelMapper is sealed");
        mapperType.IsSealed.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        LogArrange("Creating TokenExchangeDataModelMapper instance");

        // Act
        LogAct("Constructing TokenExchangeDataModelMapper");
        var mapper = new TokenExchangeDataModelMapper();

        // Assert
        LogAssert("Verifying mapper was created successfully");
        mapper.ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableSchemaToPublic()
    {
        // Arrange
        LogArrange("Creating TokenExchangeDataModelMapper to verify table schema");

        // Act
        LogAct("Constructing TokenExchangeDataModelMapper and reading TableSchema");
        var mapper = new TokenExchangeDataModelMapper();

        // Assert
        LogAssert("Verifying TableSchema is 'public'");
        mapper.TableSchema.ShouldBe("public");
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableNameToAuthTokenExchanges()
    {
        // Arrange
        LogArrange("Creating TokenExchangeDataModelMapper to verify table name");

        // Act
        LogAct("Constructing TokenExchangeDataModelMapper and reading TableName");
        var mapper = new TokenExchangeDataModelMapper();

        // Assert
        LogAssert("Verifying TableName is 'auth_token_exchanges'");
        mapper.TableName.ShouldBe("auth_token_exchanges");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapUserIdColumn()
    {
        // Arrange
        LogArrange("Creating TokenExchangeDataModelMapper to verify UserId column mapping");

        // Act
        LogAct("Constructing TokenExchangeDataModelMapper and reading ColumnMapDictionary");
        var mapper = new TokenExchangeDataModelMapper();

        // Assert
        LogAssert("Verifying UserId column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("UserId");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapSubjectTokenJtiColumn()
    {
        // Arrange
        LogArrange("Creating TokenExchangeDataModelMapper to verify SubjectTokenJti column mapping");

        // Act
        LogAct("Constructing TokenExchangeDataModelMapper and reading ColumnMapDictionary");
        var mapper = new TokenExchangeDataModelMapper();

        // Assert
        LogAssert("Verifying SubjectTokenJti column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("SubjectTokenJti");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapRequestedAudienceColumn()
    {
        // Arrange
        LogArrange("Creating TokenExchangeDataModelMapper to verify RequestedAudience column mapping");

        // Act
        LogAct("Constructing TokenExchangeDataModelMapper and reading ColumnMapDictionary");
        var mapper = new TokenExchangeDataModelMapper();

        // Assert
        LogAssert("Verifying RequestedAudience column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("RequestedAudience");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapIssuedTokenJtiColumn()
    {
        // Arrange
        LogArrange("Creating TokenExchangeDataModelMapper to verify IssuedTokenJti column mapping");

        // Act
        LogAct("Constructing TokenExchangeDataModelMapper and reading ColumnMapDictionary");
        var mapper = new TokenExchangeDataModelMapper();

        // Assert
        LogAssert("Verifying IssuedTokenJti column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("IssuedTokenJti");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapIssuedAtColumn()
    {
        // Arrange
        LogArrange("Creating TokenExchangeDataModelMapper to verify IssuedAt column mapping");

        // Act
        LogAct("Constructing TokenExchangeDataModelMapper and reading ColumnMapDictionary");
        var mapper = new TokenExchangeDataModelMapper();

        // Assert
        LogAssert("Verifying IssuedAt column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("IssuedAt");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapExpiresAtColumn()
    {
        // Arrange
        LogArrange("Creating TokenExchangeDataModelMapper to verify ExpiresAt column mapping");

        // Act
        LogAct("Constructing TokenExchangeDataModelMapper and reading ColumnMapDictionary");
        var mapper = new TokenExchangeDataModelMapper();

        // Assert
        LogAssert("Verifying ExpiresAt column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("ExpiresAt");
    }
}
