using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Interfaces;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork.Interfaces;
using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Persistence.PostgreSql.Repositories;

public class DataModelRepositoryBaseTests : TestBase
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IPostgreSqlUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDataModelMapper<DataModelBase>> _mapperMock;

    public DataModelRepositoryBaseTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _loggerMock = new Mock<ILogger>();
        _unitOfWorkMock = new Mock<IPostgreSqlUnitOfWork>();
        _mapperMock = new Mock<IDataModelMapper<DataModelBase>>();
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparing to create repository with null logger");

        // Act
        LogAct("Creating repository with null logger");
        Action act = () => new TestableDataModelRepository(
            null!,
            _unitOfWorkMock.Object,
            _mapperMock.Object);

        // Assert
        LogAssert("Verifying ArgumentNullException is thrown");
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(act);
        exception.ParamName.ShouldBe("logger");
    }

    [Fact]
    public void Constructor_WithNullUnitOfWork_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparing to create repository with null unit of work");

        // Act
        LogAct("Creating repository with null unit of work");
        Action act = () => new TestableDataModelRepository(
            _loggerMock.Object,
            null!,
            _mapperMock.Object);

        // Assert
        LogAssert("Verifying ArgumentNullException is thrown");
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(act);
        exception.ParamName.ShouldBe("unitOfWork");
    }

    [Fact]
    public void Constructor_WithNullMapper_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparing to create repository with null mapper");

        // Act
        LogAct("Creating repository with null mapper");
        Action act = () => new TestableDataModelRepository(
            _loggerMock.Object,
            _unitOfWorkMock.Object,
            null!);

        // Assert
        LogAssert("Verifying ArgumentNullException is thrown");
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(act);
        exception.ParamName.ShouldBe("mapper");
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        LogArrange("Preparing to create repository with valid parameters");

        // Act
        LogAct("Creating repository with valid parameters");
        TestableDataModelRepository repository = new(
            _loggerMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object);

        // Assert
        LogAssert("Verifying repository is created successfully");
        repository.ShouldNotBeNull();
    }

    [Fact]
    public void Logger_ShouldReturnInjectedLogger()
    {
        // Arrange
        LogArrange("Creating repository");
        TestableDataModelRepository repository = new(
            _loggerMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object);

        // Act
        LogAct("Getting Logger property");
        ILogger logger = repository.GetLogger();

        // Assert
        LogAssert("Verifying Logger returns injected instance");
        logger.ShouldBeSameAs(_loggerMock.Object);
    }

    [Fact]
    public void UnitOfWork_ShouldReturnInjectedUnitOfWork()
    {
        // Arrange
        LogArrange("Creating repository");
        TestableDataModelRepository repository = new(
            _loggerMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object);

        // Act
        LogAct("Getting UnitOfWork property");
        IPostgreSqlUnitOfWork unitOfWork = repository.GetUnitOfWork();

        // Assert
        LogAssert("Verifying UnitOfWork returns injected instance");
        unitOfWork.ShouldBeSameAs(_unitOfWorkMock.Object);
    }

    [Fact]
    public void Mapper_ShouldReturnInjectedMapper()
    {
        // Arrange
        LogArrange("Creating repository");
        TestableDataModelRepository repository = new(
            _loggerMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object);

        // Act
        LogAct("Getting Mapper property");
        IDataModelMapper<DataModelBase> mapper = repository.GetMapper();

        // Assert
        LogAssert("Verifying Mapper returns injected instance");
        mapper.ShouldBeSameAs(_mapperMock.Object);
    }
}

/// <summary>
/// Testable implementation of DataModelRepositoryBase for unit testing.
/// Uses DataModelBase directly since it's not abstract.
/// </summary>
internal sealed class TestableDataModelRepository : DataModelRepositoryBase<DataModelBase>
{
    public TestableDataModelRepository(
        ILogger logger,
        IPostgreSqlUnitOfWork unitOfWork,
        IDataModelMapper<DataModelBase> mapper)
        : base(logger, unitOfWork, mapper)
    {
    }

    /// <summary>
    /// Exposes the protected Logger property for testing.
    /// </summary>
    public ILogger GetLogger() => Logger;

    /// <summary>
    /// Exposes the protected UnitOfWork property for testing.
    /// </summary>
    public IPostgreSqlUnitOfWork GetUnitOfWork() => UnitOfWork;

    /// <summary>
    /// Exposes the protected Mapper property for testing.
    /// </summary>
    public IDataModelMapper<DataModelBase> GetMapper() => Mapper;
}
