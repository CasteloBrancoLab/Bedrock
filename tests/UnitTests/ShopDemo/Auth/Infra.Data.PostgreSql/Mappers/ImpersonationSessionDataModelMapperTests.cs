using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Mappers;

public class ImpersonationSessionDataModelMapperTests : TestBase
{
    public ImpersonationSessionDataModelMapperTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void ImpersonationSessionDataModelMapper_ShouldInheritFromDataModelMapperBase()
    {
        // Arrange
        LogArrange("Checking ImpersonationSessionDataModelMapper type hierarchy");

        // Act
        LogAct("Inspecting type inheritance");
        var mapperType = typeof(ImpersonationSessionDataModelMapper);

        // Assert
        LogAssert("Verifying ImpersonationSessionDataModelMapper inherits from DataModelMapperBase<ImpersonationSessionDataModel>");
        mapperType.BaseType.ShouldBe(typeof(DataModelMapperBase<ImpersonationSessionDataModel>));
    }

    [Fact]
    public void ImpersonationSessionDataModelMapper_ShouldBeSealed()
    {
        // Arrange
        LogArrange("Checking ImpersonationSessionDataModelMapper type modifiers");

        // Act
        LogAct("Inspecting type modifiers");
        var mapperType = typeof(ImpersonationSessionDataModelMapper);

        // Assert
        LogAssert("Verifying ImpersonationSessionDataModelMapper is sealed");
        mapperType.IsSealed.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        LogArrange("Creating ImpersonationSessionDataModelMapper instance");

        // Act
        LogAct("Constructing ImpersonationSessionDataModelMapper");
        var mapper = new ImpersonationSessionDataModelMapper();

        // Assert
        LogAssert("Verifying mapper was created successfully");
        mapper.ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableSchemaToPublic()
    {
        // Arrange
        LogArrange("Creating ImpersonationSessionDataModelMapper to verify table schema");

        // Act
        LogAct("Constructing ImpersonationSessionDataModelMapper and reading TableSchema");
        var mapper = new ImpersonationSessionDataModelMapper();

        // Assert
        LogAssert("Verifying TableSchema is 'public'");
        mapper.TableSchema.ShouldBe("public");
    }

    [Fact]
    public void ConfigureInternal_ShouldSetTableNameToAuthImpersonationSessions()
    {
        // Arrange
        LogArrange("Creating ImpersonationSessionDataModelMapper to verify table name");

        // Act
        LogAct("Constructing ImpersonationSessionDataModelMapper and reading TableName");
        var mapper = new ImpersonationSessionDataModelMapper();

        // Assert
        LogAssert("Verifying TableName is 'auth_impersonation_sessions'");
        mapper.TableName.ShouldBe("auth_impersonation_sessions");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapOperatorUserIdColumn()
    {
        // Arrange
        LogArrange("Creating ImpersonationSessionDataModelMapper to verify OperatorUserId column mapping");

        // Act
        LogAct("Constructing ImpersonationSessionDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ImpersonationSessionDataModelMapper();

        // Assert
        LogAssert("Verifying OperatorUserId column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("OperatorUserId");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapTargetUserIdColumn()
    {
        // Arrange
        LogArrange("Creating ImpersonationSessionDataModelMapper to verify TargetUserId column mapping");

        // Act
        LogAct("Constructing ImpersonationSessionDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ImpersonationSessionDataModelMapper();

        // Assert
        LogAssert("Verifying TargetUserId column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("TargetUserId");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapExpiresAtColumn()
    {
        // Arrange
        LogArrange("Creating ImpersonationSessionDataModelMapper to verify ExpiresAt column mapping");

        // Act
        LogAct("Constructing ImpersonationSessionDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ImpersonationSessionDataModelMapper();

        // Assert
        LogAssert("Verifying ExpiresAt column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("ExpiresAt");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapStatusColumn()
    {
        // Arrange
        LogArrange("Creating ImpersonationSessionDataModelMapper to verify Status column mapping");

        // Act
        LogAct("Constructing ImpersonationSessionDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ImpersonationSessionDataModelMapper();

        // Assert
        LogAssert("Verifying Status column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("Status");
    }

    [Fact]
    public void ConfigureInternal_ShouldMapEndedAtColumn()
    {
        // Arrange
        LogArrange("Creating ImpersonationSessionDataModelMapper to verify EndedAt column mapping");

        // Act
        LogAct("Constructing ImpersonationSessionDataModelMapper and reading ColumnMapDictionary");
        var mapper = new ImpersonationSessionDataModelMapper();

        // Assert
        LogAssert("Verifying EndedAt column is mapped");
        mapper.ColumnMapDictionary.ShouldContainKey("EndedAt");
    }
}
