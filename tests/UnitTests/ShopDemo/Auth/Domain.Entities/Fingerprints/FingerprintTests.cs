using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.Fingerprints;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.Fingerprints;

public class FingerprintTests : TestBase
{
    public FingerprintTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void CreateNew_ShouldGenerateNonEmptyValue()
    {
        LogAct("Creating new Fingerprint");
        var fingerprint = Fingerprint.CreateNew();

        LogAssert("Verifying value is not empty");
        fingerprint.Value.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void CreateNew_ShouldGenerateUniqueValues()
    {
        LogAct("Creating two fingerprints");
        var a = Fingerprint.CreateNew();
        var b = Fingerprint.CreateNew();

        LogAssert("Verifying values are unique");
        a.Value.ShouldNotBe(b.Value);
    }

    [Fact]
    public void CreateNew_ShouldGenerateGuidNFormat()
    {
        LogAct("Creating fingerprint");
        var fingerprint = Fingerprint.CreateNew();

        LogAssert("Verifying 32-char hex format (Guid N)");
        fingerprint.Value.Length.ShouldBe(32);
    }

    [Fact]
    public void CreateFromExistingInfo_ShouldPreserveValue()
    {
        LogArrange("Preparing value");
        string value = "abc123def456";

        LogAct("Creating from existing info");
        var fingerprint = Fingerprint.CreateFromExistingInfo(value);

        LogAssert("Verifying value preserved");
        fingerprint.Value.ShouldBe(value);
    }

    [Fact]
    public void ComputeHash_ShouldReturnNonEmptyHash()
    {
        LogArrange("Creating fingerprint");
        var fingerprint = Fingerprint.CreateNew();

        LogAct("Computing hash");
        var hash = fingerprint.ComputeHash();

        LogAssert("Verifying hash is not empty");
        hash.IsEmpty.ShouldBeFalse();
        hash.Length.ShouldBe(32); // SHA256 = 32 bytes
    }

    [Fact]
    public void ComputeHash_SameFingerprintShouldProduceSameHash()
    {
        LogArrange("Creating two fingerprints with same value");
        var a = Fingerprint.CreateFromExistingInfo("same-value");
        var b = Fingerprint.CreateFromExistingInfo("same-value");

        LogAct("Computing hashes");
        var hashA = a.ComputeHash();
        var hashB = b.ComputeHash();

        LogAssert("Verifying hashes are equal");
        hashA.Equals(hashB).ShouldBeTrue();
    }

    [Fact]
    public void ComputeHash_DifferentFingerprintsShouldProduceDifferentHashes()
    {
        LogArrange("Creating two different fingerprints");
        var a = Fingerprint.CreateFromExistingInfo("value-a");
        var b = Fingerprint.CreateFromExistingInfo("value-b");

        LogAct("Computing hashes");
        var hashA = a.ComputeHash();
        var hashB = b.ComputeHash();

        LogAssert("Verifying hashes differ");
        hashA.Equals(hashB).ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        LogArrange("Creating two fingerprints with same value");
        var a = Fingerprint.CreateFromExistingInfo("same");
        var b = Fingerprint.CreateFromExistingInfo("same");

        LogAssert("Verifying equality");
        a.Equals(b).ShouldBeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        LogArrange("Creating two fingerprints with different values");
        var a = Fingerprint.CreateFromExistingInfo("a");
        var b = Fingerprint.CreateFromExistingInfo("b");

        LogAssert("Verifying inequality");
        a.Equals(b).ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithBoxedSameType_ShouldReturnTrue()
    {
        LogArrange("Creating fingerprint and boxing");
        var a = Fingerprint.CreateFromExistingInfo("test");
        object b = Fingerprint.CreateFromExistingInfo("test");

        LogAssert("Verifying equality via object");
        a.Equals(b).ShouldBeTrue();
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        LogAct("Comparing with different type");
        Fingerprint.CreateFromExistingInfo("test").Equals("not a fingerprint").ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        LogAct("Comparing with null");
        Fingerprint.CreateFromExistingInfo("test").Equals(null).ShouldBeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldMatch()
    {
        LogArrange("Creating two equal fingerprints");
        var a = Fingerprint.CreateFromExistingInfo("same");
        var b = Fingerprint.CreateFromExistingInfo("same");

        LogAssert("Verifying hash codes match");
        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentValue_ShouldDiffer()
    {
        LogAssert("Verifying hash codes differ");
        var a = Fingerprint.CreateFromExistingInfo("a");
        var b = Fingerprint.CreateFromExistingInfo("b");
        a.GetHashCode().ShouldNotBe(b.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnRedacted()
    {
        LogAct("Calling ToString");
        Fingerprint.CreateFromExistingInfo("secret").ToString().ShouldBe("[REDACTED]");
    }

    [Fact]
    public void EqualityOperator_ShouldWork()
    {
        LogAct("Using == operator");
        var a = Fingerprint.CreateFromExistingInfo("same");
        var b = Fingerprint.CreateFromExistingInfo("same");
        (a == b).ShouldBeTrue();
    }

    [Fact]
    public void InequalityOperator_WithDifferent_ShouldReturnTrue()
    {
        LogAct("Using != operator");
        var a = Fingerprint.CreateFromExistingInfo("a");
        var b = Fingerprint.CreateFromExistingInfo("b");
        (a != b).ShouldBeTrue();
    }

    [Fact]
    public void InequalityOperator_WithSame_ShouldReturnFalse()
    {
        LogAct("Using != operator on equal");
        var a = Fingerprint.CreateFromExistingInfo("same");
        var b = Fingerprint.CreateFromExistingInfo("same");
        (a != b).ShouldBeFalse();
    }
}
