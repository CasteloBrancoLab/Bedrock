using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

public class DE001_SealedClassRuleTests : TestBase
{
    public DE001_SealedClassRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void SealedClass_ShouldPass()
    {
        // Arrange
        LogArrange("Creating sealed class");
        var rule = new DE001_SealedClassRule();
        var source = """
            public sealed class SealedOrder { }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing sealed class");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying sealed class passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "SealedOrder");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void NonSealedClassWithHeirs_ShouldPass()
    {
        // Arrange
        LogArrange("Creating non-sealed class with a subclass");
        var rule = new DE001_SealedClassRule();
        var source = """
            public class BaseProduct { }
            public sealed class ConcreteProduct : BaseProduct { }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing non-sealed class that has heirs");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying non-sealed class with heirs passes");
        results.Count.ShouldBe(1);
        var baseResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "BaseProduct");
        baseResult.ShouldNotBeNull();
        baseResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        baseResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void NonSealedClassWithoutHeirs_ShouldFail()
    {
        // Arrange
        LogArrange("Creating non-sealed class without heirs");
        var rule = new DE001_SealedClassRule();
        var source = """
            public class UnsealedCustomer { }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing non-sealed class without heirs");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying non-sealed class without heirs fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "UnsealedCustomer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE001_SealedClass");
        typeResult.Violation.Message.ShouldContain("UnsealedCustomer");
        typeResult.Violation.Message.ShouldContain("sealed");
    }

    [Fact]
    public void NonSealedClassWithoutHeirs_ViolationMetadata_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating non-sealed class to verify violation metadata");
        var rule = new DE001_SealedClassRule();
        var source = """
            public class OpenInvoice { }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing violation metadata");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation metadata fields");
        var typeResult = results[0].TypeResults.First(t => t.TypeName == "OpenInvoice");
        var violation = typeResult.Violation!;

        violation.Rule.ShouldBe("DE001_SealedClass");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-001-entidades-devem-ser-sealed.md");
        violation.Project.ShouldBe("TestProject");
        violation.LlmHint.ShouldContain("sealed");
        violation.LlmHint.ShouldContain("OpenInvoice");
    }

    [Fact]
    public void AbstractClass_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating abstract class");
        var rule = new DE001_SealedClassRule();
        var source = """
            public abstract class AbstractPayment { }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing abstract class");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying abstract class is ignored (no type results)");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "AbstractPayment");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void Record_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating record type");
        var rule = new DE001_SealedClassRule();
        var source = """
            public record ShippingRecord(string Address);
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing record");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying record is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "ShippingRecord");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void StaticClass_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating static class");
        var rule = new DE001_SealedClassRule();
        var source = """
            public static class DiscountHelper { }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing static class");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying static class is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "DiscountHelper");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void Interface_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating interface");
        var rule = new DE001_SealedClassRule();
        var source = """
            public interface IWarehouse { }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing interface");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying interface is ignored (passed, no violation)");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "IWarehouse");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void Struct_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating struct");
        var rule = new DE001_SealedClassRule();
        var source = """
            public struct PriceAmount { public decimal Value; }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing struct");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying struct is ignored (passed, no violation)");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "PriceAmount");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void Enum_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating enum");
        var rule = new DE001_SealedClassRule();
        var source = """
            public enum OrderStatus { Pending, Completed }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing enum");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying enum is ignored (passed, no violation)");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "OrderStatus");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void MixedClasses_SealedAndNonSealed_ShouldOnlyFailNonSealed()
    {
        // Arrange
        LogArrange("Creating mix of sealed and non-sealed classes");
        var rule = new DE001_SealedClassRule();
        var source = """
            public sealed class SealedWidget { }
            public class OpenGadget { }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing mixed classes");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying only non-sealed fails");
        results.Count.ShouldBe(1);

        var sealedResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "SealedWidget");
        sealedResult.ShouldNotBeNull();
        sealedResult.Status.ShouldBe(TypeAnalysisStatus.Passed);

        var openResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "OpenGadget");
        openResult.ShouldNotBeNull();
        openResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE001_SealedClassRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE001_SealedClass");
        rule.Description.ShouldContain("sealed");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-001-entidades-devem-ser-sealed.md");
    }

    #region Helpers

    private static Dictionary<string, Compilation> CreateCompilations(string source)
    {
        return new Dictionary<string, Compilation>
        {
            ["TestProject"] = CreateSingleCompilation(source, "TestProject")
        };
    }

    private static Compilation CreateSingleCompilation(string source, string assemblyName)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, path: "TestFile.cs");
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
        };

        return CSharpCompilation.Create(
            assemblyName,
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    #endregion
}
