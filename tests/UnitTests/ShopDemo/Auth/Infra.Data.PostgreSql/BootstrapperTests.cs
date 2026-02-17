using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Interfaces;
using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.DependencyInjection;
using ShopDemo.Auth.Infra.Data.PostgreSql;
using ShopDemo.Auth.Infra.Data.PostgreSql.Connections.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql;

public class BootstrapperTests : TestBase
{
    public BootstrapperTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void ConfigureServices_ShouldRegisterMapper()
    {
        // Arrange
        LogArrange("Criando service collection");
        var services = new ServiceCollection();

        // Act
        LogAct("Chamando Bootstrapper.ConfigureServices");
        Bootstrapper.ConfigureServices(services);

        // Assert
        LogAssert("Verificando que mapper esta registrado como singleton");
        var descriptor = services.SingleOrDefault(d =>
            d.ServiceType == typeof(IDataModelMapper<UserDataModel>));
        descriptor.ShouldNotBeNull();
        descriptor.Lifetime.ShouldBe(ServiceLifetime.Singleton);
    }

    [Fact]
    public void ConfigureServices_ShouldRegisterConnection()
    {
        // Arrange
        LogArrange("Criando service collection");
        var services = new ServiceCollection();

        // Act
        LogAct("Chamando Bootstrapper.ConfigureServices");
        Bootstrapper.ConfigureServices(services);

        // Assert
        LogAssert("Verificando que connection esta registrada como scoped");
        var descriptor = services.SingleOrDefault(d =>
            d.ServiceType == typeof(IAuthPostgreSqlConnection));
        descriptor.ShouldNotBeNull();
        descriptor.Lifetime.ShouldBe(ServiceLifetime.Scoped);
    }

    [Fact]
    public void ConfigureServices_ShouldRegisterUnitOfWork()
    {
        // Arrange
        LogArrange("Criando service collection");
        var services = new ServiceCollection();

        // Act
        LogAct("Chamando Bootstrapper.ConfigureServices");
        Bootstrapper.ConfigureServices(services);

        // Assert
        LogAssert("Verificando que unit of work esta registrado como scoped");
        var descriptor = services.SingleOrDefault(d =>
            d.ServiceType == typeof(IAuthPostgreSqlUnitOfWork));
        descriptor.ShouldNotBeNull();
        descriptor.Lifetime.ShouldBe(ServiceLifetime.Scoped);
    }

    [Fact]
    public void ConfigureServices_ShouldRegisterDataModelRepository()
    {
        // Arrange
        LogArrange("Criando service collection");
        var services = new ServiceCollection();

        // Act
        LogAct("Chamando Bootstrapper.ConfigureServices");
        Bootstrapper.ConfigureServices(services);

        // Assert
        LogAssert("Verificando que data model repository esta registrado como scoped");
        var descriptor = services.SingleOrDefault(d =>
            d.ServiceType == typeof(IUserDataModelRepository));
        descriptor.ShouldNotBeNull();
        descriptor.Lifetime.ShouldBe(ServiceLifetime.Scoped);
    }

    [Fact]
    public void ConfigureServices_ShouldRegisterPostgreSqlRepository()
    {
        // Arrange
        LogArrange("Criando service collection");
        var services = new ServiceCollection();

        // Act
        LogAct("Chamando Bootstrapper.ConfigureServices");
        Bootstrapper.ConfigureServices(services);

        // Assert
        LogAssert("Verificando que repositorio PostgreSql esta registrado como scoped");
        var descriptor = services.SingleOrDefault(d =>
            d.ServiceType == typeof(IUserPostgreSqlRepository));
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
        LogAssert("Verificando que nao duplicou registros (TryAdd)");
        services.Count(d => d.ServiceType == typeof(IDataModelMapper<UserDataModel>)).ShouldBe(1);
        services.Count(d => d.ServiceType == typeof(IAuthPostgreSqlConnection)).ShouldBe(1);
        services.Count(d => d.ServiceType == typeof(IAuthPostgreSqlUnitOfWork)).ShouldBe(1);
        services.Count(d => d.ServiceType == typeof(IUserDataModelRepository)).ShouldBe(1);
        services.Count(d => d.ServiceType == typeof(IUserPostgreSqlRepository)).ShouldBe(1);
    }
}
