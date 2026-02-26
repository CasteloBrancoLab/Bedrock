using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.SigningKeys.Enums;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.SigningKeys.Enums;

public class SigningKeyStatusTests : TestBase
{
    public SigningKeyStatusTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact] public void Active_ShouldHaveValue1() { LogAct("Casting"); ((int)SigningKeyStatus.Active).ShouldBe(1); }
    [Fact] public void Rotated_ShouldHaveValue2() { LogAct("Casting"); ((int)SigningKeyStatus.Rotated).ShouldBe(2); }
    [Fact] public void Revoked_ShouldHaveValue3() { LogAct("Casting"); ((int)SigningKeyStatus.Revoked).ShouldBe(3); }
    [Fact] public void ShouldHaveExactlyThreeValues() { LogAct("Getting values"); Enum.GetValues<SigningKeyStatus>().Length.ShouldBe(3); }

    [Theory]
    [InlineData(SigningKeyStatus.Active, "Active")]
    [InlineData(SigningKeyStatus.Rotated, "Rotated")]
    [InlineData(SigningKeyStatus.Revoked, "Revoked")]
    public void ToString_ShouldReturnCorrectName(SigningKeyStatus status, string expected)
    {
        LogAct("Converting to string");
        status.ToString().ShouldBe(expected);
    }
}
