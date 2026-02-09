using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules;

public class DE032_OptimisticLockingViaEntityInfoRuleTests : TestBase
{
    public DE032_OptimisticLockingViaEntityInfoRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void ConstructorWithEntityInfoParameter_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with constructor receiving EntityInfo");
        var rule = new DE032_OptimisticLockingViaEntityInfoRule();
        var source = """
            #nullable enable
            public class EntityInfo { }
            public abstract class EntityBase<T> where T : EntityBase<T>
            {
                protected EntityBase(EntityInfo entityInfo) { }
            }
            public sealed class Order : EntityBase<Order>
            {
                private Order(EntityInfo entityInfo) : base(entityInfo) { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing constructor with EntityInfo parameter");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying constructor with EntityInfo parameter passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ConstructorWithoutEntityInfoParameter_ShouldFail()
    {
        // Arrange
        LogArrange("Creating entity with constructor missing EntityInfo");
        var rule = new DE032_OptimisticLockingViaEntityInfoRule();
        var source = """
            #nullable enable
            using System;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Product : EntityBase<Product>
            {
                private Product(Guid id, string name) { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing constructor without EntityInfo parameter");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying constructor without EntityInfo parameter fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Product");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE032_OptimisticLockingViaEntityInfo");
        typeResult.Violation.Message.ShouldContain("EntityInfo");
    }

    [Fact]
    public void ParameterlessConstructor_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating entity with only parameterless constructor");
        var rule = new DE032_OptimisticLockingViaEntityInfoRule();
        var source = """
            #nullable enable
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Customer : EntityBase<Customer>
            {
                private Customer() { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing parameterless constructor");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying parameterless constructor is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Customer");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ConstructorWithEntityInfoAsOneOfParameters_ShouldPass()
    {
        // Arrange
        LogArrange("Creating entity with EntityInfo as one of multiple parameters");
        var rule = new DE032_OptimisticLockingViaEntityInfoRule();
        var source = """
            #nullable enable
            using System;
            public class EntityInfo { }
            public abstract class EntityBase<T> where T : EntityBase<T>
            {
                protected EntityBase(EntityInfo entityInfo) { }
            }
            public sealed class Invoice : EntityBase<Invoice>
            {
                private Invoice(EntityInfo entityInfo, string number, DateTime date) : base(entityInfo) { }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing constructor with EntityInfo as one of parameters");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying constructor with EntityInfo as one of parameters passes");
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
        var rule = new DE032_OptimisticLockingViaEntityInfoRule();
        var source = """
            #nullable enable
            using System;
            public sealed class RegularClass
            {
                public RegularClass(Guid id) { }
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
        var rule = new DE032_OptimisticLockingViaEntityInfoRule();
        var source = """
            #nullable enable
            using System;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public abstract class AbstractEntity : EntityBase<AbstractEntity>
            {
                protected AbstractEntity(Guid id) { }
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
        var rule = new DE032_OptimisticLockingViaEntityInfoRule();
        var source = """
            #nullable enable
            using System;
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class Entity : EntityBase<Entity>
            {
                private Entity(Guid id) { }
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

        violation.Rule.ShouldBe("DE032_OptimisticLockingViaEntityInfo");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-032-optimistic-locking-com-entityversion.md");
        violation.LlmHint.ShouldContain("EntityInfo");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE032_OptimisticLockingViaEntityInfoRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE032_OptimisticLockingViaEntityInfo");
        rule.Description.ShouldContain("EntityInfo");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-032-optimistic-locking-com-entityversion.md");
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
