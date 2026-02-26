using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.Fingerprints;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.Fingerprints;

public class FingerprintHashTests : TestBase
{
    public FingerprintHashTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    private static byte[] CreateTestBytes(int length = 32) => Enumerable.Range(0, length).Select(i => (byte)i).ToArray();

    [Fact]
    public void CreateNew_ShouldCreateDefensiveCopy()
    {
        LogArrange("Preparing byte array");
        var original = CreateTestBytes();
        var originalCopy = original.ToArray();

        LogAct("Creating FingerprintHash");
        var hash = FingerprintHash.CreateNew(original);

        LogArrange("Mutating original array");
        original[0] = 255;

        LogAssert("Verifying defensive copy was made");
        hash.Value.Span[0].ShouldBe(originalCopy[0]);
    }

    [Fact]
    public void CreateFromExistingInfo_ShouldCreateDefensiveCopy()
    {
        LogArrange("Preparing byte array");
        var original = CreateTestBytes();
        var originalCopy = original.ToArray();

        LogAct("Creating from existing info");
        var hash = FingerprintHash.CreateFromExistingInfo(original);

        LogArrange("Mutating original");
        original[0] = 255;

        LogAssert("Verifying defensive copy");
        hash.Value.Span[0].ShouldBe(originalCopy[0]);
    }

    [Fact]
    public void IsEmpty_WithNonEmptyData_ShouldReturnFalse()
    {
        LogAct("Creating non-empty hash");
        var hash = FingerprintHash.CreateNew(CreateTestBytes());

        LogAssert("Verifying not empty");
        hash.IsEmpty.ShouldBeFalse();
    }

    [Fact]
    public void IsEmpty_WithEmptyData_ShouldReturnTrue()
    {
        LogAct("Creating empty hash");
        var hash = FingerprintHash.CreateNew(Array.Empty<byte>());

        LogAssert("Verifying empty");
        hash.IsEmpty.ShouldBeTrue();
    }

    [Fact]
    public void Length_ShouldReturnCorrectLength()
    {
        LogAct("Creating hash with known length");
        var hash = FingerprintHash.CreateNew(CreateTestBytes(16));

        LogAssert("Verifying length");
        hash.Length.ShouldBe(16);
    }

    [Fact]
    public void ToBase64Url_ShouldReturnUrlSafeString()
    {
        LogArrange("Creating hash");
        var hash = FingerprintHash.CreateNew(CreateTestBytes());

        LogAct("Converting to Base64Url");
        string result = hash.ToBase64Url();

        LogAssert("Verifying URL-safe (no +, /, =)");
        result.ShouldNotContain("+");
        result.ShouldNotContain("/");
        result.ShouldNotContain("=");
        result.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void Equals_WithSameBytes_ShouldReturnTrue()
    {
        LogArrange("Creating two hashes with same bytes");
        var bytes = CreateTestBytes();
        var a = FingerprintHash.CreateNew(bytes.ToArray());
        var b = FingerprintHash.CreateNew(bytes.ToArray());

        LogAssert("Verifying equality");
        a.Equals(b).ShouldBeTrue();
    }

    [Fact]
    public void Equals_WithDifferentBytes_ShouldReturnFalse()
    {
        LogArrange("Creating two hashes with different bytes");
        var a = FingerprintHash.CreateNew(new byte[] { 1, 2, 3 });
        var b = FingerprintHash.CreateNew(new byte[] { 4, 5, 6 });

        LogAssert("Verifying inequality");
        a.Equals(b).ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithBoxedSameType_ShouldReturnTrue()
    {
        LogArrange("Creating and boxing hash");
        var bytes = CreateTestBytes();
        var a = FingerprintHash.CreateNew(bytes.ToArray());
        object b = FingerprintHash.CreateNew(bytes.ToArray());

        LogAssert("Verifying equality via object");
        a.Equals(b).ShouldBeTrue();
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        LogAct("Comparing with different type");
        FingerprintHash.CreateNew(CreateTestBytes()).Equals("not a hash").ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        LogAct("Comparing with null");
        FingerprintHash.CreateNew(CreateTestBytes()).Equals(null).ShouldBeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameBytes_ShouldMatch()
    {
        LogArrange("Creating two equal hashes");
        var bytes = CreateTestBytes();
        var a = FingerprintHash.CreateNew(bytes.ToArray());
        var b = FingerprintHash.CreateNew(bytes.ToArray());

        LogAssert("Verifying hash codes match");
        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentBytes_ShouldDiffer()
    {
        LogAssert("Verifying hash codes differ");
        var a = FingerprintHash.CreateNew(new byte[] { 1, 2, 3 });
        var b = FingerprintHash.CreateNew(new byte[] { 4, 5, 6 });
        a.GetHashCode().ShouldNotBe(b.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnRedacted()
    {
        LogAct("Calling ToString");
        FingerprintHash.CreateNew(CreateTestBytes()).ToString().ShouldBe("[REDACTED]");
    }

    [Fact]
    public void EqualityOperator_ShouldWork()
    {
        LogAct("Using == operator");
        var bytes = CreateTestBytes();
        var a = FingerprintHash.CreateNew(bytes.ToArray());
        var b = FingerprintHash.CreateNew(bytes.ToArray());
        (a == b).ShouldBeTrue();
    }

    [Fact]
    public void InequalityOperator_WithDifferent_ShouldReturnTrue()
    {
        LogAct("Using != operator");
        var a = FingerprintHash.CreateNew(new byte[] { 1 });
        var b = FingerprintHash.CreateNew(new byte[] { 2 });
        (a != b).ShouldBeTrue();
    }

    [Fact]
    public void InequalityOperator_WithSame_ShouldReturnFalse()
    {
        LogAct("Using != on equal values");
        var bytes = CreateTestBytes();
        var a = FingerprintHash.CreateNew(bytes.ToArray());
        var b = FingerprintHash.CreateNew(bytes.ToArray());
        (a != b).ShouldBeFalse();
    }
}
