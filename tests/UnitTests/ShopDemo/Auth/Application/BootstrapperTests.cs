using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.DependencyInjection;
using ShopDemo.Auth.Application;
using ShopDemo.Auth.Application.UseCases.AuthenticateUser.Interfaces;
using ShopDemo.Auth.Application.UseCases.RegisterUser.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Application;

public class BootstrapperTests : TestBase
{
    public BootstrapperTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void ConfigureServices_ShouldRegisterRegisterUserUseCase()
    {
        // Arrange
        LogArrange("Criando service collection");
        var services = new ServiceCollection();

        // Act
        LogAct("Chamando Bootstrapper.ConfigureServices");
        Bootstrapper.ConfigureServices(services);

        // Assert
        LogAssert("Verificando que IRegisterUserUseCase esta registrado como scoped");
        var descriptor = services.SingleOrDefault(d =>
            d.ServiceType == typeof(IRegisterUserUseCase));
        descriptor.ShouldNotBeNull();
        descriptor.Lifetime.ShouldBe(ServiceLifetime.Scoped);
    }

    [Fact]
    public void ConfigureServices_ShouldRegisterAuthenticateUserUseCase()
    {
        // Arrange
        LogArrange("Criando service collection");
        var services = new ServiceCollection();

        // Act
        LogAct("Chamando Bootstrapper.ConfigureServices");
        Bootstrapper.ConfigureServices(services);

        // Assert
        LogAssert("Verificando que IAuthenticateUserUseCase esta registrado como scoped");
        var descriptor = services.SingleOrDefault(d =>
            d.ServiceType == typeof(IAuthenticateUserUseCase));
        descriptor.ShouldNotBeNull();
        descriptor.Lifetime.ShouldBe(ServiceLifetime.Scoped);
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
        LogAssert("Verificando que nao duplicou registros (TryAddScoped)");
        services.Count(d => d.ServiceType == typeof(IRegisterUserUseCase)).ShouldBe(1);
        services.Count(d => d.ServiceType == typeof(IAuthenticateUserUseCase)).ShouldBe(1);
    }
}
