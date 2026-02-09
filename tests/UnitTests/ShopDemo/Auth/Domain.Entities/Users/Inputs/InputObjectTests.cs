using Bedrock.BuildingBlocks.Core.EmailAddresses;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.Users;
using ShopDemo.Auth.Domain.Entities.Users.Enums;
using ShopDemo.Auth.Domain.Entities.Users.Inputs;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.Users.Inputs;

public class InputObjectTests : TestBase
{
    public InputObjectTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region RegisterNewInput Tests

    [Fact]
    public void RegisterNewInput_ShouldStoreEmail()
    {
        // Arrange
        LogArrange("Creating email");
        var email = EmailAddress.CreateNew("test@example.com");
        var passwordHash = PasswordHash.CreateNew([1, 2, 3]);

        // Act
        LogAct("Creating RegisterNewInput");
        var input = new RegisterNewInput(email, passwordHash);

        // Assert
        LogAssert("Verifying email is stored");
        input.Email.Value.ShouldBe("test@example.com");
    }

    [Fact]
    public void RegisterNewInput_ShouldStorePasswordHash()
    {
        // Arrange
        LogArrange("Creating password hash");
        var email = EmailAddress.CreateNew("test@example.com");
        byte[] hashBytes = [10, 20, 30];
        var passwordHash = PasswordHash.CreateNew(hashBytes);

        // Act
        LogAct("Creating RegisterNewInput");
        var input = new RegisterNewInput(email, passwordHash);

        // Assert
        LogAssert("Verifying password hash is stored");
        input.PasswordHash.Value.Span.SequenceEqual(hashBytes).ShouldBeTrue();
    }

    [Fact]
    public void RegisterNewInput_Equality_ShouldWork()
    {
        // Arrange
        LogArrange("Creating two identical inputs");
        var email = EmailAddress.CreateNew("test@example.com");
        byte[] hashBytes = [1, 2, 3];
        var hash1 = PasswordHash.CreateNew(hashBytes);
        var hash2 = PasswordHash.CreateNew(hashBytes);
        var input1 = new RegisterNewInput(email, hash1);
        var input2 = new RegisterNewInput(email, hash2);

        // Act
        LogAct("Comparing inputs");
        bool result = input1 == input2;

        // Assert
        LogAssert("Verifying record struct equality");
        result.ShouldBeTrue();
    }

    #endregion

    #region CreateFromExistingInfoInput Tests

    [Fact]
    public void CreateFromExistingInfoInput_ShouldStoreAllProperties()
    {
        // Arrange
        LogArrange("Creating all input properties");
        var entityInfo = CreateTestEntityInfo();
        string username = "testuser";
        var email = EmailAddress.CreateNew("test@example.com");
        var passwordHash = PasswordHash.CreateNew([1, 2, 3]);
        var status = UserStatus.Active;

        // Act
        LogAct("Creating CreateFromExistingInfoInput");
        var input = new CreateFromExistingInfoInput(entityInfo, username, email, passwordHash, status);

        // Assert
        LogAssert("Verifying all properties are stored");
        input.EntityInfo.ShouldBe(entityInfo);
        input.Username.ShouldBe(username);
        input.Email.Value.ShouldBe("test@example.com");
        input.PasswordHash.Value.Span.SequenceEqual(new byte[] { 1, 2, 3 }).ShouldBeTrue();
        input.Status.ShouldBe(UserStatus.Active);
    }

    #endregion

    #region ChangeStatusInput Tests

    [Fact]
    public void ChangeStatusInput_ShouldStoreNewStatus()
    {
        // Arrange
        LogArrange("Creating status input");

        // Act
        LogAct("Creating ChangeStatusInput");
        var input = new ChangeStatusInput(UserStatus.Suspended);

        // Assert
        LogAssert("Verifying status is stored");
        input.NewStatus.ShouldBe(UserStatus.Suspended);
    }

    [Theory]
    [InlineData(UserStatus.Active)]
    [InlineData(UserStatus.Suspended)]
    [InlineData(UserStatus.Blocked)]
    public void ChangeStatusInput_ShouldAcceptAllStatuses(UserStatus status)
    {
        // Arrange
        LogArrange($"Creating input with status: {status}");

        // Act
        LogAct("Creating ChangeStatusInput");
        var input = new ChangeStatusInput(status);

        // Assert
        LogAssert("Verifying status matches");
        input.NewStatus.ShouldBe(status);
    }

    #endregion

    #region ChangeUsernameInput Tests

    [Fact]
    public void ChangeUsernameInput_ShouldStoreNewUsername()
    {
        // Arrange
        LogArrange("Creating username input");

        // Act
        LogAct("Creating ChangeUsernameInput");
        var input = new ChangeUsernameInput("newuser");

        // Assert
        LogAssert("Verifying username is stored");
        input.NewUsername.ShouldBe("newuser");
    }

    #endregion

    #region ChangePasswordHashInput Tests

    [Fact]
    public void ChangePasswordHashInput_ShouldStoreNewPasswordHash()
    {
        // Arrange
        LogArrange("Creating password hash input");
        byte[] hashBytes = [5, 10, 15];
        var passwordHash = PasswordHash.CreateNew(hashBytes);

        // Act
        LogAct("Creating ChangePasswordHashInput");
        var input = new ChangePasswordHashInput(passwordHash);

        // Assert
        LogAssert("Verifying password hash is stored");
        input.NewPasswordHash.Value.Span.SequenceEqual(hashBytes).ShouldBeTrue();
    }

    #endregion

    #region Helper Methods

    private static EntityInfo CreateTestEntityInfo()
    {
        return EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Test Tenant"),
            entityChangeInfo: EntityChangeInfo.CreateFromExistingInfo(
                createdAt: DateTimeOffset.UtcNow,
                createdBy: "creator",
                createdCorrelationId: Guid.NewGuid(),
                createdExecutionOrigin: "UnitTest",
                createdBusinessOperationCode: "TEST_OP",
                lastChangedAt: null,
                lastChangedBy: null,
                lastChangedCorrelationId: null,
                lastChangedExecutionOrigin: null,
                lastChangedBusinessOperationCode: null),
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));
    }

    #endregion
}
