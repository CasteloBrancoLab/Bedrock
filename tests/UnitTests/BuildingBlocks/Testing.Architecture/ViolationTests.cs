using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture;

public class ViolationTests : TestBase
{
    public ViolationTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Violation_ShouldStoreAllRequiredProperties()
    {
        // Arrange
        LogArrange("Creating a Violation with all required properties");

        // Act
        LogAct("Instantiating Violation");
        var violation = new Violation
        {
            Rule = "SealedClass",
            Severity = Severity.Error,
            Adr = "docs/adr/001-sealed-entities.md",
            Project = "Bedrock.BuildingBlocks.Domain",
            File = "src/Entities/Customer.cs",
            Line = 10,
            Message = "Entity classes must be sealed",
            LlmHint = "Add the sealed modifier to the class declaration"
        };

        // Assert
        LogAssert("Verifying all properties are correctly stored");
        violation.Rule.ShouldBe("SealedClass");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adr/001-sealed-entities.md");
        violation.Project.ShouldBe("Bedrock.BuildingBlocks.Domain");
        violation.File.ShouldBe("src/Entities/Customer.cs");
        violation.Line.ShouldBe(10);
        violation.Message.ShouldBe("Entity classes must be sealed");
        violation.LlmHint.ShouldBe("Add the sealed modifier to the class declaration");
    }
}
