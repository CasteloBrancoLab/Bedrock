using System.Reflection;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations.Attributes;
using Bedrock.BuildingBlocks.Testing;
using FluentMigrator;
using ShopDemo.Auth.Infra.Data.PostgreSql.Migrations.Migrations;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Migrations.Migrations;

public class V202602180001_CreateAuthTablesTests : TestBase
{
    public V202602180001_CreateAuthTablesTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void ShouldHaveMigrationAttribute_WithCorrectVersion()
    {
        // Arrange
        LogArrange("Obtendo atributo Migration via reflection");

        // Act
        LogAct("Lendo versao do atributo");
        var attribute = typeof(V202602180001_CreateAuthTables)
            .GetCustomAttribute<MigrationAttribute>();

        // Assert
        LogAssert("Verificando versao 202602180001");
        attribute.ShouldNotBeNull();
        attribute.Version.ShouldBe(202602180001);
    }

    [Fact]
    public void ShouldHaveSqlScriptAttribute_WithUpAndDown()
    {
        // Arrange
        LogArrange("Obtendo atributo SqlScript via reflection");

        // Act
        LogAct("Lendo paths dos scripts");
        var attribute = typeof(V202602180001_CreateAuthTables)
            .GetCustomAttribute<SqlScriptAttribute>();

        // Assert
        LogAssert("Verificando paths Up e Down");
        attribute.ShouldNotBeNull();
        attribute.UpScriptResourceName.ShouldBe("Up/V202602180001__create_auth_tables.sql");
        attribute.DownScriptResourceName.ShouldBe("Down/V202602180001__create_auth_tables.sql");
    }

    [Fact]
    public void Constructor_ShouldSucceed()
    {
        // Arrange
        LogArrange("Preparando instanciacao da migration");

        // Act
        LogAct("Criando instancia de V202602180001_CreateAuthTables");
        var migration = new V202602180001_CreateAuthTables();

        // Assert
        LogAssert("Verificando que a migration foi criada (embedded scripts existem)");
        migration.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldInheritFromSqlScriptMigrationBase()
    {
        // Arrange
        LogArrange("Verificando hierarquia de heranca");

        // Act
        LogAct("Checando tipo base");
        var inherits = typeof(V202602180001_CreateAuthTables)
            .IsSubclassOf(typeof(SqlScriptMigrationBase));

        // Assert
        LogAssert("Verificando heranca de SqlScriptMigrationBase");
        inherits.ShouldBeTrue();
    }
}
