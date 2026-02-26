using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.ServiceClients.Enums;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.ServiceClients.Enums;

public class ServiceClientStatusTests : TestBase
{
    public ServiceClientStatusTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact] public void Active_ShouldHaveValue1() { LogAct("Casting"); ((int)ServiceClientStatus.Active).ShouldBe(1); }
    [Fact] public void Revoked_ShouldHaveValue2() { LogAct("Casting"); ((int)ServiceClientStatus.Revoked).ShouldBe(2); }
    [Fact] public void ShouldHaveExactlyTwoValues() { LogAct("Getting values"); Enum.GetValues<ServiceClientStatus>().Length.ShouldBe(2); }

    [Theory]
    [InlineData(ServiceClientStatus.Active, "Active")]
    [InlineData(ServiceClientStatus.Revoked, "Revoked")]
    public void ToString_ShouldReturnCorrectName(ServiceClientStatus status, string expected)
    {
        LogAct("Converting to string");
        status.ToString().ShouldBe(expected);
    }
}
