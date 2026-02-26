using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.Sessions.Enums;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.Sessions.Enums;

public class SessionLimitStrategyTests : TestBase
{
    public SessionLimitStrategyTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact] public void RejectNew_ShouldHaveValue1() { LogAct("Casting"); ((int)SessionLimitStrategy.RejectNew).ShouldBe(1); }
    [Fact] public void RevokeOldest_ShouldHaveValue2() { LogAct("Casting"); ((int)SessionLimitStrategy.RevokeOldest).ShouldBe(2); }
    [Fact] public void ShouldHaveExactlyTwoValues() { LogAct("Getting values"); Enum.GetValues<SessionLimitStrategy>().Length.ShouldBe(2); }

    [Theory]
    [InlineData(SessionLimitStrategy.RejectNew, "RejectNew")]
    [InlineData(SessionLimitStrategy.RevokeOldest, "RevokeOldest")]
    public void ToString_ShouldReturnCorrectName(SessionLimitStrategy strategy, string expected)
    {
        LogAct("Converting to string");
        strategy.ToString().ShouldBe(expected);
    }
}
