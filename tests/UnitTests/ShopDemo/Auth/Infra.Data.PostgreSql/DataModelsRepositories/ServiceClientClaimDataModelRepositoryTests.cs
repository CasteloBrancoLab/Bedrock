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

public class ServiceClientClaimDataModelRepositoryTests : TestBase
{
    public ServiceClientClaimDataModelRepositoryTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void Constructor_WithValidDependencies_ShouldCreateInstance()
    {
        LogArrange("Creating mock dependencies");
        var loggerMock = new Mock<ILogger<ServiceClientClaimDataModelRepository>>();
        var unitOfWorkMock = new Mock<IAuthPostgreSqlUnitOfWork>();
        var mapperMock = new Mock<IDataModelMapper<ServiceClientClaimDataModel>>();
        LogAct("Instantiating repository");
        var repository = new ServiceClientClaimDataModelRepository(loggerMock.Object, unitOfWorkMock.Object, mapperMock.Object);
        LogAssert("Verifying instance created");
        repository.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_ShouldInheritFromDataModelRepositoryBase()
    {
        LogArrange("Creating mock dependencies");
        var loggerMock = new Mock<ILogger<ServiceClientClaimDataModelRepository>>();
        var unitOfWorkMock = new Mock<IAuthPostgreSqlUnitOfWork>();
        var mapperMock = new Mock<IDataModelMapper<ServiceClientClaimDataModel>>();
        LogAct("Instantiating repository");
        var repository = new ServiceClientClaimDataModelRepository(loggerMock.Object, unitOfWorkMock.Object, mapperMock.Object);
        LogAssert("Verifying inheritance");
        repository.ShouldBeAssignableTo<DataModelRepositoryBase<ServiceClientClaimDataModel>>();
    }
}
