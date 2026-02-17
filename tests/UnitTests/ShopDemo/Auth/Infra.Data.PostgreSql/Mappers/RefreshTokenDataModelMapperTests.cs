using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Mappers;

public class RefreshTokenDataModelMapperTests : TestBase
{
    public RefreshTokenDataModelMapperTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void RefreshTokenDataModelMapper_ShouldExtendDataModelMapperBase()
    {
        // Arrange & Act
        LogArrange("Verificando hierarquia de tipos");
        var mapperType = typeof(RefreshTokenDataModelMapper);

        // Assert
        LogAssert("Verificando que herda de DataModelMapperBase<RefreshTokenDataModel>");
        mapperType.BaseType.ShouldBe(typeof(DataModelMapperBase<RefreshTokenDataModel>));
    }

    [Fact]
    public void RefreshTokenDataModelMapper_ShouldBeSealed()
    {
        // Arrange & Act
        LogArrange("Verificando modificador de classe");
        var mapperType = typeof(RefreshTokenDataModelMapper);

        // Assert
        LogAssert("Verificando que a classe e sealed");
        mapperType.IsSealed.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Arrange & Act
        LogAct("Criando instancia do mapper");
        var mapper = new RefreshTokenDataModelMapper();

        // Assert
        LogAssert("Verificando que instancia foi criada");
        mapper.ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureInternal_ShouldBeProtected()
    {
        // Arrange
        LogArrange("Obtendo metodo ConfigureInternal via reflexao");
        var mapperType = typeof(RefreshTokenDataModelMapper);

        // Act
        var method = mapperType.GetMethod(
            "ConfigureInternal",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Assert
        LogAssert("Verificando que ConfigureInternal e protected");
        method.ShouldNotBeNull();
        method.IsFamily.ShouldBeTrue();
    }

    [Fact]
    public void MapBinaryImporter_ShouldBePublic()
    {
        // Arrange
        LogArrange("Obtendo metodo MapBinaryImporter via reflexao");
        var mapperType = typeof(RefreshTokenDataModelMapper);

        // Act
        var method = mapperType.GetMethod("MapBinaryImporter");

        // Assert
        LogAssert("Verificando que MapBinaryImporter e publico");
        method.ShouldNotBeNull();
        method.IsPublic.ShouldBeTrue();
    }

    [Fact]
    public void TableSchema_ShouldBePublic()
    {
        // Arrange
        LogArrange("Criando mapper");
        var mapper = new RefreshTokenDataModelMapper();

        // Act & Assert
        LogAssert("Verificando que schema e 'public'");
        mapper.TableSchema.ShouldBe("public");
    }

    [Fact]
    public void TableName_ShouldBeAuthRefreshTokens()
    {
        // Arrange
        LogArrange("Criando mapper");
        var mapper = new RefreshTokenDataModelMapper();

        // Act & Assert
        LogAssert("Verificando que table name e 'auth_refresh_tokens'");
        mapper.TableName.ShouldBe("auth_refresh_tokens");
    }

    [Fact]
    public void ColumnMapDictionary_ShouldContainUserIdColumn()
    {
        // Arrange
        LogArrange("Criando mapper");
        var mapper = new RefreshTokenDataModelMapper();

        // Act & Assert
        LogAssert("Verificando que coluna UserId esta mapeada");
        mapper.ColumnMapDictionary.ShouldContainKey("UserId");
    }

    [Fact]
    public void ColumnMapDictionary_ShouldContainTokenHashColumn()
    {
        // Arrange
        LogArrange("Criando mapper");
        var mapper = new RefreshTokenDataModelMapper();

        // Act & Assert
        LogAssert("Verificando que coluna TokenHash esta mapeada");
        mapper.ColumnMapDictionary.ShouldContainKey("TokenHash");
    }

    [Fact]
    public void ColumnMapDictionary_ShouldContainFamilyIdColumn()
    {
        // Arrange
        LogArrange("Criando mapper");
        var mapper = new RefreshTokenDataModelMapper();

        // Act & Assert
        LogAssert("Verificando que coluna FamilyId esta mapeada");
        mapper.ColumnMapDictionary.ShouldContainKey("FamilyId");
    }

    [Fact]
    public void ColumnMapDictionary_ShouldContainExpiresAtColumn()
    {
        // Arrange
        LogArrange("Criando mapper");
        var mapper = new RefreshTokenDataModelMapper();

        // Act & Assert
        LogAssert("Verificando que coluna ExpiresAt esta mapeada");
        mapper.ColumnMapDictionary.ShouldContainKey("ExpiresAt");
    }

    [Fact]
    public void ColumnMapDictionary_ShouldContainStatusColumn()
    {
        // Arrange
        LogArrange("Criando mapper");
        var mapper = new RefreshTokenDataModelMapper();

        // Act & Assert
        LogAssert("Verificando que coluna Status esta mapeada");
        mapper.ColumnMapDictionary.ShouldContainKey("Status");
    }

    [Fact]
    public void ColumnMapDictionary_ShouldContainRevokedAtColumn()
    {
        // Arrange
        LogArrange("Criando mapper");
        var mapper = new RefreshTokenDataModelMapper();

        // Act & Assert
        LogAssert("Verificando que coluna RevokedAt esta mapeada");
        mapper.ColumnMapDictionary.ShouldContainKey("RevokedAt");
    }

    [Fact]
    public void ColumnMapDictionary_ShouldContainReplacedByTokenIdColumn()
    {
        // Arrange
        LogArrange("Criando mapper");
        var mapper = new RefreshTokenDataModelMapper();

        // Act & Assert
        LogAssert("Verificando que coluna ReplacedByTokenId esta mapeada");
        mapper.ColumnMapDictionary.ShouldContainKey("ReplacedByTokenId");
    }
}
