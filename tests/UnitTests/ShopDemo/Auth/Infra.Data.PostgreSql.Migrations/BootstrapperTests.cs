using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.DependencyInjection;
using ShopDemo.Auth.Infra.Data.PostgreSql.Migrations;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Migrations;

public class BootstrapperTests : TestBase
{
    public BootstrapperTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void ConfigureServices_ShouldRegisterMigrationManager()
    {
        // Arrange
        LogArrange("Criando service collection");
        var services = new ServiceCollection();

        // Act
        LogAct("Chamando Bootstrapper.ConfigureServices");
        Bootstrapper.ConfigureServices(services);

        // Assert
        LogAssert("Verificando que AuthMigrationManager esta registrado como singleton");
        var descriptor = services.SingleOrDefault(d =>
            d.ServiceType == typeof(AuthMigrationManager));
        descriptor.ShouldNotBeNull();
        descriptor.Lifetime.ShouldBe(ServiceLifetime.Singleton);
    }

    [Fact]
    public void ConfigureServices_ShouldReturnServiceCollectionForChaining()
    {
        // Arrange
        LogArrange("Criando service collection");
        var services = new ServiceCollection();

        // Act
        LogAct("Chamando Bootstrapper.ConfigureServices");
        var result = Bootstrapper.ConfigureServices(services);

        // Assert
        LogAssert("Verificando que retorna a mesma instancia para encadeamento");
        result.ShouldBeSameAs(services);
    }

    [Fact]
    public void ConfigureServices_CalledTwice_ShouldNotDuplicateRegistrations()
    {
        // Arrange
        LogArrange("Criando service collection");
        var services = new ServiceCollection();

        // Act
        LogAct("Chamando Bootstrapper.ConfigureServices duas vezes");
        Bootstrapper.ConfigureServices(services);
        Bootstrapper.ConfigureServices(services);

        // Assert
        LogAssert("Verificando que nao duplicou registros (TryAdd)");
        services.Count(d => d.ServiceType == typeof(AuthMigrationManager)).ShouldBe(1);
    }
}
