using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.SigningKeys;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.SigningKeys;

public class KidTests : TestBase
{
    public KidTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void CreateNew_ShouldPreserveValue()
    {
        LogAct("Creating Kid");
        var kid = Kid.CreateNew("kid-123");
        LogAssert("Verifying value");
        kid.Value.ShouldBe("kid-123");
    }

    [Fact]
    public void CreateFromExistingInfo_ShouldPreserveValue()
    {
        LogAct("Creating Kid from existing");
        var kid = Kid.CreateFromExistingInfo("existing-kid");
        LogAssert("Verifying value");
        kid.Value.ShouldBe("existing-kid");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        LogArrange("Creating two Kids with same value");
        var a = Kid.CreateNew("same");
        var b = Kid.CreateNew("same");
        LogAssert("Verifying equality");
        a.Equals(b).ShouldBeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        LogAssert("Verifying inequality");
        Kid.CreateNew("a").Equals(Kid.CreateNew("b")).ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithBoxedSameType_ShouldReturnTrue()
    {
        LogArrange("Boxing Kid");
        var a = Kid.CreateNew("same");
        object b = Kid.CreateNew("same");
        LogAssert("Verifying equality via object");
        a.Equals(b).ShouldBeTrue();
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        LogAssert("Verifying false for different type");
        Kid.CreateNew("test").Equals("not a kid").ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        LogAssert("Verifying false for null");
        Kid.CreateNew("test").Equals(null).ShouldBeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldMatch()
    {
        LogAssert("Verifying hash codes match");
        Kid.CreateNew("same").GetHashCode().ShouldBe(Kid.CreateNew("same").GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentValue_ShouldDiffer()
    {
        LogAssert("Verifying hash codes differ");
        Kid.CreateNew("a").GetHashCode().ShouldNotBe(Kid.CreateNew("b").GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        LogAssert("Verifying ToString returns value");
        Kid.CreateNew("my-kid").ToString().ShouldBe("my-kid");
    }

    [Fact]
    public void EqualityOperator_ShouldWork()
    {
        LogAssert("Verifying == operator");
        (Kid.CreateNew("same") == Kid.CreateNew("same")).ShouldBeTrue();
    }

    [Fact]
    public void InequalityOperator_WithDifferent_ShouldReturnTrue()
    {
        LogAssert("Verifying != operator");
        (Kid.CreateNew("a") != Kid.CreateNew("b")).ShouldBeTrue();
    }

    [Fact]
    public void InequalityOperator_WithSame_ShouldReturnFalse()
    {
        LogAssert("Verifying != returns false for equal");
        (Kid.CreateNew("same") != Kid.CreateNew("same")).ShouldBeFalse();
    }
}
