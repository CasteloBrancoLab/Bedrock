using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

public class DE033_NotReadonlyStructRuleTests : TestBase
{
    public DE033_NotReadonlyStructRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void SealedClassInheritingEntityBase_ShouldPass()
    {
        // Arrange
        LogArrange("Creating sealed class inheriting EntityBase");
        var rule = new DE033_NotReadonlyStructRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Order : EntityBase<Order>
            {
                public string? Name { get; private set; }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing sealed class inheriting EntityBase");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying sealed class inheriting EntityBase passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ReadonlyStruct_ShouldNotBeAnalyzedAsEntity()
    {
        // Arrange
        LogArrange("Creating readonly struct (not an entity)");
        var rule = new DE033_NotReadonlyStructRule();
        var source = """
            #nullable enable
            public readonly struct ValueObject
            {
                public string Value { get; }
                public ValueObject(string value) => Value = value;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing readonly struct");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying readonly struct is not treated as entity");
        results.Count.ShouldBe(1);
        // Struct should pass or be ignored since it's not an entity
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "ValueObject");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void RecordStruct_ShouldNotBeAnalyzedAsEntity()
    {
        // Arrange
        LogArrange("Creating record struct (not an entity)");
        var rule = new DE033_NotReadonlyStructRule();
        var source = """
            #nullable enable
            public readonly record struct InputObject(string Name, int Value);
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing record struct");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying record struct is not treated as entity");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "InputObject");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void RegularClass_ShouldPass()
    {
        // Arrange
        LogArrange("Creating regular class");
        var rule = new DE033_NotReadonlyStructRule();
        var source = """
            #nullable enable
            public sealed class RegularClass
            {
                public string? Name { get; set; }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing regular class");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying regular class passes");
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
        var rule = new DE033_NotReadonlyStructRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractEntity : EntityBase<AbstractEntity>
            {
                public string? Name { get; protected set; }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing abstract class");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying abstract class is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "AbstractEntity");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE033_NotReadonlyStructRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE033_NotReadonlyStruct");
        rule.Description.ShouldContain("readonly struct");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-033-antipadrao-readonly-struct-para-entidades.md");
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
