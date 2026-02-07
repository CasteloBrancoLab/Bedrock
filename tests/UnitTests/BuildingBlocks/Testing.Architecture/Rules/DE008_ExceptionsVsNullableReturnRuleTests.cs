using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules;

public class DE008_ExceptionsVsNullableReturnRuleTests : TestBase
{
    public DE008_ExceptionsVsNullableReturnRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void MethodWithoutThrow_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity without throw statements");
        var rule = new DE008_ExceptionsVsNullableReturnRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Order : EntityBase<Order>
            {
                public Order? Update(string status) => null;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing method without throw");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying method without throw passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void MethodWithThrowStatement_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with throw statement");
        var rule = new DE008_ExceptionsVsNullableReturnRule();
        var source = """
            #nullable enable
            using System;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Product : EntityBase<Product>
            {
                public Product? Update(string name)
                {
                    throw new InvalidOperationException("Invalid");
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing method with throw statement");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying method with throw statement fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Product");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE008_ExceptionsVsNullableReturn");
        typeResult.Violation.Message.ShouldContain("throw");
    }

    [Fact]
    public void MethodWithThrowExpression_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with throw expression");
        var rule = new DE008_ExceptionsVsNullableReturnRule();
        var source = """
            #nullable enable
            using System;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer>
            {
                public string GetName(string? name) => name ?? throw new ArgumentException("Name required");
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing method with throw expression");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying method with throw expression fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Customer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
    }

    [Fact]
    public void MethodWithArgumentNullExceptionThrowIfNull_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with ArgumentNullException.ThrowIfNull (allowed guard clause)");
        var rule = new DE008_ExceptionsVsNullableReturnRule();
        var source = """
            #nullable enable
            using System;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Invoice : EntityBase<Invoice>
            {
                public Invoice? Process(object dependency)
                {
                    ArgumentNullException.ThrowIfNull(dependency);
                    return this;
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing method with ThrowIfNull guard clause");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying ThrowIfNull guard clause is allowed");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Invoice");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void MethodWithThrowIfNullOrWhiteSpace_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with ArgumentException.ThrowIfNullOrWhiteSpace (allowed guard clause)");
        var rule = new DE008_ExceptionsVsNullableReturnRule();
        var source = """
            #nullable enable
            using System;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Payment : EntityBase<Payment>
            {
                public Payment? Process(string? name)
                {
                    ArgumentException.ThrowIfNullOrWhiteSpace(name);
                    return this;
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing method with ThrowIfNullOrWhiteSpace guard clause");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying ThrowIfNullOrWhiteSpace guard clause is allowed");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Payment");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void MethodWithGuardClauseInIfStatement_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with ArgumentNullException in if statement");
        var rule = new DE008_ExceptionsVsNullableReturnRule();
        var source = """
            #nullable enable
            using System;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Shipment : EntityBase<Shipment>
            {
                public Shipment? Process(object? dependency)
                {
                    if (dependency == null)
                        throw new ArgumentNullException(nameof(dependency));
                    return this;
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing method with guard clause in if statement");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying guard clause in if statement is allowed");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Shipment");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ClassNotInheritingEntityBase_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating class not inheriting EntityBase");
        var rule = new DE008_ExceptionsVsNullableReturnRule();
        var source = """
            #nullable enable
            using System;
            public sealed class RegularClass
            {
                public void Process()
                {
                    throw new InvalidOperationException("Error");
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
        var rule = new DE008_ExceptionsVsNullableReturnRule();
        var source = """
            #nullable enable
            using System;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractEntity : EntityBase<AbstractEntity>
            {
                public void Process()
                {
                    throw new InvalidOperationException("Error");
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
        var rule = new DE008_ExceptionsVsNullableReturnRule();
        var source = """
            #nullable enable
            using System;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Account : EntityBase<Account>
            {
                public void Update()
                {
                    throw new Exception("Error");
                }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing violation metadata");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation metadata fields");
        var typeResult = results[0].TypeResults.First(t => t.TypeName == "Account");
        var violation = typeResult.Violation!;

        violation.Rule.ShouldBe("DE008_ExceptionsVsNullableReturn");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-008-excecoes-vs-retorno-nullable.md");
        violation.LlmHint.ShouldContain("ExecutionContext");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE008_ExceptionsVsNullableReturnRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE008_ExceptionsVsNullableReturn");
        rule.Description.ShouldContain("exceções");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-008-excecoes-vs-retorno-nullable.md");
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
