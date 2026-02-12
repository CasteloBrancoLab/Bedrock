using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Interfaces;
using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories;
using ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.DataModelsRepositories;

public class UserDataModelRepositoryTests : TestBase
{
    public UserDataModelRepositoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Constructor_WithValidDependencies_ShouldCreateInstance()
    {
        // Arrange
        LogArrange("Creating mock dependencies");
        var loggerMock = new Mock<ILogger<UserDataModelRepository>>();
        var unitOfWorkMock = new Mock<IAuthPostgreSqlUnitOfWork>();
        var mapperMock = new Mock<IDataModelMapper<UserDataModel>>();

        // Act
        LogAct("Instantiating UserDataModelRepository");
        var repository = new UserDataModelRepository(
            loggerMock.Object,
            unitOfWorkMock.Object,
            mapperMock.Object);

        // Assert
        LogAssert("Verifying instance was created successfully");
        repository.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_ShouldInheritFromDataModelRepositoryBase()
    {
        // Arrange
        LogArrange("Creating mock dependencies");
        var loggerMock = new Mock<ILogger<UserDataModelRepository>>();
        var unitOfWorkMock = new Mock<IAuthPostgreSqlUnitOfWork>();
        var mapperMock = new Mock<IDataModelMapper<UserDataModel>>();

        // Act
        LogAct("Instantiating UserDataModelRepository");
        var repository = new UserDataModelRepository(
            loggerMock.Object,
            unitOfWorkMock.Object,
            mapperMock.Object);

        // Assert
        LogAssert("Verifying inheritance from DataModelRepositoryBase");
        repository.ShouldBeAssignableTo<DataModelRepositoryBase<UserDataModel>>();
    }
}
