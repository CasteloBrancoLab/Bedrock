using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

public class DE017_RegisterNewAndCreateFromExistingInfoRuleTests : TestBase
{
    public DE017_RegisterNewAndCreateFromExistingInfoRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void EntityWithCreateFromExistingInfoReturningNonNullable_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with CreateFromExistingInfo returning non-nullable");
        var rule = new DE017_RegisterNewAndCreateFromExistingInfoRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Order : EntityBase<Order>
            {
                public static Order CreateFromExistingInfo(object input)
                {
                    return new Order();
                }

                private Order() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with CreateFromExistingInfo returning non-nullable");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with CreateFromExistingInfo returning non-nullable passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void EntityWithoutCreateFromExistingInfo_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity without CreateFromExistingInfo");
        var rule = new DE017_RegisterNewAndCreateFromExistingInfoRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Product : EntityBase<Product>
            {
                private Product() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity without CreateFromExistingInfo");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity without CreateFromExistingInfo fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Product");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE017_RegisterNewAndCreateFromExistingInfo");
        typeResult.Violation.Message.ShouldContain("CreateFromExistingInfo");
    }

    [Fact]
    public void EntityWithCreateFromExistingInfoReturningNullable_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with CreateFromExistingInfo returning nullable");
        var rule = new DE017_RegisterNewAndCreateFromExistingInfoRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer>
            {
                public static Customer? CreateFromExistingInfo(object input)
                {
                    return null;
                }

                private Customer() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with CreateFromExistingInfo returning nullable");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with CreateFromExistingInfo returning nullable fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Customer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation!.Message.ShouldContain("non-nullable");
    }

    [Fact]
    public void EntityWithPrivateCreateFromExistingInfo_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with private CreateFromExistingInfo");
        var rule = new DE017_RegisterNewAndCreateFromExistingInfoRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Invoice : EntityBase<Invoice>
            {
                private static Invoice CreateFromExistingInfo(object input)
                {
                    return new Invoice();
                }

                private Invoice() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity with private CreateFromExistingInfo");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity with private CreateFromExistingInfo fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Invoice");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
    }

    [Fact]
    public void ClassNotInheritingEntityBase_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating class not inheriting EntityBase");
        var rule = new DE017_RegisterNewAndCreateFromExistingInfoRule();
        var source = """
            #nullable enable
            public sealed class RegularClass
            {
                // No CreateFromExistingInfo but should be ignored
            }
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
        var rule = new DE017_RegisterNewAndCreateFromExistingInfoRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractEntity : EntityBase<AbstractEntity>
            {
                // No CreateFromExistingInfo but should be ignored
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
    public void ViolationMetadata_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating entity to verify violation metadata");
        var rule = new DE017_RegisterNewAndCreateFromExistingInfoRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Entity : EntityBase<Entity>
            {
                private Entity() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing violation metadata");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation metadata fields");
        var typeResult = results[0].TypeResults.First(t => t.TypeName == "Entity");
        var violation = typeResult.Violation!;

        violation.Rule.ShouldBe("DE017_RegisterNewAndCreateFromExistingInfo");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-017-separacao-registernew-vs-createfromexistinginfo.md");
        violation.LlmHint.ShouldContain("CreateFromExistingInfo");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE017_RegisterNewAndCreateFromExistingInfoRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE017_RegisterNewAndCreateFromExistingInfo");
        rule.Description.ShouldContain("CreateFromExistingInfo");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-017-separacao-registernew-vs-createfromexistinginfo.md");
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
