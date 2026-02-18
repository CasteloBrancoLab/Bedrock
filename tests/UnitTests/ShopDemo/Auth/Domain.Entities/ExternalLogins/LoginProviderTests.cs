using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.ExternalLogins;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.ExternalLogins;

public class LoginProviderTests : TestBase
{
    public LoginProviderTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void CreateNew_ShouldPreserveValue()
    {
        LogAct("Creating LoginProvider");
        var p = LoginProvider.CreateNew("custom");
        LogAssert("Verifying value");
        p.Value.ShouldBe("custom");
    }

    [Fact]
    public void CreateFromExistingInfo_ShouldPreserveValue()
    {
        LogAct("Creating from existing");
        var p = LoginProvider.CreateFromExistingInfo("existing");
        LogAssert("Verifying value");
        p.Value.ShouldBe("existing");
    }

    [Fact]
    public void Google_ShouldHaveCorrectValue()
    {
        LogAssert("Verifying Google");
        LoginProvider.Google.Value.ShouldBe("google");
    }

    [Fact]
    public void GitHub_ShouldHaveCorrectValue()
    {
        LogAssert("Verifying GitHub");
        LoginProvider.GitHub.Value.ShouldBe("github");
    }

    [Fact]
    public void Microsoft_ShouldHaveCorrectValue()
    {
        LogAssert("Verifying Microsoft");
        LoginProvider.Microsoft.Value.ShouldBe("microsoft");
    }

    [Fact]
    public void Apple_ShouldHaveCorrectValue()
    {
        LogAssert("Verifying Apple");
        LoginProvider.Apple.Value.ShouldBe("apple");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        LogAssert("Verifying equality");
        LoginProvider.CreateNew("google").Equals(LoginProvider.CreateNew("google")).ShouldBeTrue();
    }

    [Fact]
    public void Equals_CaseInsensitive_ShouldReturnTrue()
    {
        LogAssert("Verifying case-insensitive equality");
        LoginProvider.CreateNew("Google").Equals(LoginProvider.CreateNew("google")).ShouldBeTrue();
    }

    [Fact]
    public void Equals_WithUpperCase_ShouldReturnTrue()
    {
        LogAssert("Verifying uppercase equality");
        LoginProvider.CreateNew("GITHUB").Equals(LoginProvider.CreateNew("github")).ShouldBeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        LogAssert("Verifying inequality");
        LoginProvider.CreateNew("google").Equals(LoginProvider.CreateNew("github")).ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithBoxedSameType_ShouldReturnTrue()
    {
        LogArrange("Boxing");
        var a = LoginProvider.CreateNew("google");
        object b = LoginProvider.CreateNew("google");
        LogAssert("Verifying equality via object");
        a.Equals(b).ShouldBeTrue();
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        LogAssert("Verifying false for different type");
        LoginProvider.CreateNew("google").Equals("not a provider").ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        LogAssert("Verifying false for null");
        LoginProvider.CreateNew("google").Equals(null).ShouldBeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldMatch()
    {
        LogAssert("Verifying hash codes match");
        LoginProvider.CreateNew("google").GetHashCode().ShouldBe(LoginProvider.CreateNew("google").GetHashCode());
    }

    [Fact]
    public void GetHashCode_CaseInsensitive_ShouldMatch()
    {
        LogAssert("Verifying case-insensitive hash codes");
        LoginProvider.CreateNew("Google").GetHashCode().ShouldBe(LoginProvider.CreateNew("google").GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentValue_ShouldDiffer()
    {
        LogAssert("Verifying hash codes differ");
        LoginProvider.CreateNew("google").GetHashCode().ShouldNotBe(LoginProvider.CreateNew("github").GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        LogAssert("Verifying ToString");
        LoginProvider.CreateNew("custom").ToString().ShouldBe("custom");
    }

    [Fact]
    public void ToString_StaticField_ShouldReturnValue()
    {
        LogAssert("Verifying static ToString");
        LoginProvider.Google.ToString().ShouldBe("google");
    }

    [Fact]
    public void EqualityOperator_ShouldWork()
    {
        LogAssert("Verifying == operator");
        (LoginProvider.CreateNew("google") == LoginProvider.CreateNew("google")).ShouldBeTrue();
    }

    [Fact]
    public void EqualityOperator_CaseInsensitive_ShouldWork()
    {
        LogAssert("Verifying case-insensitive ==");
        (LoginProvider.CreateNew("Google") == LoginProvider.CreateNew("google")).ShouldBeTrue();
    }

    [Fact]
    public void InequalityOperator_WithDifferent_ShouldReturnTrue()
    {
        LogAssert("Verifying != operator");
        (LoginProvider.CreateNew("google") != LoginProvider.CreateNew("github")).ShouldBeTrue();
    }

    [Fact]
    public void InequalityOperator_WithSame_ShouldReturnFalse()
    {
        LogAssert("Verifying != false for equal");
        (LoginProvider.CreateNew("google") != LoginProvider.CreateNew("google")).ShouldBeFalse();
    }
}
