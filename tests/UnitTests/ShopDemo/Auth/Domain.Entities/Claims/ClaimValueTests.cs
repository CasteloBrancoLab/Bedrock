using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.Claims;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.Claims;

public class ClaimValueTests : TestBase
{
    public ClaimValueTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void CreateNew_ShouldPreserveValue()
    {
        LogArrange("Preparing short value");
        short value = 1;

        LogAct("Creating ClaimValue");
        var claimValue = ClaimValue.CreateNew(value);

        LogAssert("Verifying value preserved");
        claimValue.Value.ShouldBe(value);
    }

    [Fact]
    public void CreateFromExistingInfo_ShouldPreserveValue()
    {
        LogArrange("Preparing existing short value");
        short value = -1;

        LogAct("Creating ClaimValue from existing");
        var claimValue = ClaimValue.CreateFromExistingInfo(value);

        LogAssert("Verifying value preserved");
        claimValue.Value.ShouldBe(value);
    }

    [Fact]
    public void Granted_ShouldHaveValue1()
    {
        LogAct("Accessing Granted static field");
        var granted = ClaimValue.Granted;

        LogAssert("Verifying Granted value is 1");
        granted.Value.ShouldBe((short)1);
    }

    [Fact]
    public void Denied_ShouldHaveValueMinus1()
    {
        LogAct("Accessing Denied static field");
        var denied = ClaimValue.Denied;

        LogAssert("Verifying Denied value is -1");
        denied.Value.ShouldBe((short)-1);
    }

    [Fact]
    public void Inherited_ShouldHaveValue0()
    {
        LogAct("Accessing Inherited static field");
        var inherited = ClaimValue.Inherited;

        LogAssert("Verifying Inherited value is 0");
        inherited.Value.ShouldBe((short)0);
    }

    [Fact]
    public void IsGranted_WithGrantedValue_ShouldReturnTrue()
    {
        LogAct("Checking IsGranted on Granted");
        ClaimValue.Granted.IsGranted.ShouldBeTrue();
    }

    [Fact]
    public void IsGranted_WithDeniedValue_ShouldReturnFalse()
    {
        LogAct("Checking IsGranted on Denied");
        ClaimValue.Denied.IsGranted.ShouldBeFalse();
    }

    [Fact]
    public void IsDenied_WithDeniedValue_ShouldReturnTrue()
    {
        LogAct("Checking IsDenied on Denied");
        ClaimValue.Denied.IsDenied.ShouldBeTrue();
    }

    [Fact]
    public void IsDenied_WithGrantedValue_ShouldReturnFalse()
    {
        LogAct("Checking IsDenied on Granted");
        ClaimValue.Granted.IsDenied.ShouldBeFalse();
    }

    [Fact]
    public void IsInherited_WithInheritedValue_ShouldReturnTrue()
    {
        LogAct("Checking IsInherited on Inherited");
        ClaimValue.Inherited.IsInherited.ShouldBeTrue();
    }

    [Fact]
    public void IsInherited_WithGrantedValue_ShouldReturnFalse()
    {
        LogAct("Checking IsInherited on Granted");
        ClaimValue.Granted.IsInherited.ShouldBeFalse();
    }

    [Theory]
    [InlineData((short)1, true)]
    [InlineData((short)-1, true)]
    [InlineData((short)0, true)]
    [InlineData((short)2, false)]
    [InlineData((short)-2, false)]
    [InlineData((short)99, false)]
    public void IsValidValue_ShouldReturnExpected(short value, bool expected)
    {
        LogAct($"Checking IsValidValue for {value}");
        ClaimValue.IsValidValue(value).ShouldBe(expected);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        LogArrange("Creating two ClaimValues with same value");
        var a = ClaimValue.CreateNew(1);
        var b = ClaimValue.CreateNew(1);

        LogAct("Comparing");
        a.Equals(b).ShouldBeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        LogArrange("Creating two ClaimValues with different values");
        var a = ClaimValue.Granted;
        var b = ClaimValue.Denied;

        LogAct("Comparing");
        a.Equals(b).ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithBoxedSameType_ShouldReturnTrue()
    {
        LogArrange("Creating ClaimValue and boxing");
        var a = ClaimValue.Granted;
        object b = ClaimValue.Granted;

        LogAct("Comparing via object");
        a.Equals(b).ShouldBeTrue();
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        LogArrange("Creating ClaimValue and different type");
        var a = ClaimValue.Granted;
        object b = "not a ClaimValue";

        LogAct("Comparing with different type");
        a.Equals(b).ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        LogAct("Comparing with null");
        ClaimValue.Granted.Equals(null).ShouldBeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldMatch()
    {
        LogArrange("Creating two equal ClaimValues");
        var a = ClaimValue.CreateNew(1);
        var b = ClaimValue.CreateNew(1);

        LogAssert("Verifying hash codes match");
        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentValue_ShouldDiffer()
    {
        LogAssert("Verifying hash codes differ");
        ClaimValue.Granted.GetHashCode().ShouldNotBe(ClaimValue.Denied.GetHashCode());
    }

    [Theory]
    [InlineData((short)1, "Granted")]
    [InlineData((short)-1, "Denied")]
    [InlineData((short)0, "Inherited")]
    [InlineData((short)42, "42")]
    public void ToString_ShouldReturnExpected(short value, string expected)
    {
        LogAct($"Calling ToString for value {value}");
        ClaimValue.CreateNew(value).ToString().ShouldBe(expected);
    }

    [Fact]
    public void EqualityOperator_WithSameValue_ShouldReturnTrue()
    {
        LogAct("Using == operator");
        (ClaimValue.Granted == ClaimValue.CreateNew(1)).ShouldBeTrue();
    }

    [Fact]
    public void InequalityOperator_WithDifferentValue_ShouldReturnTrue()
    {
        LogAct("Using != operator");
        (ClaimValue.Granted != ClaimValue.Denied).ShouldBeTrue();
    }

    [Fact]
    public void InequalityOperator_WithSameValue_ShouldReturnFalse()
    {
        LogAct("Using != operator on equal values");
        (ClaimValue.Granted != ClaimValue.CreateNew(1)).ShouldBeFalse();
    }
}
