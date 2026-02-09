using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules;

public class DE018_ReconstitutionDoesNotValidateRuleTests : TestBase
{
    public DE018_ReconstitutionDoesNotValidateRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void CreateFromExistingInfoWithoutValidation_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with CreateFromExistingInfo not calling validation");
        var rule = new DE018_ReconstitutionDoesNotValidateRule();
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
        LogAct("Analyzing CreateFromExistingInfo without validation");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying CreateFromExistingInfo without validation passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void CreateFromExistingInfoCallingValidateMethod_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with CreateFromExistingInfo calling Validate method");
        var rule = new DE018_ReconstitutionDoesNotValidateRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Product : EntityBase<Product>
            {
                public static Product CreateFromExistingInfo(object input)
                {
                    ValidateName("test");
                    return new Product();
                }

                public static bool ValidateName(string? name) => true;

                private Product() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing CreateFromExistingInfo calling Validate method");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying CreateFromExistingInfo calling Validate method fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Product");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE018_ReconstitutionDoesNotValidate");
        typeResult.Violation.Message.ShouldContain("ValidateName");
    }

    [Fact]
    public void CreateFromExistingInfoCallingIsValid_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with CreateFromExistingInfo calling IsValid");
        var rule = new DE018_ReconstitutionDoesNotValidateRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer>
            {
                public static Customer CreateFromExistingInfo(object input)
                {
                    IsValid("test");
                    return new Customer();
                }

                public static bool IsValid(string? name) => true;

                private Customer() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing CreateFromExistingInfo calling IsValid");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying CreateFromExistingInfo calling IsValid fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Customer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation!.Message.ShouldContain("IsValid");
    }

    [Fact]
    public void EntityWithoutCreateFromExistingInfo_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity without CreateFromExistingInfo");
        var rule = new DE018_ReconstitutionDoesNotValidateRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Invoice : EntityBase<Invoice>
            {
                private Invoice() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity without CreateFromExistingInfo");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying entity without CreateFromExistingInfo passes (checked by DE-017)");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Invoice");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ClassNotInheritingEntityBase_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating class not inheriting EntityBase");
        var rule = new DE018_ReconstitutionDoesNotValidateRule();
        var source = """
            #nullable enable
            public sealed class RegularClass
            {
                public static RegularClass CreateFromExistingInfo(object input)
                {
                    ValidateName("test"); // Should be ignored
                    return new RegularClass();
                }

                public static bool ValidateName(string? name) => true;
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
        var rule = new DE018_ReconstitutionDoesNotValidateRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractEntity : EntityBase<AbstractEntity>
            {
                public static AbstractEntity CreateFromExistingInfo(object input)
                {
                    ValidateName("test"); // Should be ignored
                    return null!;
                }

                public static bool ValidateName(string? name) => true;
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
        var rule = new DE018_ReconstitutionDoesNotValidateRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Entity : EntityBase<Entity>
            {
                public static Entity CreateFromExistingInfo(object input)
                {
                    ValidateName("test");
                    return new Entity();
                }

                public static bool ValidateName(string? name) => true;

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

        violation.Rule.ShouldBe("DE018_ReconstitutionDoesNotValidate");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-018-reconstitution-nao-valida-dados.md");
        violation.LlmHint.ShouldContain("ValidateName");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE018_ReconstitutionDoesNotValidateRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE018_ReconstitutionDoesNotValidate");
        rule.Description.ShouldContain("CreateFromExistingInfo");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-018-reconstitution-nao-valida-dados.md");
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
