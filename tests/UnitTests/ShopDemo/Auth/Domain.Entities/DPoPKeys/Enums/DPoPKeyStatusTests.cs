using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.DPoPKeys.Enums;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.DPoPKeys.Enums;

public class DPoPKeyStatusTests : TestBase
{
    public DPoPKeyStatusTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact] public void Active_ShouldHaveValue1() { LogAct("Casting"); ((int)DPoPKeyStatus.Active).ShouldBe(1); }
    [Fact] public void Revoked_ShouldHaveValue2() { LogAct("Casting"); ((int)DPoPKeyStatus.Revoked).ShouldBe(2); }
    [Fact] public void ShouldHaveExactlyTwoValues() { LogAct("Getting values"); Enum.GetValues<DPoPKeyStatus>().Length.ShouldBe(2); }

    [Theory]
    [InlineData(DPoPKeyStatus.Active, "Active")]
    [InlineData(DPoPKeyStatus.Revoked, "Revoked")]
    public void ToString_ShouldReturnCorrectName(DPoPKeyStatus status, string expected)
    {
        LogAct("Converting to string");
        status.ToString().ShouldBe(expected);
    }
}
