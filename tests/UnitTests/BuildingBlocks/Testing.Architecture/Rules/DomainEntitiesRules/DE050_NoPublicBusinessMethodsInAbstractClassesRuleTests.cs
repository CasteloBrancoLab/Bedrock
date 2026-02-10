using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

public class DE050_NoPublicBusinessMethodsInAbstractClassesRuleTests : TestBase
{
    public DE050_NoPublicBusinessMethodsInAbstractClassesRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void AbstractClassWithOnlyProtectedMethods_ShouldPass()
    {
        // Arrange
        LogArrange("Creating abstract class with only protected methods");
        var rule = new DE050_NoPublicBusinessMethodsInAbstractClassesRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractOrder : EntityBase<AbstractOrder>
            {
                protected void SetNameInternal(string name) { }
                public static bool ValidateName(string name) => true;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing abstract class with only protected methods");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying abstract class with only protected methods passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "AbstractOrder");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void AbstractClassWithPublicInstanceMethod_ShouldFail()
    {
        // Arrange
        LogArrange("Creating abstract class with public instance method");
        var rule = new DE050_NoPublicBusinessMethodsInAbstractClassesRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractProduct : EntityBase<AbstractProduct>
            {
                public void ChangeName(string name) { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing abstract class with public instance method");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying abstract class with public instance method fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "AbstractProduct");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE050_NoPublicBusinessMethodsInAbstractClasses");
        typeResult.Violation.Message.ShouldContain("ChangeName");
    }

    [Fact]
    public void AbstractClassWithPublicStaticMethod_ShouldPass()
    {
        // Arrange
        LogArrange("Creating abstract class with public static method");
        var rule = new DE050_NoPublicBusinessMethodsInAbstractClassesRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractCustomer : EntityBase<AbstractCustomer>
            {
                public static bool ValidateName(string name) => true;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing abstract class with public static method");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying abstract class with public static method passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "AbstractCustomer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ConcreteClass_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating concrete class with public instance method");
        var rule = new DE050_NoPublicBusinessMethodsInAbstractClassesRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Order : EntityBase<Order>
            {
                public void ChangeName(string name) { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing concrete class with public instance method");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying concrete class is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void AbstractClassNotInheritingEntityBase_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating abstract class not inheriting EntityBase");
        var rule = new DE050_NoPublicBusinessMethodsInAbstractClassesRule();
        var source = """
            #nullable enable
            public abstract class AbstractClass
            {
                public void DoSomething() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing abstract class not inheriting EntityBase");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying abstract class not inheriting EntityBase is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "AbstractClass");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ViolationMetadata_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating abstract class to verify violation metadata");
        var rule = new DE050_NoPublicBusinessMethodsInAbstractClassesRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractEntity : EntityBase<AbstractEntity>
            {
                public void Update() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing violation metadata");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation metadata fields");
        var typeResult = results[0].TypeResults.First(t => t.TypeName == "AbstractEntity");
        var violation = typeResult.Violation!;

        violation.Rule.ShouldBe("DE050_NoPublicBusinessMethodsInAbstractClasses");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-050-classe-abstrata-nao-expoe-metodos-publicos-negocio.md");
        violation.LlmHint.ShouldContain("Internal");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE050_NoPublicBusinessMethodsInAbstractClassesRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE050_NoPublicBusinessMethodsInAbstractClasses");
        rule.Description.ShouldContain("publicos");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-050-classe-abstrata-nao-expoe-metodos-publicos-negocio.md");
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
