using System.Runtime.CompilerServices;
using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Data.Repositories;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Interfaces;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Domain.Repositories;
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

    #region EnumerateAllAsync Tests

    [Fact]
    public async Task EnumerateAllAsync_WhenHandlerReturnsTrue_ShouldProcessAllItems()
    {
        // Arrange
        LogArrange("Setting up repository with items");
        var expectedItems = new List<TestAggregateRoot>
        {
            new("Item1"),
            new("Item2")
        };
        var processedItems = new List<TestAggregateRoot>();
        var repository = new TestRepository(_loggerMock.Object)
        {
            GetAllInternalResult = expectedItems.ToAsyncEnumerable()
        };

        // Act
        LogAct("Calling EnumerateAllAsync");
        var result = await repository.EnumerateAllAsync(
            _executionContext,
            _paginationInfo,
            (ctx, item, pagination, ct) =>
            {
                processedItems.Add(item);
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verifying all items processed");
        result.ShouldBeTrue();
        processedItems.Count.ShouldBe(2);
        processedItems[0].Name.ShouldBe("Item1");
        processedItems[1].Name.ShouldBe("Item2");
        LogInfo("EnumerateAllAsync processed all items successfully");
    }

    [Fact]
    public async Task EnumerateAllAsync_WhenHandlerReturnsFalse_ShouldStopProcessing()
    {
        // Arrange
        LogArrange("Setting up repository with multiple items");
        var expectedItems = new List<TestAggregateRoot>
        {
            new("Item1"),
            new("Item2"),
            new("Item3")
        };
        var processedItems = new List<TestAggregateRoot>();
        var repository = new TestRepository(_loggerMock.Object)
        {
            GetAllInternalResult = expectedItems.ToAsyncEnumerable()
        };

        // Act
        LogAct("Calling EnumerateAllAsync with handler that stops on second item");
        var result = await repository.EnumerateAllAsync(
            _executionContext,
            _paginationInfo,
            (ctx, item, pagination, ct) =>
            {
                processedItems.Add(item);
                return Task.FromResult(processedItems.Count < 2);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verifying processing stopped after second item");
        result.ShouldBeTrue();
        processedItems.Count.ShouldBe(2);
        processedItems[0].Name.ShouldBe("Item1");
        processedItems[1].Name.ShouldBe("Item2");
        LogInfo("EnumerateAllAsync stopped when handler returned false");
    }

    [Fact]
    public async Task EnumerateAllAsync_WhenInternalMethodThrows_ShouldReturnFalseAndLogException()
    {
        // Arrange
        LogArrange("Setting up repository to throw exception");
        var processedItems = new List<TestAggregateRoot>();
        var repository = new TestRepository(_loggerMock.Object)
        {
            GetAllInternalShouldThrow = true
        };

        // Act
        LogAct("Calling EnumerateAllAsync");
        var result = await repository.EnumerateAllAsync(
            _executionContext,
            _paginationInfo,
            (ctx, item, pagination, ct) =>
            {
                processedItems.Add(item);
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verifying false result and logging");
        result.ShouldBeFalse();
        processedItems.ShouldBeEmpty();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        LogInfo("EnumerateAllAsync returned false and logged exception");
    }

    [Fact]
    public async Task EnumerateAllAsync_WhenExceptionDuringIteration_ShouldReturnFalseAndLogException()
    {
        // Arrange
        LogArrange("Setting up repository that throws mid-iteration");
        var processedItems = new List<TestAggregateRoot>();
        var repository = new TestRepository(_loggerMock.Object)
        {
            GetAllInternalResult = CreateThrowingAsyncEnumerable()
        };

        // Act
        LogAct("Calling EnumerateAllAsync");
        var result = await repository.EnumerateAllAsync(
            _executionContext,
            _paginationInfo,
            (ctx, item, pagination, ct) =>
            {
                processedItems.Add(item);
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verifying false result after mid-iteration exception");
        result.ShouldBeFalse();
        processedItems.Count.ShouldBe(1);
        processedItems[0].Name.ShouldBe("Item1");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        LogInfo("EnumerateAllAsync caught mid-iteration exception");
    }

    [Fact]
    public async Task EnumerateAllAsync_WhenNoItems_ShouldReturnTrueWithoutCallingHandler()
    {
        // Arrange
        LogArrange("Setting up repository with no items");
        var handlerCalled = false;
        var repository = new TestRepository(_loggerMock.Object)
        {
            GetAllInternalResult = AsyncEnumerable.Empty<TestAggregateRoot>()
        };

        // Act
        LogAct("Calling EnumerateAllAsync");
        var result = await repository.EnumerateAllAsync(
            _executionContext,
            _paginationInfo,
            (ctx, item, pagination, ct) =>
            {
                handlerCalled = true;
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verifying true result without handler calls");
        result.ShouldBeTrue();
        handlerCalled.ShouldBeFalse();
        LogInfo("EnumerateAllAsync returned true for empty enumerable");
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

    #region EnumerateModifiedSinceAsync Tests

    [Fact]
    public async Task EnumerateModifiedSinceAsync_WhenHandlerReturnsTrue_ShouldProcessAllItems()
    {
        // Arrange
        LogArrange("Setting up repository with modified items");
        var expectedItems = new List<TestAggregateRoot>
        {
            new("Modified1"),
            new("Modified2"),
            new("Modified3")
        };
        var processedItems = new List<TestAggregateRoot>();
        var repository = new TestRepository(_loggerMock.Object)
        {
            GetModifiedSinceInternalResult = expectedItems.ToAsyncEnumerable()
        };

        // Act
        LogAct("Calling EnumerateModifiedSinceAsync");
        var result = await repository.EnumerateModifiedSinceAsync(
            _executionContext,
            _timeProvider,
            _sinceDate,
            (ctx, item, tp, since, ct) =>
            {
                processedItems.Add(item);
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verifying all items processed");
        result.ShouldBeTrue();
        processedItems.Count.ShouldBe(3);
        processedItems[0].Name.ShouldBe("Modified1");
        processedItems[1].Name.ShouldBe("Modified2");
        processedItems[2].Name.ShouldBe("Modified3");
        LogInfo("EnumerateModifiedSinceAsync processed all items successfully");
    }

    [Fact]
    public async Task EnumerateModifiedSinceAsync_WhenHandlerReturnsFalse_ShouldStopProcessing()
    {
        // Arrange
        LogArrange("Setting up repository with multiple modified items");
        var expectedItems = new List<TestAggregateRoot>
        {
            new("Modified1"),
            new("Modified2"),
            new("Modified3")
        };
        var processedItems = new List<TestAggregateRoot>();
        var repository = new TestRepository(_loggerMock.Object)
        {
            GetModifiedSinceInternalResult = expectedItems.ToAsyncEnumerable()
        };

        // Act
        LogAct("Calling EnumerateModifiedSinceAsync with handler that stops on first item");
        var result = await repository.EnumerateModifiedSinceAsync(
            _executionContext,
            _timeProvider,
            _sinceDate,
            (ctx, item, tp, since, ct) =>
            {
                processedItems.Add(item);
                return Task.FromResult(false);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verifying processing stopped after first item");
        result.ShouldBeTrue();
        processedItems.Count.ShouldBe(1);
        processedItems[0].Name.ShouldBe("Modified1");
        LogInfo("EnumerateModifiedSinceAsync stopped when handler returned false");
    }

    [Fact]
    public async Task EnumerateModifiedSinceAsync_WhenInternalMethodThrows_ShouldReturnFalseAndLogException()
    {
        // Arrange
        LogArrange("Setting up repository to throw exception");
        var processedItems = new List<TestAggregateRoot>();
        var repository = new TestRepository(_loggerMock.Object)
        {
            GetModifiedSinceInternalShouldThrow = true
        };

        // Act
        LogAct("Calling EnumerateModifiedSinceAsync");
        var result = await repository.EnumerateModifiedSinceAsync(
            _executionContext,
            _timeProvider,
            _sinceDate,
            (ctx, item, tp, since, ct) =>
            {
                processedItems.Add(item);
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verifying false result and logging");
        result.ShouldBeFalse();
        processedItems.ShouldBeEmpty();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        LogInfo("EnumerateModifiedSinceAsync returned false and logged exception");
    }

    [Fact]
    public async Task EnumerateModifiedSinceAsync_WhenNoItems_ShouldReturnTrueWithoutCallingHandler()
    {
        // Arrange
        LogArrange("Setting up repository with no modified items");
        var handlerCalled = false;
        var repository = new TestRepository(_loggerMock.Object)
        {
            GetModifiedSinceInternalResult = AsyncEnumerable.Empty<TestAggregateRoot>()
        };

        // Act
        LogAct("Calling EnumerateModifiedSinceAsync");
        var result = await repository.EnumerateModifiedSinceAsync(
            _executionContext,
            _timeProvider,
            _sinceDate,
            (ctx, item, tp, since, ct) =>
            {
                handlerCalled = true;
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verifying true result without handler calls");
        result.ShouldBeTrue();
        handlerCalled.ShouldBeFalse();
        LogInfo("EnumerateModifiedSinceAsync returned true for empty enumerable");
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

    private static async IAsyncEnumerable<TestAggregateRoot> CreateThrowingAsyncEnumerable()
    {
        yield return new TestAggregateRoot("Item1");
        await Task.Yield();
        throw new InvalidOperationException("Test exception during iteration");
    }

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
