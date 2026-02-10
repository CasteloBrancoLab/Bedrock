using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

public class DE030_MessageCodesWithCreateMessageCodeRuleTests : TestBase
{
    public DE030_MessageCodesWithCreateMessageCodeRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void ValidateMethodWithValidationUtilsAndCreateMessageCode_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with Validate method using ValidationUtils and CreateMessageCode");
        var rule = new DE030_MessageCodesWithCreateMessageCodeRule();
        var source = """
            #nullable enable
            public static class ValidationUtils
            {
                public static bool ValidateIsRequired(object ctx, string? value, string code) => true;
            }
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Order : EntityBase<Order>
            {
                public static bool ValidateName(object ctx, string? name)
                {
                    var code = CreateMessageCode<Order>("Name");
                    return ValidationUtils.ValidateIsRequired(ctx, name, code);
                }

                private static string CreateMessageCode<T>(string propertyName) => $"{typeof(T).Name}.{propertyName}";
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing Validate method with ValidationUtils and CreateMessageCode");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying Validate method with ValidationUtils and CreateMessageCode passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ValidateMethodWithValidationUtilsWithoutCreateMessageCode_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with Validate method using ValidationUtils without CreateMessageCode");
        var rule = new DE030_MessageCodesWithCreateMessageCodeRule();
        var source = """
            #nullable enable
            public static class ValidationUtils
            {
                public static bool ValidateIsRequired(object ctx, string? value, string code) => true;
            }
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Product : EntityBase<Product>
            {
                public static bool ValidateName(object ctx, string? name)
                {
                    return ValidationUtils.ValidateIsRequired(ctx, name, "Product.Name");
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing Validate method without CreateMessageCode");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying Validate method without CreateMessageCode fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Product");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE030_MessageCodesWithCreateMessageCode");
        typeResult.Violation.Message.ShouldContain("CreateMessageCode");
    }

    [Fact]
    public void ValidateMethodWithoutValidationUtils_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with Validate method without ValidationUtils");
        var rule = new DE030_MessageCodesWithCreateMessageCodeRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer>
            {
                public static bool ValidateName(object ctx, string? name)
                {
                    return !string.IsNullOrEmpty(name);
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing Validate method without ValidationUtils");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying Validate method without ValidationUtils passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Customer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void NonValidateMethod_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating entity with non-Validate method");
        var rule = new DE030_MessageCodesWithCreateMessageCodeRule();
        var source = """
            #nullable enable
            public static class ValidationUtils
            {
                public static bool ValidateIsRequired(object ctx, string? value, string code) => true;
            }
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Invoice : EntityBase<Invoice>
            {
                public static bool IsValid(object ctx, string? name)
                {
                    return ValidationUtils.ValidateIsRequired(ctx, name, "hardcoded"); // OK - not Validate*
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing non-Validate method");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying non-Validate method is ignored");
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
        var rule = new DE030_MessageCodesWithCreateMessageCodeRule();
        var source = """
            #nullable enable
            public static class ValidationUtils
            {
                public static bool ValidateIsRequired(object ctx, string? value, string code) => true;
            }
            public sealed class RegularClass
            {
                public static bool ValidateName(object ctx, string? name)
                {
                    return ValidationUtils.ValidateIsRequired(ctx, name, "hardcoded");
                }
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
        var rule = new DE030_MessageCodesWithCreateMessageCodeRule();
        var source = """
            #nullable enable
            public static class ValidationUtils
            {
                public static bool ValidateIsRequired(object ctx, string? value, string code) => true;
            }
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractEntity : EntityBase<AbstractEntity>
            {
                public static bool ValidateName(object ctx, string? name)
                {
                    return ValidationUtils.ValidateIsRequired(ctx, name, "hardcoded");
                }
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
        var rule = new DE030_MessageCodesWithCreateMessageCodeRule();
        var source = """
            #nullable enable
            public static class ValidationUtils
            {
                public static bool ValidateIsRequired(object ctx, string? value, string code) => true;
            }
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Entity : EntityBase<Entity>
            {
                public static bool ValidateName(object ctx, string? name)
                {
                    return ValidationUtils.ValidateIsRequired(ctx, name, "hardcoded");
                }
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

        violation.Rule.ShouldBe("DE030_MessageCodesWithCreateMessageCode");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-030-message-codes-com-createmessagecode.md");
        violation.LlmHint.ShouldContain("CreateMessageCode");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE030_MessageCodesWithCreateMessageCodeRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE030_MessageCodesWithCreateMessageCode");
        rule.Description.ShouldContain("CreateMessageCode");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-030-message-codes-com-createmessagecode.md");
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
