using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.ImpersonationSessions.Enums;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.ImpersonationSessions.Enums;

public class ImpersonationSessionStatusTests : TestBase
{
    public ImpersonationSessionStatusTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact] public void Active_ShouldHaveValue1() { LogAct("Casting"); ((int)ImpersonationSessionStatus.Active).ShouldBe(1); }
    [Fact] public void Ended_ShouldHaveValue2() { LogAct("Casting"); ((int)ImpersonationSessionStatus.Ended).ShouldBe(2); }
    [Fact] public void ShouldHaveExactlyTwoValues() { LogAct("Getting values"); Enum.GetValues<ImpersonationSessionStatus>().Length.ShouldBe(2); }

    [Theory]
    [InlineData(ImpersonationSessionStatus.Active, "Active")]
    [InlineData(ImpersonationSessionStatus.Ended, "Ended")]
    public void ToString_ShouldReturnCorrectName(ImpersonationSessionStatus status, string expected)
    {
        LogAct("Converting to string");
        status.ToString().ShouldBe(expected);
    }
}
