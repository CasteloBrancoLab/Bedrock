using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Interfaces;
using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.DataModelsRepositories;

public class RefreshTokenDataModelRepositoryTests : TestBase
{
    public RefreshTokenDataModelRepositoryTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Arrange
        LogArrange("Criando mocks para dependencias");
        var loggerMock = new Mock<ILogger<RefreshTokenDataModelRepository>>();
        var unitOfWorkMock = new Mock<IAuthPostgreSqlUnitOfWork>();
        var mapperMock = new Mock<IDataModelMapper<RefreshTokenDataModel>>();

        // Act
        LogAct("Criando instancia do repository");
        var repository = new RefreshTokenDataModelRepository(
            loggerMock.Object,
            unitOfWorkMock.Object,
            mapperMock.Object);

        // Assert
        LogAssert("Verificando que instancia foi criada");
        repository.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_ShouldExtendDataModelRepositoryBase()
    {
        // Arrange
        LogArrange("Criando mocks para dependencias");
        var loggerMock = new Mock<ILogger<RefreshTokenDataModelRepository>>();
        var unitOfWorkMock = new Mock<IAuthPostgreSqlUnitOfWork>();
        var mapperMock = new Mock<IDataModelMapper<RefreshTokenDataModel>>();

        // Act
        LogAct("Criando instancia do repository");
        var repository = new RefreshTokenDataModelRepository(
            loggerMock.Object,
            unitOfWorkMock.Object,
            mapperMock.Object);

        // Assert
        LogAssert("Verificando que herda de DataModelRepositoryBase");
        repository.ShouldBeAssignableTo<DataModelRepositoryBase<RefreshTokenDataModel>>();
    }

    [Fact]
    public void Constructor_ShouldImplementIRefreshTokenDataModelRepository()
    {
        // Arrange
        LogArrange("Criando mocks para dependencias");
        var loggerMock = new Mock<ILogger<RefreshTokenDataModelRepository>>();
        var unitOfWorkMock = new Mock<IAuthPostgreSqlUnitOfWork>();
        var mapperMock = new Mock<IDataModelMapper<RefreshTokenDataModel>>();

        // Act
        LogAct("Criando instancia do repository");
        var repository = new RefreshTokenDataModelRepository(
            loggerMock.Object,
            unitOfWorkMock.Object,
            mapperMock.Object);

        // Assert
        LogAssert("Verificando que implementa IRefreshTokenDataModelRepository");
        repository.ShouldBeAssignableTo<IRefreshTokenDataModelRepository>();
    }
}
