using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.Tenants.Enums;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.Tenants.Enums;

public class TenantTierTests : TestBase
{
    public TenantTierTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact] public void Basic_ShouldHaveValue1() { LogAct("Casting"); ((int)TenantTier.Basic).ShouldBe(1); }
    [Fact] public void Professional_ShouldHaveValue2() { LogAct("Casting"); ((int)TenantTier.Professional).ShouldBe(2); }
    [Fact] public void Enterprise_ShouldHaveValue3() { LogAct("Casting"); ((int)TenantTier.Enterprise).ShouldBe(3); }
    [Fact] public void ShouldHaveExactlyThreeValues() { LogAct("Getting values"); Enum.GetValues<TenantTier>().Length.ShouldBe(3); }

    [Theory]
    [InlineData(TenantTier.Basic, "Basic")]
    [InlineData(TenantTier.Professional, "Professional")]
    [InlineData(TenantTier.Enterprise, "Enterprise")]
    public void ToString_ShouldReturnCorrectName(TenantTier tier, string expected)
    {
        LogAct("Converting to string");
        tier.ToString().ShouldBe(expected);
    }
}
