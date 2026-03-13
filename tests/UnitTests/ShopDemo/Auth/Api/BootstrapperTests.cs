using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Web.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ShopDemo.Auth.Api;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Api;

public class BootstrapperTests : TestBase
{
    private readonly StartupInfo _startupInfo;

    public BootstrapperTests(ITestOutputHelper output) : base(output)
    {
        BedrockHost.ConfigureDefaults(out _startupInfo);
    }

    [Fact]
    public void ConfigureServices_ShouldAddControllers()
    {
        // Arrange
        LogArrange("Criando service collection");
        var services = new ServiceCollection();

        // Act
        LogAct("Chamando Bootstrapper.ConfigureServices");
        Bootstrapper.ConfigureServices(services, _startupInfo);

        // Assert
        LogAssert("Verificando que controllers foram adicionados");
        services.ShouldContain(d => d.ServiceType.FullName!.Contains("IControllerActivator"));
    }

    [Fact]
    public void ConfigureServices_ShouldReturnServiceCollectionForChaining()
    {
        // Arrange
        LogArrange("Criando service collection");
        var services = new ServiceCollection();

        // Act
        LogAct("Chamando Bootstrapper.ConfigureServices");
        var result = Bootstrapper.ConfigureServices(services, _startupInfo);

        // Assert
        LogAssert("Verificando que retorna a mesma instancia para encadeamento");
        result.ShouldBeSameAs(services);
    }
}
