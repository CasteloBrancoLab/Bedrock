using Bedrock.BuildingBlocks.Core.EmailAddresses;
using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Application.Factories.Messages.Events;
using ShopDemo.Auth.Domain.Entities.Users;
using ShopDemo.Auth.Domain.Entities.Users.Inputs;
using ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Events;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Application.Factories.Messages.Events;

public class UserRegisteredEventFactoryTests : TestBase
{
    public UserRegisteredEventFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void Create_ShouldReturnEventWithCorrectMetadata()
    {
        // Arrange
        LogArrange("Creating execution context, user and time provider");
        var executionContext = CreateTestExecutionContext();
        var user = CreateTestUser(executionContext);
        var email = "test@example.com";

        // Act
        LogAct("Creating UserRegisteredEvent via factory");
        var evt = UserRegisteredEventFactory.Create(
            executionContext, TimeProvider.System, email, user);

        // Assert
        LogAssert("Verifying event metadata matches execution context");
        evt.ShouldNotBeNull();
        evt.ShouldBeOfType<UserRegisteredEvent>();
        evt.Metadata.CorrelationId.ShouldBe(executionContext.CorrelationId);
        evt.Metadata.TenantCode.ShouldBe(executionContext.TenantInfo.Code);
        evt.Metadata.ExecutionUser.ShouldBe(executionContext.ExecutionUser);
        evt.Metadata.MessageId.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void Create_ShouldReturnEventWithCorrectInput()
    {
        // Arrange
        LogArrange("Creating execution context and user");
        var executionContext = CreateTestExecutionContext();
        var user = CreateTestUser(executionContext);
        var email = "test@example.com";

        // Act
        LogAct("Creating UserRegisteredEvent via factory");
        var evt = UserRegisteredEventFactory.Create(
            executionContext, TimeProvider.System, email, user);

        // Assert
        LogAssert("Verifying event input contains the email");
        evt.Input.Email.ShouldBe(email);
    }

    [Fact]
    public void Create_ShouldReturnEventWithCorrectNewState()
    {
        // Arrange
        LogArrange("Creating execution context and user");
        var executionContext = CreateTestExecutionContext();
        var user = CreateTestUser(executionContext);
        var email = "test@example.com";

        // Act
        LogAct("Creating UserRegisteredEvent via factory");
        var evt = UserRegisteredEventFactory.Create(
            executionContext, TimeProvider.System, email, user);

        // Assert
        LogAssert("Verifying event NewState matches user entity");
        evt.NewState.Id.ShouldBe(user.EntityInfo.Id.Value);
        evt.NewState.Username.ShouldBe(user.Username);
        evt.NewState.Email.ShouldBe(user.Email.Value ?? string.Empty);
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
