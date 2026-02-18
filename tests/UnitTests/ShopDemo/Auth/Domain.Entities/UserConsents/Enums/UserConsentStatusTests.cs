using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.UserConsents.Enums;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.UserConsents.Enums;

public class UserConsentStatusTests : TestBase
{
    public UserConsentStatusTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact] public void Active_ShouldHaveValue1() { LogAct("Casting"); ((int)UserConsentStatus.Active).ShouldBe(1); }
    [Fact] public void Revoked_ShouldHaveValue2() { LogAct("Casting"); ((int)UserConsentStatus.Revoked).ShouldBe(2); }
    [Fact] public void ShouldHaveExactlyTwoValues() { LogAct("Getting values"); Enum.GetValues<UserConsentStatus>().Length.ShouldBe(2); }

    [Theory]
    [InlineData(UserConsentStatus.Active, "Active")]
    [InlineData(UserConsentStatus.Revoked, "Revoked")]
    public void ToString_ShouldReturnCorrectName(UserConsentStatus status, string expected)
    {
        LogAct("Converting to string");
        status.ToString().ShouldBe(expected);
    }
}
