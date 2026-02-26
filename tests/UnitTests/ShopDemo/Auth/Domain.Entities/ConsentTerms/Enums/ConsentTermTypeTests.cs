using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.ConsentTerms.Enums;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.ConsentTerms.Enums;

public class ConsentTermTypeTests : TestBase
{
    public ConsentTermTypeTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact] public void TermsOfUse_ShouldHaveValue1() { LogAct("Casting"); ((int)ConsentTermType.TermsOfUse).ShouldBe(1); }
    [Fact] public void PrivacyPolicy_ShouldHaveValue2() { LogAct("Casting"); ((int)ConsentTermType.PrivacyPolicy).ShouldBe(2); }
    [Fact] public void Marketing_ShouldHaveValue3() { LogAct("Casting"); ((int)ConsentTermType.Marketing).ShouldBe(3); }
    [Fact] public void ShouldHaveExactlyThreeValues() { LogAct("Getting values"); Enum.GetValues<ConsentTermType>().Length.ShouldBe(3); }

    [Theory]
    [InlineData(ConsentTermType.TermsOfUse, "TermsOfUse")]
    [InlineData(ConsentTermType.PrivacyPolicy, "PrivacyPolicy")]
    [InlineData(ConsentTermType.Marketing, "Marketing")]
    public void ToString_ShouldReturnCorrectName(ConsentTermType type, string expected)
    {
        LogAct("Converting to string");
        type.ToString().ShouldBe(expected);
    }
}
