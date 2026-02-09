using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Security.Passwords;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Security.Passwords;

public class PasswordHasherTests : TestBase
{
    private const int ExpectedHashLength = 49; // 1 (pepper version) + 16 (salt) + 32 (hash)

    public PasswordHasherTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region HashPassword Tests

    [Fact]
    public void HashPassword_ShouldProduceCorrectLengthHash()
    {
        // Arrange
        LogArrange("Creating hasher with pepper config");
        var hasher = CreateTestHasher();
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Hashing password");
        var result = hasher.HashPassword(executionContext, "ValidPassword1!");

        // Assert
        LogAssert("Verifying hash length is 49 bytes");
        result.Hash.Length.ShouldBe(ExpectedHashLength);
    }

    [Fact]
    public void HashPassword_ShouldEmbedCorrectPepperVersion()
    {
        // Arrange
        LogArrange("Creating hasher with pepper version 1");
        var hasher = CreateTestHasher(activePepperVersion: 1);
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Hashing password");
        var result = hasher.HashPassword(executionContext, "ValidPassword1!");

        // Assert
        LogAssert("Verifying pepper version in hash and result");
        result.PepperVersion.ShouldBe(1);
        result.Hash[0].ShouldBe((byte)1);
    }

    [Fact]
    public void HashPassword_SamePasswordProducesDifferentHashes()
    {
        // Arrange
        LogArrange("Creating hasher");
        var hasher = CreateTestHasher();
        var executionContext = CreateTestExecutionContext();
        string password = "ValidPassword1!";

        // Act
        LogAct("Hashing same password twice");
        var result1 = hasher.HashPassword(executionContext, password);
        var result2 = hasher.HashPassword(executionContext, password);

        // Assert
        LogAssert("Verifying different hashes due to random salt");
        result1.Hash.AsSpan().SequenceEqual(result2.Hash).ShouldBeFalse();
    }

    [Fact]
    public void HashPassword_WithDifferentPepperVersion_ShouldEmbedVersion()
    {
        // Arrange
        LogArrange("Creating hasher with pepper version 2");
        var hasher = CreateTestHasher(activePepperVersion: 2);
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Hashing password");
        var result = hasher.HashPassword(executionContext, "ValidPassword1!");

        // Assert
        LogAssert("Verifying pepper version 2 is embedded");
        result.PepperVersion.ShouldBe(2);
        result.Hash[0].ShouldBe((byte)2);
    }

    #endregion

    #region VerifyPassword Tests

    [Fact]
    public void VerifyPassword_CorrectPassword_ShouldReturnValid()
    {
        // Arrange
        LogArrange("Creating hasher and hashing a password");
        var hasher = CreateTestHasher();
        var executionContext = CreateTestExecutionContext();
        string password = "CorrectPassword!";
        var hashResult = hasher.HashPassword(executionContext, password);

        // Act
        LogAct("Verifying with correct password");
        var result = hasher.VerifyPassword(executionContext, password, hashResult.Hash);

        // Assert
        LogAssert("Verifying password is valid");
        result.IsValid.ShouldBeTrue();
        result.NeedsRehash.ShouldBeFalse();
    }

    [Fact]
    public void VerifyPassword_IncorrectPassword_ShouldReturnInvalid()
    {
        // Arrange
        LogArrange("Creating hasher and hashing a password");
        var hasher = CreateTestHasher();
        var executionContext = CreateTestExecutionContext();
        var hashResult = hasher.HashPassword(executionContext, "CorrectPassword!");

        // Act
        LogAct("Verifying with incorrect password");
        var result = hasher.VerifyPassword(executionContext, "WrongPassword!", hashResult.Hash);

        // Assert
        LogAssert("Verifying password is invalid");
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void VerifyPassword_WithNullStoredHash_ShouldReturnInvalid()
    {
        // Arrange
        LogArrange("Creating hasher");
        var hasher = CreateTestHasher();
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Verifying with null stored hash");
        var result = hasher.VerifyPassword(executionContext, "password", null!);

        // Assert
        LogAssert("Verifying result is invalid");
        result.IsValid.ShouldBeFalse();
        result.NeedsRehash.ShouldBeFalse();
    }

    [Fact]
    public void VerifyPassword_WithWrongLengthHash_ShouldReturnInvalid()
    {
        // Arrange
        LogArrange("Creating hasher");
        var hasher = CreateTestHasher();
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Verifying with wrong-length hash");
        var result = hasher.VerifyPassword(executionContext, "password", new byte[10]);

        // Assert
        LogAssert("Verifying result is invalid");
        result.IsValid.ShouldBeFalse();
        result.NeedsRehash.ShouldBeFalse();
    }

    [Fact]
    public void VerifyPassword_WithUnknownPepperVersion_ShouldReturnInvalid()
    {
        // Arrange
        LogArrange("Creating hasher and fabricating hash with unknown pepper version");
        var hasher = CreateTestHasher();
        var executionContext = CreateTestExecutionContext();
        byte[] fakeHash = new byte[ExpectedHashLength];
        fakeHash[0] = 99; // unknown pepper version

        // Act
        LogAct("Verifying with unknown pepper version");
        var result = hasher.VerifyPassword(executionContext, "password", fakeHash);

        // Assert
        LogAssert("Verifying result is invalid");
        result.IsValid.ShouldBeFalse();
        result.NeedsRehash.ShouldBeFalse();
    }

    #endregion

    #region Pepper Rotation Tests

    [Fact]
    public void VerifyPassword_WithOldPepperVersion_ShouldReturnValidWithNeedsRehash()
    {
        // Arrange
        LogArrange("Hashing with pepper v1, then creating hasher with v2 active");
        var pepper1 = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        var pepper2 = new byte[] { 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };

        var hasherV1 = new PasswordHasher(new PepperConfiguration(1, new Dictionary<int, byte[]>
        {
            { 1, pepper1 }
        }));
        var executionContext = CreateTestExecutionContext();
        string password = "MyPassword123!";

        var hashResult = hasherV1.HashPassword(executionContext, password);

        var hasherV2 = new PasswordHasher(new PepperConfiguration(2, new Dictionary<int, byte[]>
        {
            { 1, pepper1 },
            { 2, pepper2 }
        }));

        // Act
        LogAct("Verifying hash from v1 with hasher configured for v2");
        var result = hasherV2.VerifyPassword(executionContext, password, hashResult.Hash);

        // Assert
        LogAssert("Verifying password is valid but needs rehash");
        result.IsValid.ShouldBeTrue();
        result.NeedsRehash.ShouldBeTrue();
    }

    [Fact]
    public void VerifyPassword_WithCurrentPepperVersion_ShouldNotNeedRehash()
    {
        // Arrange
        LogArrange("Hashing with pepper v1 and verifying with same v1");
        var hasher = CreateTestHasher(activePepperVersion: 1);
        var executionContext = CreateTestExecutionContext();
        string password = "MyPassword123!";
        var hashResult = hasher.HashPassword(executionContext, password);

        // Act
        LogAct("Verifying with same pepper version");
        var result = hasher.VerifyPassword(executionContext, password, hashResult.Hash);

        // Assert
        LogAssert("Verifying no rehash needed");
        result.IsValid.ShouldBeTrue();
        result.NeedsRehash.ShouldBeFalse();
    }

    #endregion

    #region NeedsRehash Tests

    [Fact]
    public void NeedsRehash_WithCurrentVersion_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating hasher and hashing with current version");
        var hasher = CreateTestHasher(activePepperVersion: 1);
        var executionContext = CreateTestExecutionContext();
        var hashResult = hasher.HashPassword(executionContext, "password");

        // Act
        LogAct("Checking if needs rehash");
        bool result = hasher.NeedsRehash(hashResult.Hash);

        // Assert
        LogAssert("Verifying rehash not needed");
        result.ShouldBeFalse();
    }

    [Fact]
    public void NeedsRehash_WithOldVersion_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Hashing with v1, then checking with hasher configured for v2");
        var pepper1 = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        var pepper2 = new byte[] { 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };

        var hasherV1 = new PasswordHasher(new PepperConfiguration(1, new Dictionary<int, byte[]>
        {
            { 1, pepper1 }
        }));
        var executionContext = CreateTestExecutionContext();
        var hashResult = hasherV1.HashPassword(executionContext, "password");

        var hasherV2 = new PasswordHasher(new PepperConfiguration(2, new Dictionary<int, byte[]>
        {
            { 1, pepper1 },
            { 2, pepper2 }
        }));

        // Act
        LogAct("Checking if v1 hash needs rehash in v2 context");
        bool result = hasherV2.NeedsRehash(hashResult.Hash);

        // Assert
        LogAssert("Verifying rehash is needed");
        result.ShouldBeTrue();
    }

    [Fact]
    public void NeedsRehash_WithNullHash_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating hasher");
        var hasher = CreateTestHasher();

        // Act
        LogAct("Checking null hash");
        bool result = hasher.NeedsRehash(null!);

        // Assert
        LogAssert("Verifying rehash needed for null hash");
        result.ShouldBeTrue();
    }

    [Fact]
    public void NeedsRehash_WithEmptyHash_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating hasher");
        var hasher = CreateTestHasher();

        // Act
        LogAct("Checking empty hash");
        bool result = hasher.NeedsRehash([]);

        // Assert
        LogAssert("Verifying rehash needed for empty hash");
        result.ShouldBeTrue();
    }

    [Fact]
    public void NeedsRehash_WithMinimumLengthHash_ShouldCheckPepperVersion()
    {
        // Arrange
        LogArrange("Creating hasher with pepper version 1 and a 1-byte hash");
        var hasher = CreateTestHasher(activePepperVersion: 1);
        byte[] singleByteHash = [(byte)1]; // pepper version byte only

        // Act
        LogAct("Checking single-byte hash with matching pepper version");
        bool result = hasher.NeedsRehash(singleByteHash);

        // Assert
        LogAssert("Verifying rehash not needed when pepper version matches (length >= PepperVersionLength)");
        result.ShouldBeFalse();
    }

    [Fact]
    public void NeedsRehash_WithMinimumLengthHash_DifferentVersion_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating hasher with pepper version 1 and a 1-byte hash with version 2");
        var hasher = CreateTestHasher(activePepperVersion: 1);
        byte[] singleByteHash = [(byte)2]; // different pepper version

        // Act
        LogAct("Checking single-byte hash with different pepper version");
        bool result = hasher.NeedsRehash(singleByteHash);

        // Assert
        LogAssert("Verifying rehash needed when pepper version differs");
        result.ShouldBeTrue();
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullConfig_ShouldThrow()
    {
        // Arrange
        LogArrange("Preparing null config");

        // Act & Assert
        LogAct("Creating PasswordHasher with null config");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(() => new PasswordHasher(null!));
    }

    #endregion

    #region Helper Methods

    private static PasswordHasher CreateTestHasher(int activePepperVersion = 1)
    {
        var peppers = new Dictionary<int, byte[]>
        {
            { 1, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 } },
            { 2, new byte[] { 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 } }
        };
        var config = new PepperConfiguration(activePepperVersion, peppers);
        return new PasswordHasher(config);
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

    #endregion
}
