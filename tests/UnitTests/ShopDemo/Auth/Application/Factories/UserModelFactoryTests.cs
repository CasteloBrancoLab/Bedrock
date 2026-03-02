using Bedrock.BuildingBlocks.Core.EmailAddresses;
using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Application.Factories;
using ShopDemo.Auth.Domain.Entities.Users;
using ShopDemo.Auth.Domain.Entities.Users.Inputs;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Application.Factories;

public class UserModelFactoryTests : TestBase
{
    public UserModelFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void FromEntity_WithValidInputs_ShouldReturnModelWithCorrectFields()
    {
        // Arrange
        LogArrange("Creating test user and execution context");
        var executionContext = CreateTestExecutionContext();
        var user = CreateTestUser(executionContext);

        // Act
        LogAct("Mapping user entity to UserModel");
        var model = UserModelFactory.FromEntity(user, executionContext);

        // Assert
        LogAssert("Verifying all model fields match entity");
        model.Id.ShouldBe(user.EntityInfo.Id.Value);
        model.TenantCode.ShouldBe(executionContext.TenantInfo.Code);
        model.Username.ShouldBe(user.Username);
        model.Email.ShouldBe(user.Email.Value ?? string.Empty);
        model.Status.ShouldBe(user.Status.ToString());
        model.CreatedAt.ShouldBe(user.EntityInfo.EntityChangeInfo.CreatedAt);
        model.CreatedBy.ShouldBe(user.EntityInfo.EntityChangeInfo.CreatedBy);
        model.LastChangedAt.ShouldBe(user.EntityInfo.EntityChangeInfo.LastChangedAt);
        model.LastChangedBy.ShouldBe(user.EntityInfo.EntityChangeInfo.LastChangedBy);
    }

    [Fact]
    public void FromEntity_WithNullUser_ShouldThrow()
    {
        // Act & Assert
        LogAct("Calling FromEntity with null user");
        LogAssert("Verifying ArgumentNullException is thrown");
        var executionContext = CreateTestExecutionContext();
        Should.Throw<ArgumentNullException>(() =>
            UserModelFactory.FromEntity(null!, executionContext));
    }

    [Fact]
    public void FromEntity_WithNullExecutionContext_ShouldThrow()
    {
        // Act & Assert
        LogAct("Calling FromEntity with null execution context");
        LogAssert("Verifying ArgumentNullException is thrown");
        var executionContext = CreateTestExecutionContext();
        var user = CreateTestUser(executionContext);
        Should.Throw<ArgumentNullException>(() =>
            UserModelFactory.FromEntity(user, null!));
    }

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

    private static User CreateTestUser(ExecutionContext executionContext)
    {
        var email = EmailAddress.CreateNew("test@example.com");
        var passwordHash = PasswordHash.CreateNew(CreateValidHashBytes());
        var input = new RegisterNewInput(email, passwordHash);
        return User.RegisterNew(executionContext, input)!;
    }

    private static byte[] CreateValidHashBytes()
    {
        byte[] bytes = new byte[49];
        bytes[0] = 1;
        for (int i = 1; i < bytes.Length; i++) bytes[i] = (byte)(i % 256);
        return bytes;
    }
}
