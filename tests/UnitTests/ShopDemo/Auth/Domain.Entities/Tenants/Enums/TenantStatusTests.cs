using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.Tenants.Enums;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.Tenants.Enums;

public class TenantStatusTests : TestBase
{
    public TenantStatusTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact] public void Active_ShouldHaveValue1() { LogAct("Casting"); ((int)TenantStatus.Active).ShouldBe(1); }
    [Fact] public void Suspended_ShouldHaveValue2() { LogAct("Casting"); ((int)TenantStatus.Suspended).ShouldBe(2); }
    [Fact] public void Maintenance_ShouldHaveValue3() { LogAct("Casting"); ((int)TenantStatus.Maintenance).ShouldBe(3); }
    [Fact] public void ShouldHaveExactlyThreeValues() { LogAct("Getting values"); Enum.GetValues<TenantStatus>().Length.ShouldBe(3); }

    [Theory]
    [InlineData(TenantStatus.Active, "Active")]
    [InlineData(TenantStatus.Suspended, "Suspended")]
    [InlineData(TenantStatus.Maintenance, "Maintenance")]
    public void ToString_ShouldReturnCorrectName(TenantStatus status, string expected)
    {
        LogAct("Converting to string");
        status.ToString().ShouldBe(expected);
    }
}
