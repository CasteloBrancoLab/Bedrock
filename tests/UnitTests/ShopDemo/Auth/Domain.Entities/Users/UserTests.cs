using Bedrock.BuildingBlocks.Core.EmailAddresses;
using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
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

using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.Users;

public class UserTests : TestBase
{
    public UserTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region RegisterNew Tests

    [Fact]
    public void RegisterNew_WithValidInput_ShouldCreateUser()
    {
        // Arrange
        LogArrange("Creating execution context and input");
        var executionContext = CreateTestExecutionContext();
        var email = EmailAddress.CreateNew("user@example.com");
        var passwordHash = PasswordHash.CreateNew(CreateValidHashBytes());
        var input = new RegisterNewInput(email, passwordHash);

        // Act
        LogAct("Registering new user");
        var user = User.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying user was created successfully");
        user.ShouldNotBeNull();
        user.Username.ShouldBe("user@example.com");
        user.Email.Value.ShouldBe("user@example.com");
        user.PasswordHash.Value.Span.SequenceEqual(CreateValidHashBytes()).ShouldBeTrue();
        user.Status.ShouldBe(UserStatus.Active);
    }

    [Fact]
    public void RegisterNew_ShouldSetUsernameToLowercaseEmail()
    {
        // Arrange
        LogArrange("Creating input with uppercase email");
        var executionContext = CreateTestExecutionContext();
        var email = EmailAddress.CreateNew("User@EXAMPLE.COM");
        var passwordHash = PasswordHash.CreateNew(CreateValidHashBytes());
        var input = new RegisterNewInput(email, passwordHash);

        // Act
        LogAct("Registering new user");
        var user = User.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying username is lowercase");
        user.ShouldNotBeNull();
        user.Username.ShouldBe("user@example.com");
    }

    [Fact]
    public void RegisterNew_ShouldAlwaysSetStatusToActive()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var email = EmailAddress.CreateNew("test@example.com");
        var passwordHash = PasswordHash.CreateNew(CreateValidHashBytes());
        var input = new RegisterNewInput(email, passwordHash);

        // Act
        LogAct("Registering new user");
        var user = User.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying status is Active");
        user.ShouldNotBeNull();
        user.Status.ShouldBe(UserStatus.Active);
    }

    [Fact]
    public void RegisterNew_ShouldAssignEntityInfo()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var email = EmailAddress.CreateNew("test@example.com");
        var passwordHash = PasswordHash.CreateNew(CreateValidHashBytes());
        var input = new RegisterNewInput(email, passwordHash);

        // Act
        LogAct("Registering new user");
        var user = User.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying EntityInfo is assigned");
        user.ShouldNotBeNull();
        user.EntityInfo.Id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void RegisterNew_WithEmptyPasswordHash_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with empty password hash");
        var executionContext = CreateTestExecutionContext();
        var email = EmailAddress.CreateNew("test@example.com");
        var passwordHash = default(PasswordHash);
        var input = new RegisterNewInput(email, passwordHash);

        // Act
        LogAct("Registering new user with empty hash");
        var user = User.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        user.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithNullEmail_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with null email");
        var executionContext = CreateTestExecutionContext();
        var email = default(EmailAddress);
        var passwordHash = PasswordHash.CreateNew(CreateValidHashBytes());
        var input = new RegisterNewInput(email, passwordHash);

        // Act
        LogAct("Registering new user with null email");
        var user = User.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        user.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region CreateFromExistingInfo Tests

    [Fact]
    public void CreateFromExistingInfo_ShouldCreateUserWithAllProperties()
    {
        // Arrange
        LogArrange("Creating all properties for existing user");
        var entityInfo = CreateTestEntityInfo();
        string username = "existinguser";
        var email = EmailAddress.CreateNew("existing@example.com");
        var passwordHash = PasswordHash.CreateNew(CreateValidHashBytes());
        var status = UserStatus.Suspended;
        var input = new CreateFromExistingInfoInput(entityInfo, username, email, passwordHash, status);

        // Act
        LogAct("Creating user from existing info");
        var user = User.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying all properties are set");
        user.EntityInfo.ShouldBe(entityInfo);
        user.Username.ShouldBe("existinguser");
        user.Email.Value.ShouldBe("existing@example.com");
        user.PasswordHash.Value.Span.SequenceEqual(CreateValidHashBytes()).ShouldBeTrue();
        user.Status.ShouldBe(UserStatus.Suspended);
    }

    [Fact]
    public void CreateFromExistingInfo_ShouldNotValidate()
    {
        // Arrange
        LogArrange("Creating input with empty username (would fail validation)");
        var entityInfo = CreateTestEntityInfo();
        string username = "";
        var email = EmailAddress.CreateNew("test@example.com");
        var passwordHash = PasswordHash.CreateNew(CreateValidHashBytes());
        var input = new CreateFromExistingInfoInput(entityInfo, username, email, passwordHash, UserStatus.Active);

        // Act
        LogAct("Creating user from existing info with empty username");
        var user = User.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying user was created without validation");
        user.ShouldNotBeNull();
        user.Username.ShouldBe("");
    }

    #endregion

    #region ChangeStatus Tests

    [Theory]
    [InlineData(UserStatus.Active, UserStatus.Suspended)]
    [InlineData(UserStatus.Active, UserStatus.Blocked)]
    [InlineData(UserStatus.Suspended, UserStatus.Active)]
    [InlineData(UserStatus.Suspended, UserStatus.Blocked)]
    [InlineData(UserStatus.Blocked, UserStatus.Active)]
    public void ChangeStatus_WithValidTransition_ShouldSucceed(UserStatus from, UserStatus to)
    {
        // Arrange
        LogArrange($"Creating user with status {from}");
        var executionContext = CreateTestExecutionContext();
        var user = CreateTestUser(executionContext, from);
        var input = new ChangeStatusInput(to);

        // Act
        LogAct($"Changing status from {from} to {to}");
        var result = user.ChangeStatus(executionContext, input);

        // Assert
        LogAssert($"Verifying status changed to {to}");
        result.ShouldNotBeNull();
        result.Status.ShouldBe(to);
    }

    [Fact]
    public void ChangeStatus_ShouldReturnNewInstance()
    {
        // Arrange
        LogArrange("Creating active user");
        var executionContext = CreateTestExecutionContext();
        var user = CreateTestUser(executionContext, UserStatus.Active);
        var input = new ChangeStatusInput(UserStatus.Suspended);

        // Act
        LogAct("Changing status");
        var result = user.ChangeStatus(executionContext, input);

        // Assert
        LogAssert("Verifying new instance was returned (clone-modify-return)");
        result.ShouldNotBeNull();
        result.ShouldNotBeSameAs(user);
        user.Status.ShouldBe(UserStatus.Active);
        result.Status.ShouldBe(UserStatus.Suspended);
    }

    [Theory]
    [InlineData(UserStatus.Active)]
    [InlineData(UserStatus.Suspended)]
    [InlineData(UserStatus.Blocked)]
    public void ChangeStatus_ToSameStatus_ShouldReturnNull(UserStatus status)
    {
        // Arrange
        LogArrange($"Creating user with status {status}");
        var executionContext = CreateTestExecutionContext();
        var user = CreateTestUser(executionContext, status);
        var input = new ChangeStatusInput(status);

        // Act
        LogAct("Changing to same status");
        var result = user.ChangeStatus(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned for same-status transition");
        result.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ChangeStatus_BlockedToSuspended_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating blocked user");
        var executionContext = CreateTestExecutionContext();
        var user = CreateTestUser(executionContext, UserStatus.Blocked);
        var input = new ChangeStatusInput(UserStatus.Suspended);

        // Act
        LogAct("Attempting Blocked -> Suspended transition");
        var result = user.ChangeStatus(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned for invalid transition");
        result.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ChangeUsername Tests

    [Fact]
    public void ChangeUsername_WithValidUsername_ShouldSucceed()
    {
        // Arrange
        LogArrange("Creating user");
        var executionContext = CreateTestExecutionContext();
        var user = CreateTestUser(executionContext, UserStatus.Active);
        var input = new ChangeUsernameInput("newusername");

        // Act
        LogAct("Changing username");
        var result = user.ChangeUsername(executionContext, input);

        // Assert
        LogAssert("Verifying username was changed");
        result.ShouldNotBeNull();
        result.Username.ShouldBe("newusername");
    }

    [Fact]
    public void ChangeUsername_ShouldReturnNewInstance()
    {
        // Arrange
        LogArrange("Creating user");
        var executionContext = CreateTestExecutionContext();
        var user = CreateTestUser(executionContext, UserStatus.Active);
        var input = new ChangeUsernameInput("newusername");

        // Act
        LogAct("Changing username");
        var result = user.ChangeUsername(executionContext, input);

        // Assert
        LogAssert("Verifying clone-modify-return pattern");
        result.ShouldNotBeNull();
        result.ShouldNotBeSameAs(user);
    }

    [Fact]
    public void ChangeUsername_WithEmptyUsername_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating user");
        var executionContext = CreateTestExecutionContext();
        var user = CreateTestUser(executionContext, UserStatus.Active);
        var input = new ChangeUsernameInput("");

        // Act
        LogAct("Changing username to empty");
        var result = user.ChangeUsername(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned for empty username");
        result.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ChangeUsername_WithNullUsername_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating user");
        var executionContext = CreateTestExecutionContext();
        var user = CreateTestUser(executionContext, UserStatus.Active);
        var input = new ChangeUsernameInput(null!);

        // Act
        LogAct("Changing username to null");
        var result = user.ChangeUsername(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned for null username");
        result.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ChangeUsername_ExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating user with overly long username");
        var executionContext = CreateTestExecutionContext();
        var user = CreateTestUser(executionContext, UserStatus.Active);
        string longUsername = new('a', UserMetadata.UsernameMaxLength + 1);
        var input = new ChangeUsernameInput(longUsername);

        // Act
        LogAct("Changing username to overly long value");
        var result = user.ChangeUsername(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned for too-long username");
        result.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ChangePasswordHash Tests

    [Fact]
    public void ChangePasswordHash_WithValidHash_ShouldSucceed()
    {
        // Arrange
        LogArrange("Creating user");
        var executionContext = CreateTestExecutionContext();
        var user = CreateTestUser(executionContext, UserStatus.Active);
        byte[] newHashBytes = [99, 88, 77, 66, 55];
        var input = new ChangePasswordHashInput(PasswordHash.CreateNew(newHashBytes));

        // Act
        LogAct("Changing password hash");
        var result = user.ChangePasswordHash(executionContext, input);

        // Assert
        LogAssert("Verifying password hash was changed");
        result.ShouldNotBeNull();
        result.PasswordHash.Value.Span.SequenceEqual(newHashBytes).ShouldBeTrue();
    }

    [Fact]
    public void ChangePasswordHash_ShouldReturnNewInstance()
    {
        // Arrange
        LogArrange("Creating user");
        var executionContext = CreateTestExecutionContext();
        var user = CreateTestUser(executionContext, UserStatus.Active);
        var input = new ChangePasswordHashInput(PasswordHash.CreateNew(CreateValidHashBytes()));

        // Act
        LogAct("Changing password hash");
        var result = user.ChangePasswordHash(executionContext, input);

        // Assert
        LogAssert("Verifying clone-modify-return pattern");
        result.ShouldNotBeNull();
        result.ShouldNotBeSameAs(user);
    }

    [Fact]
    public void ChangePasswordHash_WithEmptyHash_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating user");
        var executionContext = CreateTestExecutionContext();
        var user = CreateTestUser(executionContext, UserStatus.Active);
        var input = new ChangePasswordHashInput(default(PasswordHash));

        // Act
        LogAct("Changing to empty password hash");
        var result = user.ChangePasswordHash(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned for empty hash");
        result.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ChangePasswordHash_ExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating user");
        var executionContext = CreateTestExecutionContext();
        var user = CreateTestUser(executionContext, UserStatus.Active);
        byte[] longHash = new byte[UserMetadata.PasswordHashMaxLength + 1];
        var input = new ChangePasswordHashInput(PasswordHash.CreateNew(longHash));

        // Act
        LogAct("Changing to overly long password hash");
        var result = user.ChangePasswordHash(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned for too-long hash");
        result.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region RegisterNew - Invalid Email Path

    [Fact]
    public void RegisterNew_WithDefaultEmail_ShouldReturnNullAndCollectAllErrors()
    {
        // Arrange
        LogArrange("Creating input with default email to trigger SetEmail validation failure");
        var executionContext = CreateTestExecutionContext();
        var email = default(EmailAddress);
        var passwordHash = PasswordHash.CreateNew(CreateValidHashBytes());
        var input = new RegisterNewInput(email, passwordHash);

        // Act
        LogAct("Registering new user with default email");
        var user = User.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null returned and errors collected");
        user.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
        executionContext.Messages.Count().ShouldBeGreaterThan(0);
    }

    #endregion

    #region RegisterNew - Invalid Status Validation Path

    [Fact]
    public void RegisterNew_ShouldCallSetStatusWithActive()
    {
        // Arrange
        LogArrange("Creating valid input to verify SetStatus is called");
        var executionContext = CreateTestExecutionContext();
        var email = EmailAddress.CreateNew("status@test.com");
        var passwordHash = PasswordHash.CreateNew(CreateValidHashBytes());
        var input = new RegisterNewInput(email, passwordHash);

        // Act
        LogAct("Registering new user");
        var user = User.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying status was set to Active via SetStatus");
        user.ShouldNotBeNull();
        user.Status.ShouldBe(UserStatus.Active);
    }

    #endregion

    #region Instance IsValid (IsValidInternal) Tests

    [Fact]
    public void IsValid_Instance_WithValidUser_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating valid user");
        var executionContext = CreateTestExecutionContext();
        var user = CreateTestUser(executionContext, UserStatus.Active);

        // Act
        LogAct("Calling instance IsValid to trigger IsValidInternal");
        bool result = user.IsValid(executionContext);

        // Assert
        LogAssert("Verifying instance validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_Instance_WithInvalidUser_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating user with invalid state via CreateFromExistingInfo");
        var entityInfo = CreateTestEntityInfo();
        var input = new CreateFromExistingInfoInput(
            entityInfo, "", EmailAddress.CreateNew("test@test.com"),
            PasswordHash.CreateNew(CreateValidHashBytes()), UserStatus.Active);
        var user = User.CreateFromExistingInfo(input);
        var validationContext = CreateTestExecutionContext();

        // Act
        LogAct("Calling instance IsValid on user with empty username");
        bool result = user.IsValid(validationContext);

        // Assert
        LogAssert("Verifying instance validation fails for empty username");
        result.ShouldBeFalse();
        validationContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region Default Property Values

    [Fact]
    public void CreateFromExistingInfo_WithEmptyUsername_ShouldPreserveEmptyString()
    {
        // Arrange
        LogArrange("Creating input to test Username default initializer");
        var entityInfo = CreateTestEntityInfo();
        var input = new CreateFromExistingInfoInput(
            entityInfo, "", EmailAddress.CreateNew("test@test.com"),
            PasswordHash.CreateNew(CreateValidHashBytes()), UserStatus.Active);

        // Act
        LogAct("Creating user from existing info");
        var user = User.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying username is empty string (not mutated default)");
        user.Username.ShouldBe("");
        user.Username.ShouldNotBeNull();
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_ShouldCreateIdenticalCopy()
    {
        // Arrange
        LogArrange("Creating user");
        var executionContext = CreateTestExecutionContext();
        var user = CreateTestUser(executionContext, UserStatus.Active);

        // Act
        LogAct("Cloning user");
        var clone = user.Clone();

        // Assert
        LogAssert("Verifying clone has same values");
        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(user);
        clone.Username.ShouldBe(user.Username);
        clone.Email.Value.ShouldBe(user.Email.Value);
        clone.Status.ShouldBe(user.Status);
    }

    #endregion

    #region ValidateUsername Tests

    [Fact]
    public void ValidateUsername_WithValidUsername_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid username");
        bool result = User.ValidateUsername(executionContext, "validuser");

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateUsername_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null username");
        bool result = User.ValidateUsername(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateUsername_WithEmptyString_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating empty username");
        bool result = User.ValidateUsername(executionContext, "");

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateUsername_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating username at max length");
        var executionContext = CreateTestExecutionContext();
        string username = new('a', UserMetadata.UsernameMaxLength);

        // Act
        LogAct("Validating max-length username");
        bool result = User.ValidateUsername(executionContext, username);

        // Assert
        LogAssert("Verifying validation passes at boundary");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateUsername_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating username exceeding max length");
        var executionContext = CreateTestExecutionContext();
        string username = new('a', UserMetadata.UsernameMaxLength + 1);

        // Act
        LogAct("Validating too-long username");
        bool result = User.ValidateUsername(executionContext, username);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateUsername_AtMinLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating username at min length");
        var executionContext = CreateTestExecutionContext();
        string username = new('a', UserMetadata.UsernameMinLength);

        // Act
        LogAct("Validating min-length username");
        bool result = User.ValidateUsername(executionContext, username);

        // Assert
        LogAssert("Verifying validation passes at min boundary");
        result.ShouldBeTrue();
    }

    #endregion

    #region ValidateEmail Tests

    [Fact]
    public void ValidateEmail_WithValidEmail_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();
        var email = EmailAddress.CreateNew("test@example.com");

        // Act
        LogAct("Validating valid email");
        bool result = User.ValidateEmail(executionContext, email);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateEmail_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null email");
        bool result = User.ValidateEmail(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidatePasswordHash Tests

    [Fact]
    public void ValidatePasswordHash_WithValidHash_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();
        var hash = PasswordHash.CreateNew(CreateValidHashBytes());

        // Act
        LogAct("Validating valid password hash");
        bool result = User.ValidatePasswordHash(executionContext, hash);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidatePasswordHash_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null password hash");
        bool result = User.ValidatePasswordHash(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidatePasswordHash_WithEmptyValue_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();
        var hash = PasswordHash.CreateNew([]);

        // Act
        LogAct("Validating empty password hash");
        bool result = User.ValidatePasswordHash(executionContext, hash);

        // Assert
        LogAssert("Verifying validation fails for empty hash");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidatePasswordHash_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating hash at max length");
        var executionContext = CreateTestExecutionContext();
        byte[] hashBytes = new byte[UserMetadata.PasswordHashMaxLength];
        hashBytes[0] = 1; // ensure non-empty
        var hash = PasswordHash.CreateNew(hashBytes);

        // Act
        LogAct("Validating max-length password hash");
        bool result = User.ValidatePasswordHash(executionContext, hash);

        // Assert
        LogAssert("Verifying validation passes at boundary");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidatePasswordHash_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating hash exceeding max length");
        var executionContext = CreateTestExecutionContext();
        byte[] hashBytes = new byte[UserMetadata.PasswordHashMaxLength + 1];
        hashBytes[0] = 1;
        var hash = PasswordHash.CreateNew(hashBytes);

        // Act
        LogAct("Validating too-long password hash");
        bool result = User.ValidatePasswordHash(executionContext, hash);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateStatus Tests

    [Theory]
    [InlineData(UserStatus.Active)]
    [InlineData(UserStatus.Suspended)]
    [InlineData(UserStatus.Blocked)]
    public void ValidateStatus_WithValidStatus_ShouldReturnTrue(UserStatus status)
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct($"Validating status: {status}");
        bool result = User.ValidateStatus(executionContext, status);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateStatus_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null status");
        bool result = User.ValidateStatus(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateStatusTransition Tests

    [Theory]
    [InlineData(UserStatus.Active, UserStatus.Suspended)]
    [InlineData(UserStatus.Active, UserStatus.Blocked)]
    [InlineData(UserStatus.Suspended, UserStatus.Active)]
    [InlineData(UserStatus.Suspended, UserStatus.Blocked)]
    [InlineData(UserStatus.Blocked, UserStatus.Active)]
    public void ValidateStatusTransition_ValidTransitions_ShouldReturnTrue(UserStatus from, UserStatus to)
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct($"Validating transition {from} -> {to}");
        bool result = User.ValidateStatusTransition(executionContext, from, to);

        // Assert
        LogAssert("Verifying transition is valid");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateStatusTransition_BlockedToSuspended_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating Blocked -> Suspended transition");
        bool result = User.ValidateStatusTransition(executionContext, UserStatus.Blocked, UserStatus.Suspended);

        // Assert
        LogAssert("Verifying transition is invalid");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Theory]
    [InlineData(UserStatus.Active)]
    [InlineData(UserStatus.Suspended)]
    [InlineData(UserStatus.Blocked)]
    public void ValidateStatusTransition_SameStatus_ShouldReturnFalse(UserStatus status)
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct($"Validating {status} -> {status} transition");
        bool result = User.ValidateStatusTransition(executionContext, status, status);

        // Assert
        LogAssert("Verifying same-status transition is invalid");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateStatusTransition_WithNullFrom_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null -> Active transition");
        bool result = User.ValidateStatusTransition(executionContext, null, UserStatus.Active);

        // Assert
        LogAssert("Verifying null from is invalid");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateStatusTransition_WithNullTo_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating Active -> null transition");
        bool result = User.ValidateStatusTransition(executionContext, UserStatus.Active, null);

        // Assert
        LogAssert("Verifying null to is invalid");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region IsValid Tests

    [Fact]
    public void IsValid_WithAllValidFields_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating all valid fields");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();
        string username = "validuser";
        var email = EmailAddress.CreateNew("test@example.com");
        var passwordHash = PasswordHash.CreateNew(CreateValidHashBytes());
        UserStatus status = UserStatus.Active;

        // Act
        LogAct("Calling IsValid");
        bool result = User.IsValid(executionContext, entityInfo, username, email, passwordHash, status);

        // Assert
        LogAssert("Verifying all fields are valid");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WithNullUsername_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null username");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();
        var email = EmailAddress.CreateNew("test@example.com");
        var passwordHash = PasswordHash.CreateNew(CreateValidHashBytes());

        // Act
        LogAct("Calling IsValid with null username");
        bool result = User.IsValid(executionContext, entityInfo, null, email, passwordHash, UserStatus.Active);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullEmail_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null email");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();
        var passwordHash = PasswordHash.CreateNew(CreateValidHashBytes());

        // Act
        LogAct("Calling IsValid with null email");
        bool result = User.IsValid(executionContext, entityInfo, "user", null, passwordHash, UserStatus.Active);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullPasswordHash_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null password hash");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();
        var email = EmailAddress.CreateNew("test@example.com");

        // Act
        LogAct("Calling IsValid with null password hash");
        bool result = User.IsValid(executionContext, entityInfo, "user", email, null, UserStatus.Active);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullStatus_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null status");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();
        var email = EmailAddress.CreateNew("test@example.com");
        var passwordHash = PasswordHash.CreateNew(CreateValidHashBytes());

        // Act
        LogAct("Calling IsValid with null status");
        bool result = User.IsValid(executionContext, entityInfo, "user", email, passwordHash, null);

        // Assert
        LogAssert("Verifying validation fails");
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

    private static User CreateTestUser(ExecutionContext executionContext, UserStatus status)
    {
        var email = EmailAddress.CreateNew("test@example.com");
        var passwordHash = PasswordHash.CreateNew(CreateValidHashBytes());
        var input = new RegisterNewInput(email, passwordHash);
        var user = User.RegisterNew(executionContext, input)!;

        if (status != UserStatus.Active)
        {
            var changeStatusInput = new ChangeStatusInput(status);
            user = user.ChangeStatus(executionContext, changeStatusInput)!;
        }

        return user;
    }

    private static byte[] CreateValidHashBytes()
    {
        byte[] bytes = new byte[49];
        bytes[0] = 1; // pepper version
        for (int i = 1; i < bytes.Length; i++)
        {
            bytes[i] = (byte)(i % 256);
        }
        return bytes;
    }

    #endregion
}
