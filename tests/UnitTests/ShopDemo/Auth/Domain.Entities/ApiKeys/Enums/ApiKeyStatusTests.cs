using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.ApiKeys.Enums;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.ApiKeys.Enums;

public class ApiKeyStatusTests : TestBase
{
    public ApiKeyStatusTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact] public void Active_ShouldHaveValue1() { LogAct("Casting"); ((int)ApiKeyStatus.Active).ShouldBe(1); }
    [Fact] public void Revoked_ShouldHaveValue2() { LogAct("Casting"); ((int)ApiKeyStatus.Revoked).ShouldBe(2); }
    [Fact] public void ShouldHaveExactlyTwoValues() { LogAct("Getting values"); Enum.GetValues<ApiKeyStatus>().Length.ShouldBe(2); }

    [Theory]
    [InlineData(ApiKeyStatus.Active, "Active")]
    [InlineData(ApiKeyStatus.Revoked, "Revoked")]
    public void ToString_ShouldReturnCorrectName(ApiKeyStatus status, string expected)
    {
        LogAct("Converting to string");
        status.ToString().ShouldBe(expected);
    }
}
