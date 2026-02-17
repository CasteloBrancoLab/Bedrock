using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using RefreshTokenMetadata = ShopDemo.Auth.Domain.Entities.RefreshTokens.RefreshToken.RefreshTokenMetadata;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.RefreshTokens;

public class RefreshTokenMetadataTests : TestBase
{
    public RefreshTokenMetadataTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region Property Name Tests

    [Fact]
    public void UserIdPropertyName_ShouldBeUserId()
    {
        // Arrange & Act
        LogAct("Reading UserIdPropertyName");
        string name = RefreshTokenMetadata.UserIdPropertyName;

        // Assert
        LogAssert("Verifying property name");
        name.ShouldBe("UserId");
    }

    [Fact]
    public void TokenHashPropertyName_ShouldBeTokenHash()
    {
        // Arrange & Act
        LogAct("Reading TokenHashPropertyName");
        string name = RefreshTokenMetadata.TokenHashPropertyName;

        // Assert
        LogAssert("Verifying property name");
        name.ShouldBe("TokenHash");
    }

    [Fact]
    public void FamilyIdPropertyName_ShouldBeFamilyId()
    {
        // Arrange & Act
        LogAct("Reading FamilyIdPropertyName");
        string name = RefreshTokenMetadata.FamilyIdPropertyName;

        // Assert
        LogAssert("Verifying property name");
        name.ShouldBe("FamilyId");
    }

    [Fact]
    public void ExpiresAtPropertyName_ShouldBeExpiresAt()
    {
        // Arrange & Act
        LogAct("Reading ExpiresAtPropertyName");
        string name = RefreshTokenMetadata.ExpiresAtPropertyName;

        // Assert
        LogAssert("Verifying property name");
        name.ShouldBe("ExpiresAt");
    }

    [Fact]
    public void StatusPropertyName_ShouldBeStatus()
    {
        // Arrange & Act
        LogAct("Reading StatusPropertyName");
        string name = RefreshTokenMetadata.StatusPropertyName;

        // Assert
        LogAssert("Verifying property name");
        name.ShouldBe("Status");
    }

    #endregion

    #region Default Value Tests

    [Fact]
    public void UserIdIsRequired_Default_ShouldBeTrue()
    {
        // Arrange & Act
        LogAct("Reading UserIdIsRequired default");

        // Assert
        LogAssert("Verifying default is true");
        RefreshTokenMetadata.UserIdIsRequired.ShouldBeTrue();
    }

    [Fact]
    public void TokenHashIsRequired_Default_ShouldBeTrue()
    {
        // Arrange & Act
        LogAct("Reading TokenHashIsRequired default");

        // Assert
        LogAssert("Verifying default is true");
        RefreshTokenMetadata.TokenHashIsRequired.ShouldBeTrue();
    }

    [Fact]
    public void TokenHashMaxLength_Default_ShouldBe64()
    {
        // Arrange & Act
        LogAct("Reading TokenHashMaxLength default");

        // Assert
        LogAssert("Verifying default is 64");
        RefreshTokenMetadata.TokenHashMaxLength.ShouldBe(64);
    }

    [Fact]
    public void FamilyIdIsRequired_Default_ShouldBeTrue()
    {
        // Arrange & Act
        LogAct("Reading FamilyIdIsRequired default");

        // Assert
        LogAssert("Verifying default is true");
        RefreshTokenMetadata.FamilyIdIsRequired.ShouldBeTrue();
    }

    [Fact]
    public void ExpiresAtIsRequired_Default_ShouldBeTrue()
    {
        // Arrange & Act
        LogAct("Reading ExpiresAtIsRequired default");

        // Assert
        LogAssert("Verifying default is true");
        RefreshTokenMetadata.ExpiresAtIsRequired.ShouldBeTrue();
    }

    [Fact]
    public void StatusIsRequired_Default_ShouldBeTrue()
    {
        // Arrange & Act
        LogAct("Reading StatusIsRequired default");

        // Assert
        LogAssert("Verifying default is true");
        RefreshTokenMetadata.StatusIsRequired.ShouldBeTrue();
    }

    #endregion

    #region ChangeUserIdMetadata Tests

    [Fact]
    public void ChangeUserIdMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original value");
        bool originalIsRequired = RefreshTokenMetadata.UserIdIsRequired;

        try
        {
            // Act
            LogAct("Changing UserId metadata");
            RefreshTokenMetadata.ChangeUserIdMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying updated value");
            RefreshTokenMetadata.UserIdIsRequired.ShouldBeFalse();
        }
        finally
        {
            RefreshTokenMetadata.ChangeUserIdMetadata(originalIsRequired);
        }
    }

    #endregion

    #region ChangeTokenHashMetadata Tests

    [Fact]
    public void ChangeTokenHashMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original values");
        bool originalIsRequired = RefreshTokenMetadata.TokenHashIsRequired;
        int originalMaxLength = RefreshTokenMetadata.TokenHashMaxLength;

        try
        {
            // Act
            LogAct("Changing TokenHash metadata");
            RefreshTokenMetadata.ChangeTokenHashMetadata(
                isRequired: false,
                maxLength: 128
            );

            // Assert
            LogAssert("Verifying updated values");
            RefreshTokenMetadata.TokenHashIsRequired.ShouldBeFalse();
            RefreshTokenMetadata.TokenHashMaxLength.ShouldBe(128);
        }
        finally
        {
            RefreshTokenMetadata.ChangeTokenHashMetadata(originalIsRequired, originalMaxLength);
        }
    }

    #endregion

    #region ChangeFamilyIdMetadata Tests

    [Fact]
    public void ChangeFamilyIdMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original value");
        bool originalIsRequired = RefreshTokenMetadata.FamilyIdIsRequired;

        try
        {
            // Act
            LogAct("Changing FamilyId metadata");
            RefreshTokenMetadata.ChangeFamilyIdMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying updated value");
            RefreshTokenMetadata.FamilyIdIsRequired.ShouldBeFalse();
        }
        finally
        {
            RefreshTokenMetadata.ChangeFamilyIdMetadata(originalIsRequired);
        }
    }

    #endregion

    #region ChangeExpiresAtMetadata Tests

    [Fact]
    public void ChangeExpiresAtMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original value");
        bool originalIsRequired = RefreshTokenMetadata.ExpiresAtIsRequired;

        try
        {
            // Act
            LogAct("Changing ExpiresAt metadata");
            RefreshTokenMetadata.ChangeExpiresAtMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying updated value");
            RefreshTokenMetadata.ExpiresAtIsRequired.ShouldBeFalse();
        }
        finally
        {
            RefreshTokenMetadata.ChangeExpiresAtMetadata(originalIsRequired);
        }
    }

    #endregion

    #region ChangeStatusMetadata Tests

    [Fact]
    public void ChangeStatusMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original value");
        bool originalIsRequired = RefreshTokenMetadata.StatusIsRequired;

        try
        {
            // Act
            LogAct("Changing Status metadata");
            RefreshTokenMetadata.ChangeStatusMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying updated value");
            RefreshTokenMetadata.StatusIsRequired.ShouldBeFalse();
        }
        finally
        {
            RefreshTokenMetadata.ChangeStatusMetadata(originalIsRequired);
        }
    }

    #endregion
}
