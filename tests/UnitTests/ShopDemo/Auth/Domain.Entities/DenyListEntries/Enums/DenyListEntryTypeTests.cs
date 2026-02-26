using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.DenyListEntries.Enums;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.DenyListEntries.Enums;

public class DenyListEntryTypeTests : TestBase
{
    public DenyListEntryTypeTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact] public void Jti_ShouldHaveValue1() { LogAct("Casting"); ((int)DenyListEntryType.Jti).ShouldBe(1); }
    [Fact] public void UserId_ShouldHaveValue2() { LogAct("Casting"); ((int)DenyListEntryType.UserId).ShouldBe(2); }
    [Fact] public void ShouldHaveExactlyTwoValues() { LogAct("Getting values"); Enum.GetValues<DenyListEntryType>().Length.ShouldBe(2); }

    [Theory]
    [InlineData(DenyListEntryType.Jti, "Jti")]
    [InlineData(DenyListEntryType.UserId, "UserId")]
    public void ToString_ShouldReturnCorrectName(DenyListEntryType type, string expected)
    {
        LogAct("Converting to string");
        type.ToString().ShouldBe(expected);
    }
}
