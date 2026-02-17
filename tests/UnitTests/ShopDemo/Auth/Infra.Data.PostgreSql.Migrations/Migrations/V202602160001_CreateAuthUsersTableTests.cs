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

public class V202602160001_CreateAuthUsersTableTests : TestBase
{
    public V202602160001_CreateAuthUsersTableTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void ShouldHaveMigrationAttribute_WithCorrectVersion()
    {
        // Arrange
        LogArrange("Obtendo atributo Migration via reflection");

        // Act
        LogAct("Lendo versao do atributo");
        var attribute = typeof(V202602160001_CreateAuthUsersTable)
            .GetCustomAttribute<MigrationAttribute>();

        // Assert
        LogAssert("Verificando versao 202602160001");
        attribute.ShouldNotBeNull();
        attribute.Version.ShouldBe(202602160001);
    }

    [Fact]
    public void ShouldHaveSqlScriptAttribute_WithUpAndDown()
    {
        // Arrange
        LogArrange("Obtendo atributo SqlScript via reflection");

        // Act
        LogAct("Lendo paths dos scripts");
        var attribute = typeof(V202602160001_CreateAuthUsersTable)
            .GetCustomAttribute<SqlScriptAttribute>();

        // Assert
        LogAssert("Verificando paths Up e Down");
        attribute.ShouldNotBeNull();
        attribute.UpScriptResourceName.ShouldBe("Up/V202602160001__create_auth_users_table.sql");
        attribute.DownScriptResourceName.ShouldBe("Down/V202602160001__create_auth_users_table.sql");
    }

    [Fact]
    public void Constructor_ShouldSucceed()
    {
        // Arrange
        LogArrange("Preparando instanciacao da migration");

        // Act
        LogAct("Criando instancia de V202602160001_CreateAuthUsersTable");
        var migration = new V202602160001_CreateAuthUsersTable();

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
        var inherits = typeof(V202602160001_CreateAuthUsersTable)
            .IsSubclassOf(typeof(SqlScriptMigrationBase));

        // Assert
        LogAssert("Verificando heranca de SqlScriptMigrationBase");
        inherits.ShouldBeTrue();
    }
}
