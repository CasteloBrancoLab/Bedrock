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
using ShopDemo.Auth.Domain.Entities.DenyListEntries;
using ShopDemo.Auth.Domain.Entities.DenyListEntries.Enums;
using ShopDemo.Auth.Domain.Entities.DenyListEntries.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Repositories;

public class DenyListEntryPostgreSqlRepositoryTests : TestBase
{
    private static readonly Faker Faker = new();

    private readonly Mock<IDenyListEntryDataModelRepository> _dataModelRepositoryMock;
    private readonly DenyListEntryPostgreSqlRepository _repository;

    public DenyListEntryPostgreSqlRepositoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _dataModelRepositoryMock = new Mock<IDenyListEntryDataModelRepository>();
        _repository = new DenyListEntryPostgreSqlRepository(_dataModelRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullDataModelRepository_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparing null argument for constructor");

        // Act
        LogAct("Attempting to create DenyListEntryPostgreSqlRepository with null");
        var action = () => new DenyListEntryPostgreSqlRepository(null!);

        // Assert
        LogAssert("Verifying ArgumentNullException is thrown");
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithValidDataModelRepository_ShouldCreateInstance()
    {
        // Arrange
        LogArrange("Creating mock IDenyListEntryDataModelRepository");
        var dataModelRepositoryMock = new Mock<IDenyListEntryDataModelRepository>();

        // Act
        LogAct("Creating DenyListEntryPostgreSqlRepository");
        var repository = new DenyListEntryPostgreSqlRepository(dataModelRepositoryMock.Object);

        // Assert
        LogAssert("Verifying instance was created");
        repository.ShouldNotBeNull();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenDataModelFound_ShouldReturnDenyListEntry()
    {
        // Arrange
        LogArrange("Setting up mock to return a DenyListEntryDataModel");
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
        LogAssert("Verifying DenyListEntry was created from DataModel");
        result.ShouldNotBeNull();
        result.Value.ShouldBe(dataModel.Value);
        result.Type.ShouldBe((DenyListEntryType)dataModel.Type);
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
            .ReturnsAsync((DenyListEntryDataModel?)null);

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
        LogArrange("Creating DenyListEntry entity and setting up mock");
        var executionContext = CreateTestExecutionContext();
        var entry = CreateTestDenyListEntry(DenyListEntryType.Jti, "some-token-value");

        _dataModelRepositoryMock
            .Setup(static x => x.InsertAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<DenyListEntryDataModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling RegisterNewAsync");
        var result = await _repository.RegisterNewAsync(executionContext, entry, CancellationToken.None);

        // Assert
        LogAssert("Verifying InsertAsync was called with a DenyListEntryDataModel");
        result.ShouldBeTrue();
        _dataModelRepositoryMock.Verify(
            static x => x.InsertAsync(
                It.IsAny<ExecutionContext>(),
                It.Is<DenyListEntryDataModel>(static dm =>
                    dm.Value == "some-token-value"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region ExistsByTypeAndValueAsync Tests

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExistsByTypeAndValueAsync_ShouldDelegateToDataModelRepository(bool expectedResult)
    {
        // Arrange
        LogArrange($"Setting up mock to return {expectedResult}");
        var executionContext = CreateTestExecutionContext();
        var type = DenyListEntryType.Jti;
        string value = "some-token";

        _dataModelRepositoryMock
            .Setup(static x => x.ExistsByTypeAndValueAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<short>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        LogAct("Calling ExistsByTypeAndValueAsync");
        var result = await _repository.ExistsByTypeAndValueAsync(executionContext, type, value, CancellationToken.None);

        // Assert
        LogAssert($"Verifying result is {expectedResult}");
        result.ShouldBe(expectedResult);
        _dataModelRepositoryMock.Verify(
            static x => x.ExistsByTypeAndValueAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<short>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetByTypeAndValueAsync Tests

    [Fact]
    public async Task GetByTypeAndValueAsync_WhenFound_ShouldReturnDenyListEntry()
    {
        // Arrange
        LogArrange("Setting up mock to return a DenyListEntryDataModel by type and value");
        var executionContext = CreateTestExecutionContext();
        var type = DenyListEntryType.Jti;
        string value = "blocked-token";
        var dataModel = CreateTestDataModel(Guid.NewGuid());
        dataModel.Value = value;
        dataModel.Type = (short)type;

        _dataModelRepositoryMock
            .Setup(static x => x.GetByTypeAndValueAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<short>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataModel);

        // Act
        LogAct("Calling GetByTypeAndValueAsync");
        var result = await _repository.GetByTypeAndValueAsync(executionContext, type, value, CancellationToken.None);

        // Assert
        LogAssert("Verifying DenyListEntry was created from DataModel");
        result.ShouldNotBeNull();
        result.Value.ShouldBe(value);
        result.Type.ShouldBe(type);
    }

    [Fact]
    public async Task GetByTypeAndValueAsync_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mock to return null for type and value search");
        var executionContext = CreateTestExecutionContext();

        _dataModelRepositoryMock
            .Setup(static x => x.GetByTypeAndValueAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<short>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((DenyListEntryDataModel?)null);

        // Act
        LogAct("Calling GetByTypeAndValueAsync");
        var result = await _repository.GetByTypeAndValueAsync(
            executionContext, DenyListEntryType.Jti, "nonexistent", CancellationToken.None);

        // Assert
        LogAssert("Verifying null was returned");
        result.ShouldBeNull();
    }

    #endregion

    #region DeleteExpiredAsync Tests

    [Fact]
    public async Task DeleteExpiredAsync_ShouldDelegateToDataModelRepository()
    {
        // Arrange
        LogArrange("Setting up mock to return count of deleted entries");
        var executionContext = CreateTestExecutionContext();
        var referenceDate = DateTimeOffset.UtcNow;

        _dataModelRepositoryMock
            .Setup(static x => x.DeleteExpiredAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        LogAct("Calling DeleteExpiredAsync");
        var result = await _repository.DeleteExpiredAsync(executionContext, referenceDate, CancellationToken.None);

        // Assert
        LogAssert("Verifying result was 5 deleted entries");
        result.ShouldBe(5);
        _dataModelRepositoryMock.Verify(
            static x => x.DeleteExpiredAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region DeleteByTypeAndValueAsync Tests

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteByTypeAndValueAsync_ShouldDelegateToDataModelRepository(bool expectedResult)
    {
        // Arrange
        LogArrange($"Setting up mock to return {expectedResult}");
        var executionContext = CreateTestExecutionContext();
        var type = DenyListEntryType.Jti;
        string value = "token-to-delete";

        _dataModelRepositoryMock
            .Setup(static x => x.DeleteByTypeAndValueAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<short>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        LogAct("Calling DeleteByTypeAndValueAsync");
        var result = await _repository.DeleteByTypeAndValueAsync(
            executionContext, type, value, CancellationToken.None);

        // Assert
        LogAssert($"Verifying result is {expectedResult}");
        result.ShouldBe(expectedResult);
        _dataModelRepositoryMock.Verify(
            static x => x.DeleteByTypeAndValueAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<short>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
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
        EnumerateAllItemHandler<DenyListEntry> handler = (_, _, _, _) => Task.FromResult(true);

        _dataModelRepositoryMock
            .Setup(static x => x.EnumerateAllAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<PaginationInfo>(),
                It.IsAny<DataModelItemHandler<DenyListEntryDataModel>>(),
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
                It.IsAny<DataModelItemHandler<DenyListEntryDataModel>>(),
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
        EnumerateAllItemHandler<DenyListEntry> handler = (_, _, _, _) => Task.FromResult(true);

        _dataModelRepositoryMock
            .Setup(static x => x.EnumerateAllAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<PaginationInfo>(),
                It.IsAny<DataModelItemHandler<DenyListEntryDataModel>>(),
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
        EnumerateModifiedSinceItemHandler<DenyListEntry> handler = (_, _, _, _, _) => Task.FromResult(true);

        _dataModelRepositoryMock
            .Setup(static x => x.EnumerateModifiedSinceAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DataModelItemHandler<DenyListEntryDataModel>>(),
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
                It.IsAny<DataModelItemHandler<DenyListEntryDataModel>>(),
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
        EnumerateModifiedSinceItemHandler<DenyListEntry> handler = (_, _, _, _, _) => Task.FromResult(true);

        _dataModelRepositoryMock
            .Setup(static x => x.EnumerateModifiedSinceAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DataModelItemHandler<DenyListEntryDataModel>>(),
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

    #region Helper Methods

    private static ExecutionContext CreateTestExecutionContext()
    {
        var tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Test Tenant");
        var timeProvider = TimeProvider.System;

        return ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: tenantInfo,
            executionUser: "test.user",
            executionOrigin: "UnitTest",
            businessOperationCode: "TEST_OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: timeProvider);
    }

    private static DenyListEntryDataModel CreateTestDataModel(Guid id)
    {
        return new DenyListEntryDataModel
        {
            Id = id,
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedExecutionOrigin = null,
            LastChangedCorrelationId = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            Type = (short)DenyListEntryType.Jti,
            Value = Faker.Random.AlphaNumeric(32),
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(24),
            Reason = null
        };
    }

    private static DenyListEntry CreateTestDenyListEntry(
        DenyListEntryType type,
        string value,
        Guid? entityId = null)
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
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));

        return DenyListEntry.CreateFromExistingInfo(
            new CreateFromExistingInfoDenyListEntryInput(
                entityInfo,
                type,
                value,
                DateTimeOffset.UtcNow.AddHours(24),
                null));
    }

    #endregion
}
