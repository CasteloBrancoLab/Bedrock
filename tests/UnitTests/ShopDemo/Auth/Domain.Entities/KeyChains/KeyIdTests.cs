using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.KeyChains;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.KeyChains;

public class KeyIdTests : TestBase
{
    public KeyIdTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void CreateNew_ShouldPreserveValue()
    {
        LogAct("Creating KeyId");
        var keyId = KeyId.CreateNew("key-123");
        LogAssert("Verifying value");
        keyId.Value.ShouldBe("key-123");
    }

    [Fact]
    public void CreateFromExistingInfo_ShouldPreserveValue()
    {
        LogAct("Creating KeyId from existing");
        var keyId = KeyId.CreateFromExistingInfo("existing-key");
        LogAssert("Verifying value");
        keyId.Value.ShouldBe("existing-key");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        LogAssert("Verifying equality");
        KeyId.CreateNew("same").Equals(KeyId.CreateNew("same")).ShouldBeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        LogAssert("Verifying inequality");
        KeyId.CreateNew("a").Equals(KeyId.CreateNew("b")).ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithBoxedSameType_ShouldReturnTrue()
    {
        LogArrange("Boxing KeyId");
        var a = KeyId.CreateNew("same");
        object b = KeyId.CreateNew("same");
        LogAssert("Verifying equality via object");
        a.Equals(b).ShouldBeTrue();
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        LogAssert("Verifying false for different type");
        KeyId.CreateNew("test").Equals("not a keyid").ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        LogAssert("Verifying false for null");
        KeyId.CreateNew("test").Equals(null).ShouldBeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldMatch()
    {
        LogAssert("Verifying hash codes match");
        KeyId.CreateNew("same").GetHashCode().ShouldBe(KeyId.CreateNew("same").GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentValue_ShouldDiffer()
    {
        LogAssert("Verifying hash codes differ");
        KeyId.CreateNew("a").GetHashCode().ShouldNotBe(KeyId.CreateNew("b").GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        LogAssert("Verifying ToString");
        KeyId.CreateNew("my-key").ToString().ShouldBe("my-key");
    }

    [Fact]
    public void EqualityOperator_ShouldWork()
    {
        LogAssert("Verifying == operator");
        (KeyId.CreateNew("same") == KeyId.CreateNew("same")).ShouldBeTrue();
    }

    [Fact]
    public void InequalityOperator_WithDifferent_ShouldReturnTrue()
    {
        LogAssert("Verifying != operator");
        (KeyId.CreateNew("a") != KeyId.CreateNew("b")).ShouldBeTrue();
    }

    [Fact]
    public void InequalityOperator_WithSame_ShouldReturnFalse()
    {
        LogAssert("Verifying != false for equal");
        (KeyId.CreateNew("same") != KeyId.CreateNew("same")).ShouldBeFalse();
    }
}
