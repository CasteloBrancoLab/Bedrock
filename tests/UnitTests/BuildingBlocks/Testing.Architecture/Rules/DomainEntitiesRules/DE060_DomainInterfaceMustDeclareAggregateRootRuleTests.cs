using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

public class DE060_DomainInterfaceMustDeclareAggregateRootRuleTests : TestBase
{
    public DE060_DomainInterfaceMustDeclareAggregateRootRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void InterfaceWithIAggregateRoot_ShouldPass()
    {
        // Arrange
        LogArrange("Creating aggregate root with domain interface inheriting IAggregateRoot");
        var rule = new DE060_DomainInterfaceMustDeclareAggregateRootRule();
        var source = """
            #nullable enable
            public interface IEntity { }
            public interface IAggregateRoot : IEntity { }
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public interface IUser : IAggregateRoot
            {
                string Username { get; }
            }
            public sealed class User : EntityBase<User>, IUser
            {
                public string Username { get; private set; } = string.Empty;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing aggregate root with correct domain interface");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying aggregate root with IAggregateRoot interface passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "User");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void InterfaceWithIEntity_OnAggregateRoot_ShouldFail()
    {
        // Arrange
        LogArrange("Creating aggregate root with domain interface inheriting only IEntity");
        var rule = new DE060_DomainInterfaceMustDeclareAggregateRootRule();
        var source = """
            #nullable enable
            public interface IEntity { }
            public interface IAggregateRoot : IEntity { }
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public interface IUser : IEntity
            {
                string Username { get; }
            }
            public sealed class User : EntityBase<User>, IAggregateRoot, IUser
            {
                public string Username { get; private set; } = string.Empty;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing aggregate root with IEntity-only domain interface");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying aggregate root with IEntity-only interface fails");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "User");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("DE060_DomainInterfaceMustDeclareAggregateRoot");
        typeResult.Violation.Message.ShouldContain("IUser");
        typeResult.Violation.Message.ShouldContain("IAggregateRoot");
    }

    [Fact]
    public void InterfaceWithoutEntityHierarchy_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating aggregate root with non-domain interface");
        var rule = new DE060_DomainInterfaceMustDeclareAggregateRootRule();
        var source = """
            #nullable enable
            public interface IEntity { }
            public interface IAggregateRoot : IEntity { }
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public interface IFormattable
            {
                string Format();
            }
            public sealed class Order : EntityBase<Order>, IAggregateRoot, IFormattable
            {
                public string Format() => string.Empty;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing aggregate root with non-domain interface");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying non-domain interface is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "Order");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void EntityNotAggregateRoot_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating entity (not aggregate root) with IEntity interface");
        var rule = new DE060_DomainInterfaceMustDeclareAggregateRootRule();
        var source = """
            #nullable enable
            public interface IEntity { }
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public interface IOrderItem : IEntity
            {
                decimal Price { get; }
            }
            public sealed class OrderItem : EntityBase<OrderItem>, IOrderItem
            {
                public decimal Price { get; private set; }
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing entity that is not an aggregate root");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying non-aggregate-root entity is ignored");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "OrderItem");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ClassWithoutDomainInterface_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating aggregate root without any domain interface");
        var rule = new DE060_DomainInterfaceMustDeclareAggregateRootRule();
        var source = """
            #nullable enable
            public interface IEntity { }
            public interface IAggregateRoot : IEntity { }
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public sealed class SimpleAggregateRoot : EntityBase<SimpleAggregateRoot>, IAggregateRoot
            {
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing aggregate root without domain interface");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying aggregate root without domain interface passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "SimpleAggregateRoot");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void InterfaceInheritingIAggregateRootIndirectly_ShouldPass()
    {
        // Arrange
        LogArrange("Creating aggregate root with interface inheriting IAggregateRoot indirectly");
        var rule = new DE060_DomainInterfaceMustDeclareAggregateRootRule();
        var source = """
            #nullable enable
            public interface IEntity { }
            public interface IAggregateRoot : IEntity { }
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public interface IBaseUser : IAggregateRoot
            {
                string Username { get; }
            }
            public interface IUser : IBaseUser
            {
                string Email { get; }
            }
            public sealed class User : EntityBase<User>, IUser
            {
                public string Username { get; private set; } = string.Empty;
                public string Email { get; private set; } = string.Empty;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing aggregate root with indirect IAggregateRoot inheritance");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying indirect IAggregateRoot inheritance passes");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "User");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void ViolationMetadata_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating aggregate root to verify violation metadata");
        var rule = new DE060_DomainInterfaceMustDeclareAggregateRootRule();
        var source = """
            #nullable enable
            public interface IEntity { }
            public interface IAggregateRoot : IEntity { }
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public interface IInvoice : IEntity
            {
                string Number { get; }
            }
            public sealed class Invoice : EntityBase<Invoice>, IAggregateRoot, IInvoice
            {
                public string Number { get; private set; } = string.Empty;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing violation metadata");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation metadata fields");
        var typeResult = results[0].TypeResults.First(t => t.TypeName == "Invoice");
        var violation = typeResult.Violation!;

        violation.Rule.ShouldBe("DE060_DomainInterfaceMustDeclareAggregateRoot");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/domain-entities/DE-060-interface-dominio-deve-declarar-iaggregateroot.md");
        violation.LlmHint.ShouldContain("IAggregateRoot");
        violation.LlmHint.ShouldContain("IInvoice");
        violation.Message.ShouldContain("IInvoice");
        violation.Message.ShouldContain("Invoice");
    }

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new DE060_DomainInterfaceMustDeclareAggregateRootRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("DE060_DomainInterfaceMustDeclareAggregateRoot");
        rule.Description.ShouldContain("IAggregateRoot");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/domain-entities/DE-060-interface-dominio-deve-declarar-iaggregateroot.md");
    }

    [Fact]
    public void AbstractClass_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating abstract class with IEntity interface");
        var rule = new DE060_DomainInterfaceMustDeclareAggregateRootRule();
        var source = """
            #nullable enable
            public interface IEntity { }
            public interface IAggregateRoot : IEntity { }
            public abstract class EntityBase<T> where T : EntityBase<T> { }
            public interface ICustomer : IEntity
            {
                string Name { get; }
            }
            public abstract class CustomerBase : EntityBase<CustomerBase>, IAggregateRoot, ICustomer
            {
                public string Name { get; } = string.Empty;
            }
            """;
        var compilations = CreateCompilations(source);

        // Act
        LogAct("Analyzing abstract class");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying abstract class is ignored by DomainEntityRuleBase");
        results.Count.ShouldBe(1);
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "CustomerBase");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
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
