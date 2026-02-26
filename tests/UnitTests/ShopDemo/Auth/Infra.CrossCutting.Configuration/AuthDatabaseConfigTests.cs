using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Infra.CrossCutting.Configuration;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.CrossCutting.Configuration;

public class AuthDatabaseConfigTests : TestBase
{
    public AuthDatabaseConfigTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        LogArrange("Criando AuthDatabaseConfig com valores padrao");
        LogAct("Instanciando AuthDatabaseConfig");
        var config = new AuthDatabaseConfig();

        // Assert
        LogAssert("Verificando valores padrao");
        config.ConnectionString.ShouldBe(string.Empty);
        config.CommandTimeout.ShouldBe(30);
    }

    [Fact]
    public void ConnectionString_SetAndGet_ShouldReturnValue()
    {
        // Arrange
        LogArrange("Criando AuthDatabaseConfig");
        var config = new AuthDatabaseConfig();

        // Act
        LogAct("Definindo ConnectionString");
        config.ConnectionString = "Host=localhost;Database=test";

        // Assert
        LogAssert("Verificando valor");
        config.ConnectionString.ShouldBe("Host=localhost;Database=test");
    }

    [Fact]
    public void CommandTimeout_SetAndGet_ShouldReturnValue()
    {
        // Arrange
        LogArrange("Criando AuthDatabaseConfig");
        var config = new AuthDatabaseConfig();

        // Act
        LogAct("Definindo CommandTimeout");
        config.CommandTimeout = 120;

        // Assert
        LogAssert("Verificando valor");
        config.CommandTimeout.ShouldBe(120);
    }

    [Fact]
    public void AuthDatabaseConfig_ShouldBeSealed()
    {
        // Arrange
        LogArrange("Obtendo tipo AuthDatabaseConfig");
        var type = typeof(AuthDatabaseConfig);

        // Act
        LogAct("Verificando se classe e sealed");
        var isSealed = type.IsSealed;

        // Assert
        LogAssert("Verificando que classe e sealed");
        isSealed.ShouldBeTrue();
    }
}
