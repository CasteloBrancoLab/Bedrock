using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Data.Repositories;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Interfaces;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;

namespace Bedrock.UnitTests.BuildingBlocks.Data.Repositories;

public class RepositoryBaseTests : TestBase
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly ExecutionContext _executionContext;
    private readonly Id _testId;
    private readonly PaginationInfo _paginationInfo;
    private readonly TimeProvider _timeProvider;
    private readonly DateTimeOffset _sinceDate;

    public RepositoryBaseTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _loggerMock = new Mock<ILogger>();
        _loggerMock
            .Setup(x => x.IsEnabled(It.IsAny<LogLevel>()))
            .Returns(true);
        _loggerMock
            .Setup(x => x.BeginScope(It.IsAny<It.IsAnyType>()))
            .Returns(Mock.Of<IDisposable>());

        var tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Test Tenant");
        _executionContext = ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: tenantInfo,
            executionUser: "test-user",
            executionOrigin: "test-origin",
            businessOperationCode: "TEST_OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: TimeProvider.System
        );

        _testId = Id.GenerateNewId();
        _paginationInfo = PaginationInfo.Create(page: 1, pageSize: 10);
        _timeProvider = TimeProvider.System;
        _sinceDate = DateTimeOffset.UtcNow.AddDays(-7);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparing to create repository with null logger");

        // Act & Assert
        LogAct("Creating repository with null logger");
        var exception = Should.Throw<ArgumentNullException>(() =>
            new TestRepository(null!));

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("logger");
        LogInfo("ArgumentNullException thrown for null logger");
    }

    [Fact]
    public void Constructor_WithValidLogger_ShouldCreateInstance()
    {
        // Arrange
        LogArrange("Preparing to create repository with valid logger");

        // Act
        LogAct("Creating repository");
        var repository = new TestRepository(_loggerMock.Object);

        // Assert
        LogAssert("Verifying repository was created");
        repository.ShouldNotBeNull();
        repository.ExposedLogger.ShouldBe(_loggerMock.Object);
        LogInfo("Repository created successfully with valid logger");
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WhenInternalMethodSucceeds_ShouldReturnResults()
    {
        // Arrange
        LogArrange("Setting up repository with successful GetAllInternal");
        var expectedItems = new List<TestAggregateRoot>
        {
            new("Item1"),
            new("Item2")
        };
        var repository = new TestRepository(_loggerMock.Object)
        {
            GetAllInternalResult = expectedItems.ToAsyncEnumerable()
        };

        // Act
        LogAct("Calling GetAllAsync");
        var results = await repository.GetAllAsync(_executionContext, _paginationInfo, CancellationToken.None)
            .ToListAsync();

        // Assert
        LogAssert("Verifying results");
        results.Count.ShouldBe(2);
        results[0].Name.ShouldBe("Item1");
        results[1].Name.ShouldBe("Item2");
        LogInfo("GetAllAsync returned expected results");
    }

    [Fact]
    public async Task GetAllAsync_WhenInternalMethodThrows_ShouldReturnEmptyAndLogException()
    {
        // Arrange
        LogArrange("Setting up repository to throw exception");
        var repository = new TestRepository(_loggerMock.Object)
        {
            GetAllInternalShouldThrow = true
        };

        // Act
        LogAct("Calling GetAllAsync");
        var results = await repository.GetAllAsync(_executionContext, _paginationInfo, CancellationToken.None)
            .ToListAsync();

        // Assert
        LogAssert("Verifying empty result and logging");
        results.ShouldBeEmpty();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        LogInfo("GetAllAsync returned empty and logged exception");
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenInternalMethodSucceeds_ShouldReturnResult()
    {
        // Arrange
        LogArrange("Setting up repository with successful GetByIdInternal");
        var expectedItem = new TestAggregateRoot("Found Item");
        var repository = new TestRepository(_loggerMock.Object)
        {
            GetByIdInternalResult = expectedItem
        };

        // Act
        LogAct("Calling GetByIdAsync");
        var result = await repository.GetByIdAsync(_executionContext, _testId, CancellationToken.None);

        // Assert
        LogAssert("Verifying result");
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Found Item");
        LogInfo("GetByIdAsync returned expected result");
    }

    [Fact]
    public async Task GetByIdAsync_WhenInternalMethodReturnsNull_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up repository to return null");
        var repository = new TestRepository(_loggerMock.Object)
        {
            GetByIdInternalResult = null
        };

        // Act
        LogAct("Calling GetByIdAsync");
        var result = await repository.GetByIdAsync(_executionContext, _testId, CancellationToken.None);

        // Assert
        LogAssert("Verifying null result");
        result.ShouldBeNull();
        LogInfo("GetByIdAsync returned null as expected");
    }

    [Fact]
    public async Task GetByIdAsync_WhenInternalMethodThrows_ShouldReturnNullAndLogException()
    {
        // Arrange
        LogArrange("Setting up repository to throw exception");
        var repository = new TestRepository(_loggerMock.Object)
        {
            GetByIdInternalShouldThrow = true
        };

        // Act
        LogAct("Calling GetByIdAsync");
        var result = await repository.GetByIdAsync(_executionContext, _testId, CancellationToken.None);

        // Assert
        LogAssert("Verifying null result and logging");
        result.ShouldBeNull();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        LogInfo("GetByIdAsync returned null and logged exception");
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_WhenInternalMethodReturnsTrue_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up repository to return true");
        var repository = new TestRepository(_loggerMock.Object)
        {
            ExistsInternalResult = true
        };

        // Act
        LogAct("Calling ExistsAsync");
        var result = await repository.ExistsAsync(_executionContext, _testId, CancellationToken.None);

        // Assert
        LogAssert("Verifying true result");
        result.ShouldBeTrue();
        LogInfo("ExistsAsync returned true as expected");
    }

    [Fact]
    public async Task ExistsAsync_WhenInternalMethodReturnsFalse_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up repository to return false");
        var repository = new TestRepository(_loggerMock.Object)
        {
            ExistsInternalResult = false
        };

        // Act
        LogAct("Calling ExistsAsync");
        var result = await repository.ExistsAsync(_executionContext, _testId, CancellationToken.None);

        // Assert
        LogAssert("Verifying false result");
        result.ShouldBeFalse();
        LogInfo("ExistsAsync returned false as expected");
    }

    [Fact]
    public async Task ExistsAsync_WhenInternalMethodThrows_ShouldReturnFalseAndLogException()
    {
        // Arrange
        LogArrange("Setting up repository to throw exception");
        var repository = new TestRepository(_loggerMock.Object)
        {
            ExistsInternalShouldThrow = true
        };

        // Act
        LogAct("Calling ExistsAsync");
        var result = await repository.ExistsAsync(_executionContext, _testId, CancellationToken.None);

        // Assert
        LogAssert("Verifying false result and logging");
        result.ShouldBeFalse();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        LogInfo("ExistsAsync returned false and logged exception");
    }

    #endregion

    #region RegisterNewAsync Tests

    [Fact]
    public async Task RegisterNewAsync_WhenInternalMethodSucceeds_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up repository to return true");
        var aggregateRoot = new TestAggregateRoot("New Item");
        var repository = new TestRepository(_loggerMock.Object)
        {
            RegisterNewInternalResult = true
        };

        // Act
        LogAct("Calling RegisterNewAsync");
        var result = await repository.RegisterNewAsync(_executionContext, aggregateRoot, CancellationToken.None);

        // Assert
        LogAssert("Verifying true result");
        result.ShouldBeTrue();
        LogInfo("RegisterNewAsync returned true as expected");
    }

    [Fact]
    public async Task RegisterNewAsync_WhenInternalMethodReturnsFalse_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up repository to return false");
        var aggregateRoot = new TestAggregateRoot("New Item");
        var repository = new TestRepository(_loggerMock.Object)
        {
            RegisterNewInternalResult = false
        };

        // Act
        LogAct("Calling RegisterNewAsync");
        var result = await repository.RegisterNewAsync(_executionContext, aggregateRoot, CancellationToken.None);

        // Assert
        LogAssert("Verifying false result");
        result.ShouldBeFalse();
        LogInfo("RegisterNewAsync returned false as expected");
    }

    [Fact]
    public async Task RegisterNewAsync_WhenInternalMethodThrows_ShouldReturnFalseAndLogException()
    {
        // Arrange
        LogArrange("Setting up repository to throw exception");
        var aggregateRoot = new TestAggregateRoot("New Item");
        var repository = new TestRepository(_loggerMock.Object)
        {
            RegisterNewInternalShouldThrow = true
        };

        // Act
        LogAct("Calling RegisterNewAsync");
        var result = await repository.RegisterNewAsync(_executionContext, aggregateRoot, CancellationToken.None);

        // Assert
        LogAssert("Verifying false result and logging");
        result.ShouldBeFalse();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        LogInfo("RegisterNewAsync returned false and logged exception");
    }

    #endregion

    #region GetModifiedSinceAsync Tests

    [Fact]
    public async Task GetModifiedSinceAsync_WhenInternalMethodSucceeds_ShouldReturnResults()
    {
        // Arrange
        LogArrange("Setting up repository with successful GetModifiedSinceInternal");
        var expectedItems = new List<TestAggregateRoot>
        {
            new("Modified1"),
            new("Modified2"),
            new("Modified3")
        };
        var repository = new TestRepository(_loggerMock.Object)
        {
            GetModifiedSinceInternalResult = expectedItems.ToAsyncEnumerable()
        };

        // Act
        LogAct("Calling GetModifiedSinceAsync");
        var results = await repository.GetModifiedSinceAsync(
            _executionContext, _timeProvider, _sinceDate, CancellationToken.None)
            .ToListAsync();

        // Assert
        LogAssert("Verifying results");
        results.Count.ShouldBe(3);
        results[0].Name.ShouldBe("Modified1");
        results[1].Name.ShouldBe("Modified2");
        results[2].Name.ShouldBe("Modified3");
        LogInfo("GetModifiedSinceAsync returned expected results");
    }

    [Fact]
    public async Task GetModifiedSinceAsync_WhenInternalMethodThrows_ShouldReturnEmptyAndLogException()
    {
        // Arrange
        LogArrange("Setting up repository to throw exception");
        var repository = new TestRepository(_loggerMock.Object)
        {
            GetModifiedSinceInternalShouldThrow = true
        };

        // Act
        LogAct("Calling GetModifiedSinceAsync");
        var results = await repository.GetModifiedSinceAsync(
            _executionContext, _timeProvider, _sinceDate, CancellationToken.None)
            .ToListAsync();

        // Assert
        LogAssert("Verifying empty result and logging");
        results.ShouldBeEmpty();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        LogInfo("GetModifiedSinceAsync returned empty and logged exception");
    }

    #endregion

    #region Logger Property Tests

    [Fact]
    public void Logger_ShouldBeAccessibleByDerivedClass()
    {
        // Arrange
        LogArrange("Creating repository");
        var repository = new TestRepository(_loggerMock.Object);

        // Act
        LogAct("Accessing Logger property");
        var logger = repository.ExposedLogger;

        // Assert
        LogAssert("Verifying logger is accessible");
        logger.ShouldNotBeNull();
        logger.ShouldBe(_loggerMock.Object);
        LogInfo("Logger property is accessible to derived class");
    }

    #endregion

    #region Test Helpers

    private class TestAggregateRoot : EntityBase<TestAggregateRoot>, IAggregateRoot
    {
        public string Name { get; }

        public TestAggregateRoot(string name)
        {
            Name = name;
        }

        public TestAggregateRoot(string name, EntityInfo entityInfo) : base(entityInfo)
        {
            Name = name;
        }

        protected override bool IsValidInternal(ExecutionContext executionContext) => true;

        public override IEntity<TestAggregateRoot> Clone()
        {
            return new TestAggregateRoot(Name, EntityInfo);
        }

        protected override string CreateMessageCode(string messageSuffix)
        {
            return $"TestAggregateRoot.{messageSuffix}";
        }
    }

    private class TestRepository : RepositoryBase<TestAggregateRoot>
    {
        public IAsyncEnumerable<TestAggregateRoot>? GetAllInternalResult { get; set; }
        public bool GetAllInternalShouldThrow { get; set; }

        public TestAggregateRoot? GetByIdInternalResult { get; set; }
        public bool GetByIdInternalShouldThrow { get; set; }

        public bool ExistsInternalResult { get; set; }
        public bool ExistsInternalShouldThrow { get; set; }

        public bool RegisterNewInternalResult { get; set; }
        public bool RegisterNewInternalShouldThrow { get; set; }

        public IAsyncEnumerable<TestAggregateRoot>? GetModifiedSinceInternalResult { get; set; }
        public bool GetModifiedSinceInternalShouldThrow { get; set; }

        public ILogger ExposedLogger => Logger;

        public TestRepository(ILogger logger) : base(logger)
        {
        }

        protected override IAsyncEnumerable<TestAggregateRoot> GetAllInternalAsync(
            PaginationInfo paginationInfo,
            CancellationToken cancellationToken)
        {
            if (GetAllInternalShouldThrow)
                throw new InvalidOperationException("Test exception in GetAllInternal");

            return GetAllInternalResult ?? AsyncEnumerable.Empty<TestAggregateRoot>();
        }

        protected override Task<TestAggregateRoot?> GetByIdInternalAsync(
            ExecutionContext executionContext,
            Id id,
            CancellationToken cancellationToken)
        {
            if (GetByIdInternalShouldThrow)
                throw new InvalidOperationException("Test exception in GetByIdInternal");

            return Task.FromResult(GetByIdInternalResult);
        }

        protected override Task<bool> ExistsInternalAsync(
            ExecutionContext executionContext,
            Id id,
            CancellationToken cancellationToken)
        {
            if (ExistsInternalShouldThrow)
                throw new InvalidOperationException("Test exception in ExistsInternal");

            return Task.FromResult(ExistsInternalResult);
        }

        protected override Task<bool> RegisterNewInternalAsync(
            ExecutionContext executionContext,
            TestAggregateRoot aggregateRoot,
            CancellationToken cancellationToken)
        {
            if (RegisterNewInternalShouldThrow)
                throw new InvalidOperationException("Test exception in RegisterNewInternal");

            return Task.FromResult(RegisterNewInternalResult);
        }

        protected override IAsyncEnumerable<TestAggregateRoot> GetModifiedSinceInternalAsync(
            ExecutionContext executionContext,
            TimeProvider timeProvider,
            DateTimeOffset since,
            CancellationToken cancellationToken)
        {
            if (GetModifiedSinceInternalShouldThrow)
                throw new InvalidOperationException("Test exception in GetModifiedSinceInternal");

            return GetModifiedSinceInternalResult ?? AsyncEnumerable.Empty<TestAggregateRoot>();
        }
    }

    #endregion
}
