using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.DPoPKeys;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.DPoPKeys;

public class JwkThumbprintTests : TestBase
{
    public JwkThumbprintTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void CreateNew_ShouldPreserveValue()
    {
        LogAct("Creating JwkThumbprint");
        var t = JwkThumbprint.CreateNew("thumb-123");
        LogAssert("Verifying value");
        t.Value.ShouldBe("thumb-123");
    }

    [Fact]
    public void CreateFromExistingInfo_ShouldPreserveValue()
    {
        LogAct("Creating from existing");
        var t = JwkThumbprint.CreateFromExistingInfo("existing-thumb");
        LogAssert("Verifying value");
        t.Value.ShouldBe("existing-thumb");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        LogAssert("Verifying equality");
        JwkThumbprint.CreateNew("same").Equals(JwkThumbprint.CreateNew("same")).ShouldBeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        LogAssert("Verifying inequality");
        JwkThumbprint.CreateNew("a").Equals(JwkThumbprint.CreateNew("b")).ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithBoxedSameType_ShouldReturnTrue()
    {
        LogArrange("Boxing");
        var a = JwkThumbprint.CreateNew("same");
        object b = JwkThumbprint.CreateNew("same");
        LogAssert("Verifying equality via object");
        a.Equals(b).ShouldBeTrue();
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        LogAssert("Verifying false for different type");
        JwkThumbprint.CreateNew("test").Equals("not a thumbprint").ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        LogAssert("Verifying false for null");
        JwkThumbprint.CreateNew("test").Equals(null).ShouldBeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldMatch()
    {
        LogAssert("Verifying hash codes match");
        JwkThumbprint.CreateNew("same").GetHashCode().ShouldBe(JwkThumbprint.CreateNew("same").GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentValue_ShouldDiffer()
    {
        LogAssert("Verifying hash codes differ");
        JwkThumbprint.CreateNew("a").GetHashCode().ShouldNotBe(JwkThumbprint.CreateNew("b").GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        LogAssert("Verifying ToString");
        JwkThumbprint.CreateNew("my-thumb").ToString().ShouldBe("my-thumb");
    }

    [Fact]
    public void EqualityOperator_ShouldWork()
    {
        LogAssert("Verifying == operator");
        (JwkThumbprint.CreateNew("same") == JwkThumbprint.CreateNew("same")).ShouldBeTrue();
    }

    [Fact]
    public void InequalityOperator_WithDifferent_ShouldReturnTrue()
    {
        LogAssert("Verifying != operator");
        (JwkThumbprint.CreateNew("a") != JwkThumbprint.CreateNew("b")).ShouldBeTrue();
    }

    [Fact]
    public void InequalityOperator_WithSame_ShouldReturnFalse()
    {
        LogAssert("Verifying != false for equal");
        (JwkThumbprint.CreateNew("same") != JwkThumbprint.CreateNew("same")).ShouldBeFalse();
    }
}
