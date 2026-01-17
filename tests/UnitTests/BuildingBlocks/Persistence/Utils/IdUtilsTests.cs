using Bedrock.BuildingBlocks.Persistence.Utils;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Persistence.Utils;

public class IdUtilsTests : TestBase
{
    public IdUtilsTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region HexIdToGuid Tests

    [Fact]
    public void HexIdToGuid_WithValidHexId_ShouldReturnGuid()
    {
        // Arrange
        LogArrange("Creating a valid 24-character hex ID");
        var hexId = "507f1f77bcf86cd799439011";

        // Act
        LogAct("Converting hex ID to Guid");
        var guid = IdUtils.HexIdToGuid(hexId);

        // Assert
        LogAssert("Verifying Guid is not empty");
        guid.ShouldNotBe(Guid.Empty);
        LogInfo("Converted Guid: {0}", guid);
    }

    [Fact]
    public void HexIdToGuid_WithAllZeros_ShouldReturnGuidWithZeros()
    {
        // Arrange
        LogArrange("Creating hex ID with all zeros");
        var hexId = "000000000000000000000000";

        // Act
        LogAct("Converting zero hex ID to Guid");
        var guid = IdUtils.HexIdToGuid(hexId);

        // Assert
        LogAssert("Verifying Guid is empty (all zeros)");
        guid.ShouldBe(Guid.Empty);
        LogInfo("Zero hex ID converts to empty Guid");
    }

    [Fact]
    public void HexIdToGuid_WithUppercaseHex_ShouldReturnGuid()
    {
        // Arrange
        LogArrange("Creating hex ID with uppercase letters");
        var hexIdUpper = "ABCDEF012345ABCDEF012345";
        var hexIdLower = "abcdef012345abcdef012345";

        // Act
        LogAct("Converting uppercase and lowercase hex IDs");
        var guidUpper = IdUtils.HexIdToGuid(hexIdUpper);
        var guidLower = IdUtils.HexIdToGuid(hexIdLower);

        // Assert
        LogAssert("Verifying both produce same Guid");
        guidUpper.ShouldBe(guidLower);
        LogInfo("Uppercase and lowercase hex IDs produce same Guid");
    }

    [Fact]
    public void HexIdToGuid_WithMixedCaseHex_ShouldReturnGuid()
    {
        // Arrange
        LogArrange("Creating hex ID with mixed case");
        var hexId = "AbCdEf012345aBcDeF012345";

        // Act
        LogAct("Converting mixed case hex ID");
        var guid = IdUtils.HexIdToGuid(hexId);

        // Assert
        LogAssert("Verifying Guid is valid");
        guid.ShouldNotBe(Guid.Empty);
        LogInfo("Mixed case conversion successful");
    }

    [Fact]
    public void HexIdToGuid_WithNullHexId_ShouldThrowArgumentException()
    {
        // Arrange
        LogArrange("Preparing null hex ID");
        string? hexId = null;

        // Act & Assert
        LogAct("Attempting to convert null hex ID");
        var exception = Should.Throw<ArgumentException>(() => IdUtils.HexIdToGuid(hexId!));
        LogAssert("Verifying ArgumentException is thrown");
        exception.ShouldNotBeNull();
        LogInfo("Null hex ID correctly throws ArgumentException");
    }

    [Fact]
    public void HexIdToGuid_WithEmptyHexId_ShouldThrowArgumentException()
    {
        // Arrange
        LogArrange("Preparing empty hex ID");
        var hexId = string.Empty;

        // Act & Assert
        LogAct("Attempting to convert empty hex ID");
        var exception = Should.Throw<ArgumentException>(() => IdUtils.HexIdToGuid(hexId));
        LogAssert("Verifying ArgumentException is thrown");
        exception.ShouldNotBeNull();
        LogInfo("Empty hex ID correctly throws ArgumentException");
    }

    [Fact]
    public void HexIdToGuid_WithTooShortHexId_ShouldThrowArgumentException()
    {
        // Arrange
        LogArrange("Creating hex ID with 23 characters (too short)");
        var hexId = "507f1f77bcf86cd79943901"; // 23 chars

        // Act & Assert
        LogAct("Attempting to convert too short hex ID");
        var exception = Should.Throw<ArgumentException>(() => IdUtils.HexIdToGuid(hexId));
        LogAssert("Verifying ArgumentException is thrown with correct message");
        exception.Message.ShouldContain("24");
        LogInfo("Too short hex ID correctly throws ArgumentException");
    }

    [Fact]
    public void HexIdToGuid_WithTooLongHexId_ShouldThrowArgumentException()
    {
        // Arrange
        LogArrange("Creating hex ID with 25 characters (too long)");
        var hexId = "507f1f77bcf86cd7994390111"; // 25 chars

        // Act & Assert
        LogAct("Attempting to convert too long hex ID");
        var exception = Should.Throw<ArgumentException>(() => IdUtils.HexIdToGuid(hexId));
        LogAssert("Verifying ArgumentException is thrown with correct message");
        exception.Message.ShouldContain("24");
        LogInfo("Too long hex ID correctly throws ArgumentException");
    }

    [Fact]
    public void HexIdToGuid_WithInvalidCharacter_ShouldThrowFormatException()
    {
        // Arrange
        LogArrange("Creating hex ID with invalid character 'G'");
        var hexId = "507f1f77bcf86cd79943901G"; // 'G' is invalid

        // Act & Assert
        LogAct("Attempting to convert hex ID with invalid character");
        var exception = Should.Throw<FormatException>(() => IdUtils.HexIdToGuid(hexId));
        LogAssert("Verifying FormatException is thrown with correct message");
        exception.Message.ShouldContain("Hexadecimal");
        LogInfo("Invalid character correctly throws FormatException");
    }

    [Fact]
    public void HexIdToGuid_WithInvalidCharacterInMiddle_ShouldThrowFormatException()
    {
        // Arrange
        LogArrange("Creating hex ID with invalid character in the middle");
        var hexId = "507f1f77bcfZ6cd799439011"; // 'Z' is invalid

        // Act & Assert
        LogAct("Attempting to convert hex ID with invalid character in middle");
        var exception = Should.Throw<FormatException>(() => IdUtils.HexIdToGuid(hexId));
        LogAssert("Verifying FormatException is thrown");
        exception.ShouldNotBeNull();
        LogInfo("Invalid character in middle correctly throws FormatException");
    }

    [Fact]
    public void HexIdToGuid_WithSpecialCharacters_ShouldThrowFormatException()
    {
        // Arrange
        LogArrange("Creating hex ID with special characters");
        var hexId = "507f1f77bcf!6cd799439011"; // '!' is invalid

        // Act & Assert
        LogAct("Attempting to convert hex ID with special character");
        var exception = Should.Throw<FormatException>(() => IdUtils.HexIdToGuid(hexId));
        LogAssert("Verifying FormatException is thrown");
        exception.ShouldNotBeNull();
        LogInfo("Special character correctly throws FormatException");
    }

    [Theory]
    [InlineData("0")]
    [InlineData("00")]
    [InlineData("0000000000000000000000000")] // 25 zeros
    [InlineData("")]
    public void HexIdToGuid_WithVariousInvalidLengths_ShouldThrowArgumentException(string hexId)
    {
        // Arrange
        LogArrange($"Testing hex ID with length {hexId.Length}");

        // Act & Assert
        LogAct("Attempting to convert invalid length hex ID");
        Should.Throw<ArgumentException>(() => IdUtils.HexIdToGuid(hexId));
        LogAssert("ArgumentException thrown for invalid length");
    }

    [Fact]
    public void HexIdToGuid_LastFourBytesAreZero()
    {
        // Arrange
        LogArrange("Creating valid hex ID to verify last 4 bytes are zero");
        var hexId = "507f1f77bcf86cd799439011";

        // Act
        LogAct("Converting and extracting bytes");
        var guid = IdUtils.HexIdToGuid(hexId);
        var bytes = guid.ToByteArray();

        // Assert
        LogAssert("Verifying last 4 bytes are zero");
        bytes[12].ShouldBe((byte)0);
        bytes[13].ShouldBe((byte)0);
        bytes[14].ShouldBe((byte)0);
        bytes[15].ShouldBe((byte)0);
        LogInfo("Last 4 bytes are correctly set to zero");
    }

    [Fact]
    public void HexIdToGuid_PreservesFirst12BytesCorrectly()
    {
        // Arrange
        LogArrange("Creating hex ID with known bytes");
        var hexId = "0102030405060708090a0b0c";

        // Act
        LogAct("Converting and extracting bytes");
        var guid = IdUtils.HexIdToGuid(hexId);
        var bytes = guid.ToByteArray();

        // Assert
        LogAssert("Verifying first 12 bytes match input");
        bytes[0].ShouldBe((byte)0x01);
        bytes[1].ShouldBe((byte)0x02);
        bytes[2].ShouldBe((byte)0x03);
        bytes[3].ShouldBe((byte)0x04);
        bytes[4].ShouldBe((byte)0x05);
        bytes[5].ShouldBe((byte)0x06);
        bytes[6].ShouldBe((byte)0x07);
        bytes[7].ShouldBe((byte)0x08);
        bytes[8].ShouldBe((byte)0x09);
        bytes[9].ShouldBe((byte)0x0a);
        bytes[10].ShouldBe((byte)0x0b);
        bytes[11].ShouldBe((byte)0x0c);
        LogInfo("First 12 bytes correctly preserved");
    }

    #endregion

    #region GuidToHexId Tests

    [Fact]
    public void GuidToHexId_WithValidGuid_ShouldReturnHexId()
    {
        // Arrange
        LogArrange("Creating a Guid with last 4 bytes as zero");
        var bytes = new byte[] { 0x50, 0x7f, 0x1f, 0x77, 0xbc, 0xf8, 0x6c, 0xd7, 0x99, 0x43, 0x90, 0x11, 0x00, 0x00, 0x00, 0x00 };
        var guid = new Guid(bytes);

        // Act
        LogAct("Converting Guid to hex ID");
        var hexId = IdUtils.GuidToHexId(guid);

        // Assert
        LogAssert("Verifying hex ID length is 24");
        hexId.Length.ShouldBe(24);
        hexId.ShouldBe("507f1f77bcf86cd799439011");
        LogInfo("Hex ID: {0}", hexId);
    }

    [Fact]
    public void GuidToHexId_WithEmptyGuid_ShouldReturnAllZeros()
    {
        // Arrange
        LogArrange("Using Guid.Empty");
        var guid = Guid.Empty;

        // Act
        LogAct("Converting empty Guid to hex ID");
        var hexId = IdUtils.GuidToHexId(guid);

        // Assert
        LogAssert("Verifying hex ID is all zeros");
        hexId.ShouldBe("000000000000000000000000");
        LogInfo("Empty Guid correctly converts to all zeros");
    }

    [Fact]
    public void GuidToHexId_WithNonZeroLastBytes_ShouldThrowArgumentException()
    {
        // Arrange
        LogArrange("Creating a Guid with non-zero last bytes");
        var bytes = new byte[] { 0x50, 0x7f, 0x1f, 0x77, 0xbc, 0xf8, 0x6c, 0xd7, 0x99, 0x43, 0x90, 0x11, 0x01, 0x00, 0x00, 0x00 };
        var guid = new Guid(bytes);

        // Act & Assert
        LogAct("Attempting to convert Guid with non-zero byte at position 12");
        var exception = Should.Throw<ArgumentException>(() => IdUtils.GuidToHexId(guid));
        LogAssert("Verifying ArgumentException is thrown with correct message");
        exception.Message.ShouldContain("Guid");
        exception.Message.ShouldContain("hexadecimal");
        LogInfo("Non-zero byte at position 12 correctly throws ArgumentException");
    }

    [Fact]
    public void GuidToHexId_WithNonZeroByteAt13_ShouldThrowArgumentException()
    {
        // Arrange
        LogArrange("Creating a Guid with non-zero byte at position 13");
        var bytes = new byte[] { 0x50, 0x7f, 0x1f, 0x77, 0xbc, 0xf8, 0x6c, 0xd7, 0x99, 0x43, 0x90, 0x11, 0x00, 0x01, 0x00, 0x00 };
        var guid = new Guid(bytes);

        // Act & Assert
        LogAct("Attempting to convert Guid with non-zero byte at position 13");
        var exception = Should.Throw<ArgumentException>(() => IdUtils.GuidToHexId(guid));
        LogAssert("Verifying ArgumentException is thrown");
        exception.ShouldNotBeNull();
        LogInfo("Non-zero byte at position 13 correctly throws ArgumentException");
    }

    [Fact]
    public void GuidToHexId_WithNonZeroByteAt14_ShouldThrowArgumentException()
    {
        // Arrange
        LogArrange("Creating a Guid with non-zero byte at position 14");
        var bytes = new byte[] { 0x50, 0x7f, 0x1f, 0x77, 0xbc, 0xf8, 0x6c, 0xd7, 0x99, 0x43, 0x90, 0x11, 0x00, 0x00, 0x01, 0x00 };
        var guid = new Guid(bytes);

        // Act & Assert
        LogAct("Attempting to convert Guid with non-zero byte at position 14");
        var exception = Should.Throw<ArgumentException>(() => IdUtils.GuidToHexId(guid));
        LogAssert("Verifying ArgumentException is thrown");
        exception.ShouldNotBeNull();
        LogInfo("Non-zero byte at position 14 correctly throws ArgumentException");
    }

    [Fact]
    public void GuidToHexId_WithNonZeroByteAt15_ShouldThrowArgumentException()
    {
        // Arrange
        LogArrange("Creating a Guid with non-zero byte at position 15");
        var bytes = new byte[] { 0x50, 0x7f, 0x1f, 0x77, 0xbc, 0xf8, 0x6c, 0xd7, 0x99, 0x43, 0x90, 0x11, 0x00, 0x00, 0x00, 0x01 };
        var guid = new Guid(bytes);

        // Act & Assert
        LogAct("Attempting to convert Guid with non-zero byte at position 15");
        var exception = Should.Throw<ArgumentException>(() => IdUtils.GuidToHexId(guid));
        LogAssert("Verifying ArgumentException is thrown");
        exception.ShouldNotBeNull();
        LogInfo("Non-zero byte at position 15 correctly throws ArgumentException");
    }

    [Fact]
    public void GuidToHexId_ShouldReturnLowercaseHex()
    {
        // Arrange
        LogArrange("Creating a Guid with letters A-F in hex");
        var bytes = new byte[] { 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x00, 0x00, 0x00, 0x00 };
        var guid = new Guid(bytes);

        // Act
        LogAct("Converting Guid to hex ID");
        var hexId = IdUtils.GuidToHexId(guid);

        // Assert
        LogAssert("Verifying hex ID is lowercase");
        hexId.ShouldBe(hexId.ToLowerInvariant());
        hexId.ShouldBe("abcdef012345abcdef012345");
        LogInfo("Hex ID is correctly lowercase: {0}", hexId);
    }

    #endregion

    #region Round-Trip Tests

    [Fact]
    public void RoundTrip_HexToGuidToHex_ShouldPreserveValue()
    {
        // Arrange
        LogArrange("Creating original hex ID");
        var originalHexId = "507f1f77bcf86cd799439011";

        // Act
        LogAct("Converting hex to Guid and back to hex");
        var guid = IdUtils.HexIdToGuid(originalHexId);
        var resultHexId = IdUtils.GuidToHexId(guid);

        // Assert
        LogAssert("Verifying round-trip preserves value");
        resultHexId.ShouldBe(originalHexId);
        LogInfo("Round-trip successful");
    }

    [Fact]
    public void RoundTrip_GuidToHexToGuid_ShouldPreserveValue()
    {
        // Arrange
        LogArrange("Creating original Guid with zero last bytes");
        var bytes = new byte[] { 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67, 0x89, 0x01, 0x23, 0x45, 0x67, 0x00, 0x00, 0x00, 0x00 };
        var originalGuid = new Guid(bytes);

        // Act
        LogAct("Converting Guid to hex and back to Guid");
        var hexId = IdUtils.GuidToHexId(originalGuid);
        var resultGuid = IdUtils.HexIdToGuid(hexId);

        // Assert
        LogAssert("Verifying round-trip preserves value");
        resultGuid.ShouldBe(originalGuid);
        LogInfo("Round-trip successful");
    }

    [Fact]
    public void RoundTrip_MultipleValues_ShouldAllPreserve()
    {
        // Arrange
        LogArrange("Creating multiple hex IDs");
        var hexIds = new[]
        {
            "000000000000000000000000",
            "ffffffffffffffffffffffff",
            "123456789abcdef012345678",
            "abcdef0123456789abcdef01",
            "507f1f77bcf86cd799439011"
        };

        // Act & Assert
        LogAct("Testing round-trip for multiple values");
        foreach (var hexId in hexIds)
        {
            var guid = IdUtils.HexIdToGuid(hexId);
            var result = IdUtils.GuidToHexId(guid);
            result.ShouldBe(hexId, $"Round-trip failed for {hexId}");
        }
        LogAssert("All round-trips successful");
        LogInfo("Tested {0} hex IDs successfully", hexIds.Length);
    }

    #endregion

    #region Edge Cases for GetHexVal

    [Fact]
    public void HexIdToGuid_WithAllDigits_ShouldConvertCorrectly()
    {
        // Arrange
        LogArrange("Creating hex ID with only digits 0-9");
        var hexId = "012345678901234567890123";

        // Act
        LogAct("Converting digits-only hex ID");
        var guid = IdUtils.HexIdToGuid(hexId);

        // Assert
        LogAssert("Verifying conversion succeeds");
        guid.ShouldNotBe(Guid.Empty);
        var roundTrip = IdUtils.GuidToHexId(guid);
        roundTrip.ShouldBe(hexId);
        LogInfo("Digits-only hex ID converts correctly");
    }

    [Fact]
    public void HexIdToGuid_WithAllLowercaseLetters_ShouldConvertCorrectly()
    {
        // Arrange
        LogArrange("Creating hex ID with only lowercase a-f");
        var hexId = "abcdefabcdefabcdefabcdef";

        // Act
        LogAct("Converting lowercase letters hex ID");
        var guid = IdUtils.HexIdToGuid(hexId);

        // Assert
        LogAssert("Verifying conversion succeeds");
        guid.ShouldNotBe(Guid.Empty);
        var roundTrip = IdUtils.GuidToHexId(guid);
        roundTrip.ShouldBe(hexId);
        LogInfo("Lowercase letters hex ID converts correctly");
    }

    [Fact]
    public void HexIdToGuid_WithAllUppercaseLetters_ShouldConvertCorrectly()
    {
        // Arrange
        LogArrange("Creating hex ID with only uppercase A-F");
        var hexId = "ABCDEFABCDEFABCDEFABCDEF";

        // Act
        LogAct("Converting uppercase letters hex ID");
        var guid = IdUtils.HexIdToGuid(hexId);

        // Assert
        LogAssert("Verifying conversion succeeds");
        guid.ShouldNotBe(Guid.Empty);
        var roundTrip = IdUtils.GuidToHexId(guid);
        roundTrip.ShouldBe(hexId.ToLowerInvariant());
        LogInfo("Uppercase letters hex ID converts correctly");
    }

    [Fact]
    public void HexIdToGuid_WithInvalidHiNibble_ShouldThrowFormatException()
    {
        // Arrange - Invalid character in hi nibble position (even index)
        LogArrange("Creating hex ID with invalid hi nibble");
        var hexId = "G07f1f77bcf86cd799439011"; // 'G' at position 0

        // Act & Assert
        LogAct("Attempting to convert with invalid hi nibble");
        var exception = Should.Throw<FormatException>(() => IdUtils.HexIdToGuid(hexId));
        LogAssert("Verifying FormatException is thrown");
        exception.ShouldNotBeNull();
        LogInfo("Invalid hi nibble correctly throws FormatException");
    }

    [Fact]
    public void HexIdToGuid_WithInvalidLoNibble_ShouldThrowFormatException()
    {
        // Arrange - Invalid character in lo nibble position (odd index)
        LogArrange("Creating hex ID with invalid lo nibble");
        var hexId = "5G7f1f77bcf86cd799439011"; // 'G' at position 1

        // Act & Assert
        LogAct("Attempting to convert with invalid lo nibble");
        var exception = Should.Throw<FormatException>(() => IdUtils.HexIdToGuid(hexId));
        LogAssert("Verifying FormatException is thrown");
        exception.ShouldNotBeNull();
        LogInfo("Invalid lo nibble correctly throws FormatException");
    }

    [Fact]
    public void HexIdToGuid_CharacterBoundaries_ShouldHandleCorrectly()
    {
        // Arrange - Test boundary characters: '/', ':', '@', '`', 'g'
        LogArrange("Testing character boundaries");

        // Characters just before valid ranges
        var invalidChars = new[] { '/', ':', '@', '`', 'g', 'G', ' ', '\t', '\n' };

        foreach (var invalidChar in invalidChars)
        {
            var hexId = new string(invalidChar, 1) + "07f1f77bcf86cd799439011"; // 24 chars total
            LogAct($"Testing invalid character '{invalidChar}'");
            Should.Throw<FormatException>(() => IdUtils.HexIdToGuid(hexId));
        }

        LogAssert("All invalid boundary characters throw FormatException");
    }

    [Fact]
    public void HexIdToGuid_AllValidHexCharacters_ShouldConvertCorrectly()
    {
        // Arrange
        LogArrange("Testing all valid hex characters");
        var allValidChars = "0123456789abcdefABCDEF01";

        // Act
        LogAct("Converting hex ID with all valid characters");
        var guid = IdUtils.HexIdToGuid(allValidChars);

        // Assert
        LogAssert("Verifying conversion succeeds");
        guid.ShouldNotBe(Guid.Empty);
        LogInfo("All valid hex characters convert correctly");
    }

    #endregion

    #region Specific Byte Position Tests

    [Theory]
    [InlineData(0, 0xFF)]
    [InlineData(1, 0xFF)]
    [InlineData(2, 0xFF)]
    [InlineData(3, 0xFF)]
    [InlineData(4, 0xFF)]
    [InlineData(5, 0xFF)]
    [InlineData(6, 0xFF)]
    [InlineData(7, 0xFF)]
    [InlineData(8, 0xFF)]
    [InlineData(9, 0xFF)]
    [InlineData(10, 0xFF)]
    [InlineData(11, 0xFF)]
    public void HexIdToGuid_EachBytePosition_ShouldMapCorrectly(int position, byte value)
    {
        // Arrange
        LogArrange($"Creating hex ID with byte 0x{value:X2} at position {position}");
        var hexChars = new char[24];
        for (int i = 0; i < 24; i++)
            hexChars[i] = '0';

        hexChars[position * 2] = (char)('0' + (value >> 4));
        if ((value >> 4) > 9)
            hexChars[position * 2] = (char)('a' + (value >> 4) - 10);

        hexChars[position * 2 + 1] = (char)('0' + (value & 0x0F));
        if ((value & 0x0F) > 9)
            hexChars[position * 2 + 1] = (char)('a' + (value & 0x0F) - 10);

        var hexId = new string(hexChars);

        // Act
        LogAct("Converting and extracting specific byte");
        var guid = IdUtils.HexIdToGuid(hexId);
        var bytes = guid.ToByteArray();

        // Assert
        LogAssert($"Verifying byte at position {position}");
        bytes[position].ShouldBe(value);
        LogInfo($"Byte at position {position} correctly mapped");
    }

    #endregion
}
