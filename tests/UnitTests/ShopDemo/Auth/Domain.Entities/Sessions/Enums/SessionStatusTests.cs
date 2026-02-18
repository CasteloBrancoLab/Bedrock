using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.Sessions.Enums;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.Sessions.Enums;

public class SessionStatusTests : TestBase
{
    public SessionStatusTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact] public void Active_ShouldHaveValue1() { LogAct("Casting"); ((int)SessionStatus.Active).ShouldBe(1); }
    [Fact] public void Revoked_ShouldHaveValue2() { LogAct("Casting"); ((int)SessionStatus.Revoked).ShouldBe(2); }
    [Fact] public void ShouldHaveExactlyTwoValues() { LogAct("Getting values"); Enum.GetValues<SessionStatus>().Length.ShouldBe(2); }

    [Theory]
    [InlineData(SessionStatus.Active, "Active")]
    [InlineData(SessionStatus.Revoked, "Revoked")]
    public void ToString_ShouldReturnCorrectName(SessionStatus status, string expected)
    {
        LogAct("Converting to string");
        status.ToString().ShouldBe(expected);
    }
}
