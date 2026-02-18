using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.KeyChains.Enums;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.KeyChains.Enums;

public class KeyChainStatusTests : TestBase
{
    public KeyChainStatusTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact] public void Active_ShouldHaveValue1() { LogAct("Casting"); ((int)KeyChainStatus.Active).ShouldBe(1); }
    [Fact] public void DecryptOnly_ShouldHaveValue2() { LogAct("Casting"); ((int)KeyChainStatus.DecryptOnly).ShouldBe(2); }
    [Fact] public void ShouldHaveExactlyTwoValues() { LogAct("Getting values"); Enum.GetValues<KeyChainStatus>().Length.ShouldBe(2); }

    [Theory]
    [InlineData(KeyChainStatus.Active, "Active")]
    [InlineData(KeyChainStatus.DecryptOnly, "DecryptOnly")]
    public void ToString_ShouldReturnCorrectName(KeyChainStatus status, string expected)
    {
        LogAct("Converting to string");
        status.ToString().ShouldBe(expected);
    }
}
