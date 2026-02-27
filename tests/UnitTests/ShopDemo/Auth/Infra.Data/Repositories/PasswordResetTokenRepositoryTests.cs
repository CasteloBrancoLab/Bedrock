using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using ShopDemo.Auth.Domain.Entities.PasswordResetTokens;
using ShopDemo.Auth.Domain.Entities.PasswordResetTokens.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Infra.Data.Repositories;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.Repositories;

public class PasswordResetTokenRepositoryTests : TestBase
{
    private readonly Mock<ILogger<PasswordResetTokenRepository>> _loggerMock;
    private readonly Mock<IPasswordResetTokenPostgreSqlRepository> _postgreSqlRepositoryMock;
    private readonly PasswordResetTokenRepository _repository;

    public PasswordResetTokenRepositoryTests(ITestOutputHelper output) : base(output)
    {
        _loggerMock = new Mock<ILogger<PasswordResetTokenRepository>>();
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _postgreSqlRepositoryMock = new Mock<IPasswordResetTokenPostgreSqlRepository>();
        _repository = new PasswordResetTokenRepository(_loggerMock.Object, _postgreSqlRepositoryMock.Object);
    }

    // Constructor Tests

    [Fact]
    public void Constructor_WhenPostgreSqlRepositoryIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparando logger valido e repositorio PostgreSql nulo");

        // Act
        LogAct("Instanciando PasswordResetTokenRepository com postgreSqlRepository nulo");
        Action act = () => new PasswordResetTokenRepository(_loggerMock.Object, null!);

        // Assert
        LogAssert("Verificando que ArgumentNullException foi lancada");
        act.ShouldThrow<ArgumentNullException>();
    }

    // GetByTokenHashAsync Tests

    [Fact]
    public async Task GetByTokenHashAsync_WhenTokenFound_ShouldReturnToken()
    {
        // Arrange
        LogArrange("Preparando contexto e PasswordResetToken para retorno");
        var executionContext = CreateTestExecutionContext();
        var tokenHash = "abc123hash";
        var token = CreateTestPasswordResetToken(executionContext);
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByTokenHashAsync(executionContext, tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        // Act
        LogAct("Chamando GetByTokenHashAsync");
        var result = await _repository.GetByTokenHashAsync(executionContext, tokenHash, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o PasswordResetToken retornado nao e nulo");
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetByTokenHashAsync_WhenTokenNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Preparando contexto e retorno nulo para token nao encontrado");
        var executionContext = CreateTestExecutionContext();
        var tokenHash = "nonexistent_hash";
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByTokenHashAsync(executionContext, tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PasswordResetToken?)null);

        // Act
        LogAct("Chamando GetByTokenHashAsync");
        var result = await _repository.GetByTokenHashAsync(executionContext, tokenHash, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o resultado retornado e nulo");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByTokenHashAsync_WhenExceptionThrown_ShouldLogAndReturnNull()
    {
        // Arrange
        LogArrange("Preparando contexto e configurando excecao no repositorio PostgreSql");
        var executionContext = CreateTestExecutionContext();
        var tokenHash = "abc123hash";
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByTokenHashAsync(executionContext, tokenHash, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        LogAct("Chamando GetByTokenHashAsync esperando excecao");
        var result = await _repository.GetByTokenHashAsync(executionContext, tokenHash, CancellationToken.None);

        // Assert
        LogAssert("Verificando que null foi retornado e o erro foi logado");
        result.ShouldBeNull();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // UpdateAsync Tests

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateAsync_WhenCalled_ShouldReturnExpectedResult(bool expectedResult)
    {
        // Arrange
        LogArrange("Preparando contexto e PasswordResetToken para atualizar");
        var executionContext = CreateTestExecutionContext();
        var token = CreateTestPasswordResetToken(executionContext);
        _postgreSqlRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        LogAct("Chamando UpdateAsync");
        var result = await _repository.UpdateAsync(executionContext, token, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o resultado retornado e o esperado");
        result.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task UpdateAsync_WhenExceptionThrown_ShouldLogAndReturnFalse()
    {
        // Arrange
        LogArrange("Preparando contexto e configurando excecao no repositorio PostgreSql");
        var executionContext = CreateTestExecutionContext();
        var token = CreateTestPasswordResetToken(executionContext);
        _postgreSqlRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, token, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        LogAct("Chamando UpdateAsync esperando excecao");
        var result = await _repository.UpdateAsync(executionContext, token, CancellationToken.None);

        // Assert
        LogAssert("Verificando que false foi retornado e o erro foi logado");
        result.ShouldBeFalse();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // RevokeAllByUserIdAsync Tests

    [Fact]
    public async Task RevokeAllByUserIdAsync_WhenTokensRevoked_ShouldReturnCount()
    {
        // Arrange
        LogArrange("Preparando contexto e userId para revogar tokens");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        _postgreSqlRepositoryMock
            .Setup(x => x.RevokeAllByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        // Act
        LogAct("Chamando RevokeAllByUserIdAsync");
        var result = await _repository.RevokeAllByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o numero de tokens revogados e o esperado");
        result.ShouldBe(3);
    }

    [Fact]
    public async Task RevokeAllByUserIdAsync_WhenNoTokensFound_ShouldReturnZero()
    {
        // Arrange
        LogArrange("Preparando contexto e userId sem tokens para revogar");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        _postgreSqlRepositoryMock
            .Setup(x => x.RevokeAllByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        LogAct("Chamando RevokeAllByUserIdAsync");
        var result = await _repository.RevokeAllByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que zero tokens foram revogados");
        result.ShouldBe(0);
    }

    [Fact]
    public async Task RevokeAllByUserIdAsync_WhenExceptionThrown_ShouldLogAndReturnZero()
    {
        // Arrange
        LogArrange("Preparando contexto e configurando excecao no repositorio PostgreSql");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        _postgreSqlRepositoryMock
            .Setup(x => x.RevokeAllByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        LogAct("Chamando RevokeAllByUserIdAsync esperando excecao");
        var result = await _repository.RevokeAllByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que zero foi retornado e o erro foi logado");
        result.ShouldBe(0);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // DeleteExpiredAsync Tests

    [Fact]
    public async Task DeleteExpiredAsync_WhenTokensDeleted_ShouldReturnCount()
    {
        // Arrange
        LogArrange("Preparando contexto e data de referencia para deletar tokens expirados");
        var executionContext = CreateTestExecutionContext();
        var referenceDate = DateTimeOffset.UtcNow;
        _postgreSqlRepositoryMock
            .Setup(x => x.DeleteExpiredAsync(executionContext, referenceDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        LogAct("Chamando DeleteExpiredAsync");
        var result = await _repository.DeleteExpiredAsync(executionContext, referenceDate, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o numero de tokens deletados e o esperado");
        result.ShouldBe(5);
    }

    [Fact]
    public async Task DeleteExpiredAsync_WhenNoExpiredTokens_ShouldReturnZero()
    {
        // Arrange
        LogArrange("Preparando contexto e data de referencia sem tokens expirados");
        var executionContext = CreateTestExecutionContext();
        var referenceDate = DateTimeOffset.UtcNow;
        _postgreSqlRepositoryMock
            .Setup(x => x.DeleteExpiredAsync(executionContext, referenceDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        LogAct("Chamando DeleteExpiredAsync");
        var result = await _repository.DeleteExpiredAsync(executionContext, referenceDate, CancellationToken.None);

        // Assert
        LogAssert("Verificando que zero tokens foram deletados");
        result.ShouldBe(0);
    }

    [Fact]
    public async Task DeleteExpiredAsync_WhenExceptionThrown_ShouldLogAndReturnZero()
    {
        // Arrange
        LogArrange("Preparando contexto e configurando excecao no repositorio PostgreSql");
        var executionContext = CreateTestExecutionContext();
        var referenceDate = DateTimeOffset.UtcNow;
        _postgreSqlRepositoryMock
            .Setup(x => x.DeleteExpiredAsync(executionContext, referenceDate, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        LogAct("Chamando DeleteExpiredAsync esperando excecao");
        var result = await _repository.DeleteExpiredAsync(executionContext, referenceDate, CancellationToken.None);

        // Assert
        LogAssert("Verificando que zero foi retornado e o erro foi logado");
        result.ShouldBe(0);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // Base Class Method Tests

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExistsAsync_WhenCalled_ShouldReturnExpectedResult(bool expectedResult)
    {
        // Arrange
        LogArrange("Preparando contexto e id para verificar existencia");
        var executionContext = CreateTestExecutionContext();
        var id = Id.CreateFromExistingInfo(Guid.NewGuid());
        _postgreSqlRepositoryMock
            .Setup(x => x.ExistsAsync(executionContext, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        LogAct("Chamando ExistsAsync");
        var result = await _repository.ExistsAsync(executionContext, id, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o resultado retornado e o esperado");
        result.ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetByIdAsync_WhenCalled_ShouldReturnExpectedResult(bool entityFound)
    {
        // Arrange
        LogArrange("Preparando contexto e id para buscar PasswordResetToken por id");
        var executionContext = CreateTestExecutionContext();
        var id = Id.CreateFromExistingInfo(Guid.NewGuid());
        var token = entityFound ? CreateTestPasswordResetToken(executionContext) : null;
        _postgreSqlRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        // Act
        LogAct("Chamando GetByIdAsync");
        var result = await _repository.GetByIdAsync(executionContext, id, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o resultado retornado e o esperado");
        if (entityFound)
            result.ShouldNotBeNull();
        else
            result.ShouldBeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RegisterNewAsync_WhenCalled_ShouldReturnExpectedResult(bool expectedResult)
    {
        // Arrange
        LogArrange("Preparando contexto e PasswordResetToken para registrar");
        var executionContext = CreateTestExecutionContext();
        var token = CreateTestPasswordResetToken(executionContext);
        _postgreSqlRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        LogAct("Chamando RegisterNewAsync");
        var result = await _repository.RegisterNewAsync(executionContext, token, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o resultado retornado e o esperado");
        result.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task EnumerateAllAsync_WhenCalled_ShouldReturnEmptyAsyncEnumerable()
    {
        // Arrange
        LogArrange("Preparando paginacao e handler para enumerar todos os PasswordResetTokens");
        var paginationInfo = PaginationInfo.All;
        var items = new List<PasswordResetToken>();
        EnumerateAllItemHandler<PasswordResetToken> handler = (_, item, _, _) =>
        {
            items.Add(item);
            return Task.FromResult(true);
        };

        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Chamando EnumerateAllAsync");
        await _repository.EnumerateAllAsync(executionContext, paginationInfo, handler, CancellationToken.None);

        // Assert
        LogAssert("Verificando que nenhum item foi enumerado (stub com yield break)");
        items.ShouldBeEmpty();
    }

    [Fact]
    public async Task EnumerateModifiedSinceAsync_WhenCalled_ShouldReturnEmptyAsyncEnumerable()
    {
        // Arrange
        LogArrange("Preparando contexto e handler para enumerar PasswordResetTokens modificados desde data");
        var executionContext = CreateTestExecutionContext();
        var since = DateTimeOffset.UtcNow.AddDays(-1);
        var items = new List<PasswordResetToken>();
        EnumerateModifiedSinceItemHandler<PasswordResetToken> handler = (_, item, _, _, _) =>
        {
            items.Add(item);
            return Task.FromResult(true);
        };

        // Act
        LogAct("Chamando EnumerateModifiedSinceAsync");
        await _repository.EnumerateModifiedSinceAsync(executionContext, TimeProvider.System, since, handler, CancellationToken.None);

        // Assert
        LogAssert("Verificando que nenhum item foi enumerado (stub com yield break)");
        items.ShouldBeEmpty();
    }

    // Helper Methods

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

    private static PasswordResetToken CreateTestPasswordResetToken(ExecutionContext executionContext)
    {
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo: executionContext.TenantInfo,
            createdAt: DateTimeOffset.UtcNow,
            createdBy: executionContext.ExecutionUser,
            createdCorrelationId: executionContext.CorrelationId,
            createdExecutionOrigin: executionContext.ExecutionOrigin,
            createdBusinessOperationCode: executionContext.BusinessOperationCode,
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(1));
        return PasswordResetToken.CreateFromExistingInfo(
            new CreateFromExistingInfoPasswordResetTokenInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                "token_hash_value",
                DateTimeOffset.UtcNow.AddHours(1),
                false,
                null));
    }
}
