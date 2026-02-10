using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

public class DE054_MaxInheritanceDepthRuleTests : TestBase
{
    public DE054_MaxInheritanceDepthRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void EntityWithOneAbstractLevel_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with one abstract level");
        var rule = new DE054_MaxInheritanceDepthRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractOrder : EntityBase<AbstractOrder> { }
            public sealed class Order : AbstractOrder { }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with one abstract level");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with one abstract level passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void EntityWithTwoAbstractLevels_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with two abstract levels");
        var rule = new DE054_MaxInheritanceDepthRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractLevel1 : EntityBase<AbstractLevel1> { }
            public abstract class AbstractLevel2 : AbstractLevel1 { }
            public sealed class Product : AbstractLevel2 { }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with two abstract levels");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with two abstract levels fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Product");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE054_MaxInheritanceDepth");
        typeResult.Violation.Message.ShouldContain("2");
    }

    [Fact]
    public void EntityDirectlyFromEntityBase_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity directly from EntityBase");
        var rule = new DE054_MaxInheritanceDepthRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer> { }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity directly from EntityBase");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity directly from EntityBase passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Customer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ClassNotInheritingEntityBase_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating class not inheriting EntityBase");
        var rule = new DE054_MaxInheritanceDepthRule();
        var source = """
            #nullable enable
            public abstract class Level1 { }
            public abstract class Level2 : Level1 { }
            public abstract class Level3 : Level2 { }
            public sealed class RegularClass : Level3 { }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing class not inheriting EntityBase");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying class not inheriting EntityBase is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "RegularClass");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void AbstractClass_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating abstract class");
        var rule = new DE054_MaxInheritanceDepthRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractLevel1 : EntityBase<AbstractLevel1> { }
            public abstract class AbstractLevel2 : AbstractLevel1 { }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing abstract class");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying abstract class is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "AbstractLevel2");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ViolationMetadata_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating entity to verify violation metadata");
        var rule = new DE054_MaxInheritanceDepthRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class Level1 : EntityBase<Level1> { }
            public abstract class Level2 : Level1 { }
            public sealed class Entity : Level2 { }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing violation metadata");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation metadata fields");
        var typeResult = results[0].TypeResults.First(t => t.TypeName == "Entity");
        var violation = typeResult.Violation!;

        violation.Rule.ShouldBe("DE054_MaxInheritanceDepth");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-054-heranca-vs-composicao-em-entidades.md");
        violation.LlmHint.ShouldContain("composicao");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE054_MaxInheritanceDepthRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE054_MaxInheritanceDepth");
        rule.Description.ShouldContain("heranca");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-054-heranca-vs-composicao-em-entidades.md");
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
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithNullableContextOptions(NullableContextOptions.Enable));
    }

    #endregion
}
