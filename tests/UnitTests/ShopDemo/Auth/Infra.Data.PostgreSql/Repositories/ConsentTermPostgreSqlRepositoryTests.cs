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
using ShopDemo.Auth.Domain.Entities.ConsentTerms;
using ShopDemo.Auth.Domain.Entities.ConsentTerms.Enums;
using ShopDemo.Auth.Domain.Entities.ConsentTerms.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Repositories;

public class ConsentTermPostgreSqlRepositoryTests : TestBase
{
    private static readonly Faker Faker = new();

    private readonly Mock<IConsentTermDataModelRepository> _dataModelRepositoryMock;
    private readonly ConsentTermPostgreSqlRepository _repository;

    public ConsentTermPostgreSqlRepositoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _dataModelRepositoryMock = new Mock<IConsentTermDataModelRepository>();
        _repository = new ConsentTermPostgreSqlRepository(_dataModelRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullDataModelRepository_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparing null argument for constructor");

        // Act
        LogAct("Attempting to create ConsentTermPostgreSqlRepository with null");
        var action = () => new ConsentTermPostgreSqlRepository(null!);

        // Assert
        LogAssert("Verifying ArgumentNullException is thrown");
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithValidDataModelRepository_ShouldCreateInstance()
    {
        // Arrange
        LogArrange("Creating mock IConsentTermDataModelRepository");
        var dataModelRepositoryMock = new Mock<IConsentTermDataModelRepository>();

        // Act
        LogAct("Creating ConsentTermPostgreSqlRepository");
        var repository = new ConsentTermPostgreSqlRepository(dataModelRepositoryMock.Object);

        // Assert
        LogAssert("Verifying instance was created");
        repository.ShouldNotBeNull();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenDataModelFound_ShouldReturnConsentTerm()
    {
        // Arrange
        LogArrange("Setting up mock to return a ConsentTermDataModel");
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
        LogAssert("Verifying ConsentTerm was created from DataModel");
        result.ShouldNotBeNull();
        result.TermVersion.ShouldBe(dataModel.Version);
        result.Content.ShouldBe(dataModel.Content);
        result.Type.ShouldBe((ConsentTermType)dataModel.Type);
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
            .ReturnsAsync((ConsentTermDataModel?)null);

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
        LogArrange("Creating ConsentTerm entity and setting up mock");
        var executionContext = CreateTestExecutionContext();
        var consentTerm = CreateTestConsentTerm(ConsentTermType.TermsOfUse, "1.0", "Terms content");

        _dataModelRepositoryMock
            .Setup(static x => x.InsertAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<ConsentTermDataModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling RegisterNewAsync");
        var result = await _repository.RegisterNewAsync(executionContext, consentTerm, CancellationToken.None);

        // Assert
        LogAssert("Verifying InsertAsync was called with a ConsentTermDataModel");
        result.ShouldBeTrue();
        _dataModelRepositoryMock.Verify(
            static x => x.InsertAsync(
                It.IsAny<ExecutionContext>(),
                It.Is<ConsentTermDataModel>(static dm =>
                    dm.Version == "1.0" &&
                    dm.Content == "Terms content"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetLatestByTypeAsync Tests

    [Fact]
    public async Task GetLatestByTypeAsync_WhenFound_ShouldReturnConsentTerm()
    {
        // Arrange
        LogArrange("Setting up mock to return a ConsentTermDataModel by type");
        var executionContext = CreateTestExecutionContext();
        var type = ConsentTermType.TermsOfUse;
        var dataModel = CreateTestDataModel(Guid.NewGuid());
        dataModel.Type = (short)type;

        _dataModelRepositoryMock
            .Setup(static x => x.GetLatestByTypeAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<short>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataModel);

        // Act
        LogAct("Calling GetLatestByTypeAsync");
        var result = await _repository.GetLatestByTypeAsync(executionContext, type, CancellationToken.None);

        // Assert
        LogAssert("Verifying ConsentTerm was returned from DataModel");
        result.ShouldNotBeNull();
        result.Type.ShouldBe(type);
    }

    [Fact]
    public async Task GetLatestByTypeAsync_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mock to return null for type search");
        var executionContext = CreateTestExecutionContext();

        _dataModelRepositoryMock
            .Setup(static x => x.GetLatestByTypeAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<short>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConsentTermDataModel?)null);

        // Act
        LogAct("Calling GetLatestByTypeAsync");
        var result = await _repository.GetLatestByTypeAsync(
            executionContext, ConsentTermType.PrivacyPolicy, CancellationToken.None);

        // Assert
        LogAssert("Verifying null was returned");
        result.ShouldBeNull();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WhenItemsExist_ShouldReturnConsentTerms()
    {
        // Arrange
        LogArrange("Setting up mock to return a list of ConsentTermDataModels");
        var executionContext = CreateTestExecutionContext();
        var dataModels = new List<ConsentTermDataModel>
        {
            CreateTestDataModel(Guid.NewGuid()),
            CreateTestDataModel(Guid.NewGuid())
        };

        _dataModelRepositoryMock
            .Setup(static x => x.GetAllAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataModels);

        // Act
        LogAct("Calling GetAllAsync");
        var result = await _repository.GetAllAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying list of ConsentTerms was returned");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoItems_ShouldReturnEmptyList()
    {
        // Arrange
        LogArrange("Setting up mock to return an empty list");
        var executionContext = CreateTestExecutionContext();

        _dataModelRepositoryMock
            .Setup(static x => x.GetAllAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ConsentTermDataModel>());

        // Act
        LogAct("Calling GetAllAsync");
        var result = await _repository.GetAllAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying empty list was returned");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
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
        EnumerateAllItemHandler<ConsentTerm> handler = (_, _, _, _) => Task.FromResult(true);

        _dataModelRepositoryMock
            .Setup(static x => x.EnumerateAllAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<PaginationInfo>(),
                It.IsAny<DataModelItemHandler<ConsentTermDataModel>>(),
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
                It.IsAny<DataModelItemHandler<ConsentTermDataModel>>(),
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
        EnumerateAllItemHandler<ConsentTerm> handler = (_, _, _, _) => Task.FromResult(true);

        _dataModelRepositoryMock
            .Setup(static x => x.EnumerateAllAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<PaginationInfo>(),
                It.IsAny<DataModelItemHandler<ConsentTermDataModel>>(),
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
        EnumerateModifiedSinceItemHandler<ConsentTerm> handler = (_, _, _, _, _) => Task.FromResult(true);

        _dataModelRepositoryMock
            .Setup(static x => x.EnumerateModifiedSinceAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DataModelItemHandler<ConsentTermDataModel>>(),
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
                It.IsAny<DataModelItemHandler<ConsentTermDataModel>>(),
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
        EnumerateModifiedSinceItemHandler<ConsentTerm> handler = (_, _, _, _, _) => Task.FromResult(true);

        _dataModelRepositoryMock
            .Setup(static x => x.EnumerateModifiedSinceAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DataModelItemHandler<ConsentTermDataModel>>(),
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

    private static ConsentTermDataModel CreateTestDataModel(Guid id)
    {
        return new ConsentTermDataModel
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
            Type = (short)ConsentTermType.TermsOfUse,
            Version = "1.0",
            Content = Faker.Lorem.Paragraphs(2),
            PublishedAt = DateTimeOffset.UtcNow
        };
    }

    private static ConsentTerm CreateTestConsentTerm(
        ConsentTermType type,
        string version,
        string content,
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

        return ConsentTerm.CreateFromExistingInfo(
            new CreateFromExistingInfoConsentTermInput(
                entityInfo,
                type,
                version,
                content,
                DateTimeOffset.UtcNow));
    }

    #endregion
}
