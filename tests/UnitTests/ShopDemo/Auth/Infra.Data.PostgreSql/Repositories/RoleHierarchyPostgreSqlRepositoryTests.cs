using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Persistence.Abstractions.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using Moq;
using ShopDemo.Auth.Domain.Entities.RoleHierarchies;
using ShopDemo.Auth.Domain.Entities.RoleHierarchies.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Repositories;

public class RoleHierarchyPostgreSqlRepositoryTests : TestBase
{
    private static readonly Faker Faker = new();

    private readonly Mock<IRoleHierarchyDataModelRepository> _dataModelRepositoryMock;
    private readonly RoleHierarchyPostgreSqlRepository _repository;

    public RoleHierarchyPostgreSqlRepositoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _dataModelRepositoryMock = new Mock<IRoleHierarchyDataModelRepository>();
        _repository = new RoleHierarchyPostgreSqlRepository(_dataModelRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullDataModelRepository_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparing null argument for constructor");

        // Act
        LogAct("Attempting to create RoleHierarchyPostgreSqlRepository with null");
        var action = () => new RoleHierarchyPostgreSqlRepository(null!);

        // Assert
        LogAssert("Verifying ArgumentNullException is thrown");
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithValidDataModelRepository_ShouldCreateInstance()
    {
        // Arrange
        LogArrange("Creating mock IRoleHierarchyDataModelRepository");
        var dataModelRepositoryMock = new Mock<IRoleHierarchyDataModelRepository>();

        // Act
        LogAct("Creating RoleHierarchyPostgreSqlRepository");
        var repository = new RoleHierarchyPostgreSqlRepository(dataModelRepositoryMock.Object);

        // Assert
        LogAssert("Verifying instance was created");
        repository.ShouldNotBeNull();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenDataModelFound_ShouldReturnRoleHierarchy()
    {
        // Arrange
        LogArrange("Setting up mock to return a RoleHierarchyDataModel");
        var executionContext = CreateTestExecutionContext();
        var entityId = Guid.NewGuid();
        var id = Id.CreateFromExistingInfo(entityId);
        var dataModel = CreateTestDataModel(entityId);

        _dataModelRepositoryMock
            .Setup(static x => x.GetByIdAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataModel);

        // Act
        LogAct("Calling GetByIdAsync");
        var result = await _repository.GetByIdAsync(executionContext, id, CancellationToken.None);

        // Assert
        LogAssert("Verifying RoleHierarchy was created from DataModel");
        result.ShouldNotBeNull();
        result.EntityInfo.Id.Value.ShouldBe(entityId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenDataModelNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mock to return null");
        var executionContext = CreateTestExecutionContext();
        var id = Id.CreateFromExistingInfo(Guid.NewGuid());

        _dataModelRepositoryMock
            .Setup(static x => x.GetByIdAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RoleHierarchyDataModel?)null);

        // Act
        LogAct("Calling GetByIdAsync");
        var result = await _repository.GetByIdAsync(executionContext, id, CancellationToken.None);

        // Assert
        LogAssert("Verifying null was returned");
        result.ShouldBeNull();
    }

    #endregion

    #region ExistsAsync Tests

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExistsAsync_ShouldDelegateToDataModelRepository(bool expectedResult)
    {
        // Arrange
        LogArrange($"Setting up mock to return {expectedResult}");
        var executionContext = CreateTestExecutionContext();
        var id = Id.CreateFromExistingInfo(Guid.NewGuid());

        _dataModelRepositoryMock
            .Setup(static x => x.ExistsAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        LogAct("Calling ExistsAsync");
        var result = await _repository.ExistsAsync(executionContext, id, CancellationToken.None);

        // Assert
        LogAssert($"Verifying result is {expectedResult}");
        result.ShouldBe(expectedResult);
        _dataModelRepositoryMock.Verify(
            static x => x.ExistsAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region RegisterNewAsync Tests

    [Fact]
    public async Task RegisterNewAsync_ShouldCreateDataModelAndDelegateInsert()
    {
        // Arrange
        LogArrange("Creating RoleHierarchy entity and setting up mock");
        var executionContext = CreateTestExecutionContext();
        var roleHierarchy = CreateTestRoleHierarchy();

        _dataModelRepositoryMock
            .Setup(static x => x.InsertAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<RoleHierarchyDataModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling RegisterNewAsync");
        var result = await _repository.RegisterNewAsync(executionContext, roleHierarchy, CancellationToken.None);

        // Assert
        LogAssert("Verifying InsertAsync was called with a RoleHierarchyDataModel");
        result.ShouldBeTrue();
        _dataModelRepositoryMock.Verify(
            static x => x.InsertAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<RoleHierarchyDataModel>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenExistingDataModelFound_ShouldDeleteWithCorrectVersion()
    {
        // Arrange
        LogArrange("Setting up mock to return existing DataModel and successful delete");
        var executionContext = CreateTestExecutionContext();
        var entityId = Guid.NewGuid();
        long dbVersion = Faker.Random.Long(1, 100);
        var existingDataModel = CreateTestDataModel(entityId);
        existingDataModel.EntityVersion = dbVersion;
        var roleHierarchy = CreateTestRoleHierarchy(entityId);

        _dataModelRepositoryMock
            .Setup(static x => x.GetByIdAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingDataModel);

        _dataModelRepositoryMock
            .Setup(static x => x.DeleteAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling DeleteAsync");
        var result = await _repository.DeleteAsync(executionContext, roleHierarchy, CancellationToken.None);

        // Assert
        LogAssert("Verifying delete was successful with correct version");
        result.ShouldBeTrue();
        _dataModelRepositoryMock.Verify(
            x => x.DeleteAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                dbVersion,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenExistingDataModelNotFound_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to return null for GetByIdAsync in delete");
        var executionContext = CreateTestExecutionContext();
        var roleHierarchy = CreateTestRoleHierarchy();

        _dataModelRepositoryMock
            .Setup(static x => x.GetByIdAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RoleHierarchyDataModel?)null);

        // Act
        LogAct("Calling DeleteAsync");
        var result = await _repository.DeleteAsync(executionContext, roleHierarchy, CancellationToken.None);

        // Assert
        LogAssert("Verifying false was returned and DeleteAsync on repository was not called");
        result.ShouldBeFalse();
        _dataModelRepositoryMock.Verify(
            static x => x.DeleteAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<Guid>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region EnumerateAllAsync Tests

    [Fact]
    public async Task EnumerateAllAsync_ShouldDelegateToDataModelRepository()
    {
        // Arrange
        LogArrange("Setting up mock to return true for EnumerateAllAsync");
        var executionContext = CreateTestExecutionContext();
        var paginationInfo = PaginationInfo.Create(page: 1, pageSize: 10);
        EnumerateAllItemHandler<RoleHierarchy> handler = (_, _, _, _) => Task.FromResult(true);

        _dataModelRepositoryMock
            .Setup(static x => x.EnumerateAllAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<PaginationInfo>(),
                It.IsAny<DataModelItemHandler<RoleHierarchyDataModel>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling EnumerateAllAsync");
        var result = await _repository.EnumerateAllAsync(
            executionContext, paginationInfo, handler, CancellationToken.None);

        // Assert
        LogAssert("Verifying EnumerateAllAsync was delegated and returned true");
        result.ShouldBeTrue();
        _dataModelRepositoryMock.Verify(
            static x => x.EnumerateAllAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<PaginationInfo>(),
                It.IsAny<DataModelItemHandler<RoleHierarchyDataModel>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EnumerateAllAsync_WhenFails_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to return false for EnumerateAllAsync");
        var executionContext = CreateTestExecutionContext();
        var paginationInfo = PaginationInfo.Create(page: 1, pageSize: 10);
        EnumerateAllItemHandler<RoleHierarchy> handler = (_, _, _, _) => Task.FromResult(true);

        _dataModelRepositoryMock
            .Setup(static x => x.EnumerateAllAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<PaginationInfo>(),
                It.IsAny<DataModelItemHandler<RoleHierarchyDataModel>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Calling EnumerateAllAsync");
        var result = await _repository.EnumerateAllAsync(
            executionContext, paginationInfo, handler, CancellationToken.None);

        // Assert
        LogAssert("Verifying EnumerateAllAsync returned false");
        result.ShouldBeFalse();
    }

    #endregion

    #region EnumerateModifiedSinceAsync Tests

    [Fact]
    public async Task EnumerateModifiedSinceAsync_ShouldDelegateToDataModelRepository()
    {
        // Arrange
        LogArrange("Setting up mock to return true for EnumerateModifiedSinceAsync");
        var executionContext = CreateTestExecutionContext();
        var timeProvider = TimeProvider.System;
        var since = DateTimeOffset.UtcNow.AddDays(-1);
        EnumerateModifiedSinceItemHandler<RoleHierarchy> handler = (_, _, _, _, _) => Task.FromResult(true);

        _dataModelRepositoryMock
            .Setup(static x => x.EnumerateModifiedSinceAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DataModelItemHandler<RoleHierarchyDataModel>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling EnumerateModifiedSinceAsync");
        var result = await _repository.EnumerateModifiedSinceAsync(
            executionContext, timeProvider, since, handler, CancellationToken.None);

        // Assert
        LogAssert("Verifying EnumerateModifiedSinceAsync was delegated and returned true");
        result.ShouldBeTrue();
        _dataModelRepositoryMock.Verify(
            static x => x.EnumerateModifiedSinceAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DataModelItemHandler<RoleHierarchyDataModel>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EnumerateModifiedSinceAsync_WhenFails_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to return false for EnumerateModifiedSinceAsync");
        var executionContext = CreateTestExecutionContext();
        var timeProvider = TimeProvider.System;
        var since = DateTimeOffset.UtcNow.AddDays(-1);
        EnumerateModifiedSinceItemHandler<RoleHierarchy> handler = (_, _, _, _, _) => Task.FromResult(true);

        _dataModelRepositoryMock
            .Setup(static x => x.EnumerateModifiedSinceAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DataModelItemHandler<RoleHierarchyDataModel>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Calling EnumerateModifiedSinceAsync");
        var result = await _repository.EnumerateModifiedSinceAsync(
            executionContext, timeProvider, since, handler, CancellationToken.None);

        // Assert
        LogAssert("Verifying EnumerateModifiedSinceAsync returned false");
        result.ShouldBeFalse();
    }

    #endregion

    #region GetByRoleIdAsync Tests

    [Fact]
    public async Task GetByRoleIdAsync_WhenHierarchiesExist_ShouldReturnList()
    {
        // Arrange
        LogArrange("Setting up mock to return list of RoleHierarchyDataModels by roleId");
        var executionContext = CreateTestExecutionContext();
        var roleId = Guid.NewGuid();
        var roleIdObj = Id.CreateFromExistingInfo(roleId);
        var dataModels = new List<RoleHierarchyDataModel>
        {
            CreateTestDataModel(roleId: roleId),
            CreateTestDataModel(roleId: roleId)
        };

        _dataModelRepositoryMock
            .Setup(x => x.GetByRoleIdAsync(
                It.IsAny<ExecutionContext>(), roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataModels);

        // Act
        LogAct("Calling GetByRoleIdAsync");
        var result = await _repository.GetByRoleIdAsync(executionContext, roleIdObj, CancellationToken.None);

        // Assert
        LogAssert("Verifying list with 2 items was returned");
        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetByRoleIdAsync_WhenNoHierarchies_ShouldReturnEmptyList()
    {
        // Arrange
        LogArrange("Setting up mock to return empty list for roleId");
        var executionContext = CreateTestExecutionContext();
        var roleId = Id.CreateFromExistingInfo(Guid.NewGuid());

        _dataModelRepositoryMock
            .Setup(static x => x.GetByRoleIdAsync(
                It.IsAny<ExecutionContext>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleHierarchyDataModel>());

        // Act
        LogAct("Calling GetByRoleIdAsync");
        var result = await _repository.GetByRoleIdAsync(executionContext, roleId, CancellationToken.None);

        // Assert
        LogAssert("Verifying empty list was returned");
        result.Count.ShouldBe(0);
    }

    #endregion

    #region GetByParentRoleIdAsync Tests

    [Fact]
    public async Task GetByParentRoleIdAsync_WhenHierarchiesExist_ShouldReturnList()
    {
        // Arrange
        LogArrange("Setting up mock to return list of RoleHierarchyDataModels by parentRoleId");
        var executionContext = CreateTestExecutionContext();
        var parentRoleId = Guid.NewGuid();
        var parentRoleIdObj = Id.CreateFromExistingInfo(parentRoleId);
        var dataModels = new List<RoleHierarchyDataModel>
        {
            CreateTestDataModel(parentRoleId: parentRoleId),
            CreateTestDataModel(parentRoleId: parentRoleId)
        };

        _dataModelRepositoryMock
            .Setup(x => x.GetByParentRoleIdAsync(
                It.IsAny<ExecutionContext>(), parentRoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataModels);

        // Act
        LogAct("Calling GetByParentRoleIdAsync");
        var result = await _repository.GetByParentRoleIdAsync(
            executionContext, parentRoleIdObj, CancellationToken.None);

        // Assert
        LogAssert("Verifying list with 2 items was returned");
        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetByParentRoleIdAsync_WhenNoHierarchies_ShouldReturnEmptyList()
    {
        // Arrange
        LogArrange("Setting up mock to return empty list for parentRoleId");
        var executionContext = CreateTestExecutionContext();
        var parentRoleId = Id.CreateFromExistingInfo(Guid.NewGuid());

        _dataModelRepositoryMock
            .Setup(static x => x.GetByParentRoleIdAsync(
                It.IsAny<ExecutionContext>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleHierarchyDataModel>());

        // Act
        LogAct("Calling GetByParentRoleIdAsync");
        var result = await _repository.GetByParentRoleIdAsync(
            executionContext, parentRoleId, CancellationToken.None);

        // Assert
        LogAssert("Verifying empty list was returned");
        result.Count.ShouldBe(0);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WhenHierarchiesExist_ShouldReturnList()
    {
        // Arrange
        LogArrange("Setting up mock to return all RoleHierarchyDataModels");
        var executionContext = CreateTestExecutionContext();
        var dataModels = new List<RoleHierarchyDataModel>
        {
            CreateTestDataModel(),
            CreateTestDataModel(),
            CreateTestDataModel()
        };

        _dataModelRepositoryMock
            .Setup(static x => x.GetAllAsync(
                It.IsAny<ExecutionContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataModels);

        // Act
        LogAct("Calling GetAllAsync");
        var result = await _repository.GetAllAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying list with 3 items was returned");
        result.Count.ShouldBe(3);
        _dataModelRepositoryMock.Verify(
            static x => x.GetAllAsync(
                It.IsAny<ExecutionContext>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoHierarchies_ShouldReturnEmptyList()
    {
        // Arrange
        LogArrange("Setting up mock to return empty list for GetAllAsync");
        var executionContext = CreateTestExecutionContext();

        _dataModelRepositoryMock
            .Setup(static x => x.GetAllAsync(
                It.IsAny<ExecutionContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleHierarchyDataModel>());

        // Act
        LogAct("Calling GetAllAsync");
        var result = await _repository.GetAllAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying empty list was returned");
        result.Count.ShouldBe(0);
    }

    #endregion

    #region Helper Methods

    private static ExecutionContext CreateTestExecutionContext()
    {
        var tenantInfo = TenantInfo.Create(Guid.NewGuid());
        return ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: tenantInfo,
            executionUser: "test.user",
            executionOrigin: "UnitTest",
            businessOperationCode: "TEST_OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: TimeProvider.System);
    }

    private static RoleHierarchyDataModel CreateTestDataModel(
        Guid? id = null,
        Guid? roleId = null,
        Guid? parentRoleId = null)
    {
        return new RoleHierarchyDataModel
        {
            Id = id ?? Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedExecutionOrigin = null,
            LastChangedCorrelationId = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            RoleId = roleId ?? Guid.NewGuid(),
            ParentRoleId = parentRoleId ?? Guid.NewGuid()
        };
    }

    private static RoleHierarchy CreateTestRoleHierarchy(Guid? entityId = null)
    {
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(entityId ?? Guid.NewGuid()),
            tenantInfo: TenantInfo.Create(Guid.NewGuid()),
            createdAt: DateTimeOffset.UtcNow,
            createdBy: "test-creator",
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "UnitTest",
            createdBusinessOperationCode: "TEST_OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(1));

        return RoleHierarchy.CreateFromExistingInfo(
            new CreateFromExistingInfoRoleHierarchyInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                Id.CreateFromExistingInfo(Guid.NewGuid())));
    }

    #endregion
}
