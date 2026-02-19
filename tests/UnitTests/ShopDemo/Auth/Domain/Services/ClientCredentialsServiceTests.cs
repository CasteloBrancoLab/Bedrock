using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Security.Passwords.Interfaces;
using Bedrock.BuildingBlocks.Testing;
using Moq;
using ShopDemo.Auth.Domain.Entities.ServiceClients;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Services;
using ShopDemo.Auth.Domain.Services.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Services;

public class ClientCredentialsServiceTests : TestBase
{
    private readonly Mock<IServiceClientRepository> _serviceClientRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly ClientCredentialsService _sut;

    public ClientCredentialsServiceTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _serviceClientRepositoryMock = new Mock<IServiceClientRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _sut = new ClientCredentialsService(_serviceClientRepositoryMock.Object, _passwordHasherMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullServiceClientRepository_ShouldThrow()
    {
        // Act & Assert
        LogAct("Creating ClientCredentialsService with null repository");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(() => new ClientCredentialsService(null!, _passwordHasherMock.Object));
    }

    [Fact]
    public void Constructor_WithNullPasswordHasher_ShouldThrow()
    {
        // Act & Assert
        LogAct("Creating ClientCredentialsService with null password hasher");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(() => new ClientCredentialsService(_serviceClientRepositoryMock.Object, null!));
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void ShouldImplementIClientCredentialsService()
    {
        LogAssert("Verifying interface implementation");
        _sut.ShouldBeAssignableTo<IClientCredentialsService>();
    }

    #endregion

    #region ValidateCredentialsAsync Tests

    [Fact]
    public async Task ValidateCredentialsAsync_WhenClientNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up repository to return null");
        var executionContext = CreateTestExecutionContext();
        _serviceClientRepositoryMock
            .Setup(x => x.GetByClientIdAsync(executionContext, "test-client", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceClient?)null);

        // Act
        LogAct("Validating credentials for non-existent client");
        var result = await _sut.ValidateCredentialsAsync(executionContext, "test-client", "secret", CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned");
        result.ShouldBeNull();
        _passwordHasherMock.Verify(
            x => x.VerifyPassword(It.IsAny<ExecutionContext>(), It.IsAny<string>(), It.IsAny<byte[]>()),
            Times.Never);
    }

    #endregion

    #region Helper Methods

    private static ExecutionContext CreateTestExecutionContext()
    {
        var tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Test Tenant");
        return ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: tenantInfo,
            executionUser: "test.user",
            executionOrigin: "UnitTest",
            businessOperationCode: "TEST_OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: TimeProvider.System);
    }

    #endregion
}
