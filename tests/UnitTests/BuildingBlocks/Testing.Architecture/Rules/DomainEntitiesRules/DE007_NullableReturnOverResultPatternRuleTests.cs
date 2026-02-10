using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

public class DE007_NullableReturnOverResultPatternRuleTests : TestBase
{
    public DE007_NullableReturnOverResultPatternRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void MethodReturningNullable_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with method returning nullable");
        var rule = new DE007_NullableReturnOverResultPatternRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Order : EntityBase<Order>
            {
                public Order? Update(string status) => this;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing method returning nullable");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying method returning nullable passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void MethodReturningResult_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with method returning Result<T>");
        var rule = new DE007_NullableReturnOverResultPatternRule();
        var source = """
            #nullable enable
            public class Result<T> { }
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Product : EntityBase<Product>
            {
                public Result<Product> Update(string name) => new Result<Product>();
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing method returning Result<T>");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying method returning Result<T> fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Product");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE007_NullableReturnOverResultPattern");
        typeResult.Violation.Message.ShouldContain("Result");
    }

    [Fact]
    public void MethodReturningEither_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with method returning Either<L,R>");
        var rule = new DE007_NullableReturnOverResultPatternRule();
        var source = """
            #nullable enable
            public class Either<TLeft, TRight> { }
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer>
            {
                public Either<string, Customer> Create(string name) => new Either<string, Customer>();
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing method returning Either<L,R>");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying method returning Either<L,R> fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Customer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation!.Message.ShouldContain("Either");
    }

    [Fact]
    public void MethodReturningErrorOr_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with method returning ErrorOr<T>");
        var rule = new DE007_NullableReturnOverResultPatternRule();
        var source = """
            #nullable enable
            public class ErrorOr<T> { }
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Invoice : EntityBase<Invoice>
            {
                public ErrorOr<Invoice> Process() => new ErrorOr<Invoice>();
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing method returning ErrorOr<T>");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying method returning ErrorOr<T> fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Invoice");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
    }

    [Fact]
    public void ValidateMethod_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating entity with Validate method returning bool");
        var rule = new DE007_NullableReturnOverResultPatternRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Payment : EntityBase<Payment>
            {
                public bool ValidateAmount() => true;
                public bool IsValid() => true;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing Validate methods");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying Validate methods are ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Payment");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ToStringOverride_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating entity with ToString override");
        var rule = new DE007_NullableReturnOverResultPatternRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Shipment : EntityBase<Shipment>
            {
                public override string ToString() => "Shipment";
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing ToString override");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying ToString override is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Shipment");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void PrivateMethod_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating entity with private method returning Result");
        var rule = new DE007_NullableReturnOverResultPatternRule();
        var source = """
            #nullable enable
            public class Result<T> { }
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Account : EntityBase<Account>
            {
                private Result<Account> ProcessInternal() => new Result<Account>();
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing private method");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying private method is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Account");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ClassNotInheritingEntityBase_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating class not inheriting EntityBase");
        var rule = new DE007_NullableReturnOverResultPatternRule();
        var source = """
            #nullable enable
            public class Result<T> { }
            public sealed class RegularClass
            {
                public Result<RegularClass> Process() => new Result<RegularClass>();
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
        var rule = new DE007_NullableReturnOverResultPatternRule();
        var source = """
            #nullable enable
            public class Result<T> { }
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractEntity : EntityBase<AbstractEntity>
            {
                public Result<AbstractEntity> Process() => new Result<AbstractEntity>();
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
        var rule = new DE007_NullableReturnOverResultPatternRule();
        var source = """
            #nullable enable
            public class Result<T> { }
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Warehouse : EntityBase<Warehouse>
            {
                public Result<Warehouse> Update() => new Result<Warehouse>();
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing violation metadata");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation metadata fields");
        var typeResult = results[0].TypeResults.First(t => t.TypeName == "Warehouse");
        var violation = typeResult.Violation!;

        violation.Rule.ShouldBe("DE007_NullableReturnOverResultPattern");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-007-retorno-nullable-vs-result-pattern.md");
        violation.LlmHint.ShouldContain("Warehouse?");
        violation.LlmHint.ShouldContain("ExecutionContext");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE007_NullableReturnOverResultPatternRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE007_NullableReturnOverResultPattern");
        rule.Description.ShouldContain("Result");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-007-retorno-nullable-vs-result-pattern.md");
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
