using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.Users;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.Users;

public class UserMetadataTests : TestBase
{
    public UserMetadataTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region Property Name Tests

    [Fact]
    public void UsernamePropertyName_ShouldBeUsername()
    {
        // Arrange & Act
        LogAct("Reading UsernamePropertyName");
        string name = UserMetadata.UsernamePropertyName;

        // Assert
        LogAssert("Verifying property name");
        name.ShouldBe("Username");
    }

    [Fact]
    public void EmailPropertyName_ShouldBeEmail()
    {
        // Arrange & Act
        LogAct("Reading EmailPropertyName");
        string name = UserMetadata.EmailPropertyName;

        // Assert
        LogAssert("Verifying property name");
        name.ShouldBe("Email");
    }

    [Fact]
    public void PasswordHashPropertyName_ShouldBePasswordHash()
    {
        // Arrange & Act
        LogAct("Reading PasswordHashPropertyName");
        string name = UserMetadata.PasswordHashPropertyName;

        // Assert
        LogAssert("Verifying property name");
        name.ShouldBe("PasswordHash");
    }

    [Fact]
    public void StatusPropertyName_ShouldBeStatus()
    {
        // Arrange & Act
        LogAct("Reading StatusPropertyName");
        string name = UserMetadata.StatusPropertyName;

        // Assert
        LogAssert("Verifying property name");
        name.ShouldBe("Status");
    }

    #endregion

    #region Default Value Tests

    [Fact]
    public void UsernameIsRequired_Default_ShouldBeTrue()
    {
        // Arrange & Act
        LogAct("Reading UsernameIsRequired default");

        // Assert
        LogAssert("Verifying default is true");
        UserMetadata.UsernameIsRequired.ShouldBeTrue();
    }

    [Fact]
    public void UsernameMinLength_Default_ShouldBe1()
    {
        // Arrange & Act
        LogAct("Reading UsernameMinLength default");

        // Assert
        LogAssert("Verifying default is 1");
        UserMetadata.UsernameMinLength.ShouldBe(1);
    }

    [Fact]
    public void UsernameMaxLength_Default_ShouldBe255()
    {
        // Arrange & Act
        LogAct("Reading UsernameMaxLength default");

        // Assert
        LogAssert("Verifying default is 255");
        UserMetadata.UsernameMaxLength.ShouldBe(255);
    }

    [Fact]
    public void EmailIsRequired_Default_ShouldBeTrue()
    {
        // Arrange & Act
        LogAct("Reading EmailIsRequired default");

        // Assert
        LogAssert("Verifying default is true");
        UserMetadata.EmailIsRequired.ShouldBeTrue();
    }

    [Fact]
    public void PasswordHashIsRequired_Default_ShouldBeTrue()
    {
        // Arrange & Act
        LogAct("Reading PasswordHashIsRequired default");

        // Assert
        LogAssert("Verifying default is true");
        UserMetadata.PasswordHashIsRequired.ShouldBeTrue();
    }

    [Fact]
    public void PasswordHashMaxLength_Default_ShouldBe128()
    {
        // Arrange & Act
        LogAct("Reading PasswordHashMaxLength default");

        // Assert
        LogAssert("Verifying default is 128");
        UserMetadata.PasswordHashMaxLength.ShouldBe(128);
    }

    [Fact]
    public void StatusIsRequired_Default_ShouldBeTrue()
    {
        // Arrange & Act
        LogAct("Reading StatusIsRequired default");

        // Assert
        LogAssert("Verifying default is true");
        UserMetadata.StatusIsRequired.ShouldBeTrue();
    }

    #endregion

    #region ChangeUsernameMetadata Tests

    [Fact]
    public void ChangeUsernameMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original values");
        bool originalIsRequired = UserMetadata.UsernameIsRequired;
        int originalMinLength = UserMetadata.UsernameMinLength;
        int originalMaxLength = UserMetadata.UsernameMaxLength;

        try
        {
            // Act
            LogAct("Changing username metadata");
            UserMetadata.ChangeUsernameMetadata(
                isRequired: false,
                minLength: 5,
                maxLength: 50
            );

            // Assert
            LogAssert("Verifying updated values");
            UserMetadata.UsernameIsRequired.ShouldBeFalse();
            UserMetadata.UsernameMinLength.ShouldBe(5);
            UserMetadata.UsernameMaxLength.ShouldBe(50);
        }
        finally
        {
            UserMetadata.ChangeUsernameMetadata(originalIsRequired, originalMinLength, originalMaxLength);
        }
    }

    #endregion

    #region ChangeEmailMetadata Tests

    [Fact]
    public void ChangeEmailMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original value");
        bool originalIsRequired = UserMetadata.EmailIsRequired;

        try
        {
            // Act
            LogAct("Changing email metadata");
            UserMetadata.ChangeEmailMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying updated value");
            UserMetadata.EmailIsRequired.ShouldBeFalse();
        }
        finally
        {
            UserMetadata.ChangeEmailMetadata(originalIsRequired);
        }
    }

    #endregion

    #region ChangePasswordHashMetadata Tests

    [Fact]
    public void ChangePasswordHashMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original values");
        bool originalIsRequired = UserMetadata.PasswordHashIsRequired;
        int originalMaxLength = UserMetadata.PasswordHashMaxLength;

        try
        {
            // Act
            LogAct("Changing password hash metadata");
            UserMetadata.ChangePasswordHashMetadata(
                isRequired: false,
                maxLength: 256
            );

            // Assert
            LogAssert("Verifying updated values");
            UserMetadata.PasswordHashIsRequired.ShouldBeFalse();
            UserMetadata.PasswordHashMaxLength.ShouldBe(256);
        }
        finally
        {
            UserMetadata.ChangePasswordHashMetadata(originalIsRequired, originalMaxLength);
        }
    }

    #endregion

    #region ChangeStatusMetadata Tests

    [Fact]
    public void ChangeStatusMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original value");
        bool originalIsRequired = UserMetadata.StatusIsRequired;

        try
        {
            // Act
            LogAct("Changing status metadata");
            UserMetadata.ChangeStatusMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying updated value");
            UserMetadata.StatusIsRequired.ShouldBeFalse();
        }
        finally
        {
            UserMetadata.ChangeStatusMetadata(originalIsRequired);
        }
    }

    #endregion
}
