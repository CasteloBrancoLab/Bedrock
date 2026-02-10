using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.CodeStyleRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.CodeStyleRules;

public class CS001_InterfacesInInterfacesNamespaceRuleTests : TestBase
{
    public CS001_InterfacesInInterfacesNamespaceRuleTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    #region Pass Cases

    [Fact]
    public void InterfaceInInterfacesFolder_ShouldPass()
    {
        // Arrange
        LogArrange("Creating interface in Interfaces/ folder with correct namespace");
        var rule = new CS001_InterfacesInInterfacesNamespaceRule();
        var source = """
            namespace MyProject.Passwords.Interfaces;
            public interface IPasswordHasher { }
            """;
        var compilations = CreateCompilations(source, "src/Passwords/Interfaces/IPasswordHasher.cs");

        // Act
        LogAct("Analyzing interface in Interfaces/ folder");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying interface in Interfaces/ folder passes");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "IPasswordHasher");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void InterfaceInNestedInterfacesFolder_ShouldPass()
    {
        // Arrange
        LogArrange("Creating interface in deeply nested Interfaces/ folder");
        var rule = new CS001_InterfacesInInterfacesNamespaceRule();
        var source = """
            namespace MyProject.Connections.Interfaces;
            public interface IConnection { }
            """;
        var compilations = CreateCompilations(source,
            "src/BuildingBlocks/Persistence/Connections/Interfaces/IConnection.cs");

        // Act
        LogAct("Analyzing interface in nested Interfaces/ folder");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying nested Interfaces/ folder passes");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "IConnection");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void ConcreteClass_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating a concrete class (not an interface)");
        var rule = new CS001_InterfacesInInterfacesNamespaceRule();
        var source = """
            namespace MyProject.Passwords;
            public sealed class PasswordHasher { }
            """;
        var compilations = CreateCompilations(source, "src/Passwords/PasswordHasher.cs");

        // Act
        LogAct("Analyzing concrete class");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying concrete class is ignored (passes)");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "PasswordHasher");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void Struct_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating a struct");
        var rule = new CS001_InterfacesInInterfacesNamespaceRule();
        var source = """
            namespace MyProject.Passwords;
            public readonly struct PasswordHash { }
            """;
        var compilations = CreateCompilations(source, "src/Passwords/PasswordHash.cs");

        // Act
        LogAct("Analyzing struct");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying struct is ignored");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "PasswordHash");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void Enum_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating an enum");
        var rule = new CS001_InterfacesInInterfacesNamespaceRule();
        var source = """
            namespace MyProject;
            public enum UserStatus { Active, Blocked }
            """;
        var compilations = CreateCompilations(source, "src/Users/UserStatus.cs");

        // Act
        LogAct("Analyzing enum");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying enum is ignored");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "UserStatus");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void AbstractClass_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating an abstract class");
        var rule = new CS001_InterfacesInInterfacesNamespaceRule();
        var source = """
            namespace MyProject;
            public abstract class RepositoryBase { }
            """;
        var compilations = CreateCompilations(source, "src/Repositories/RepositoryBase.cs");

        // Act
        LogAct("Analyzing abstract class");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying abstract class is ignored");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "RepositoryBase");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void Record_ShouldBeIgnored()
    {
        // Arrange
        LogArrange("Creating a record");
        var rule = new CS001_InterfacesInInterfacesNamespaceRule();
        var source = """
            namespace MyProject;
            public record PasswordHashResult(byte[] Hash);
            """;
        var compilations = CreateCompilations(source, "src/Passwords/PasswordHashResult.cs");

        // Act
        LogAct("Analyzing record");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying record is ignored");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "PasswordHashResult");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    #endregion

    #region Fail Cases

    [Fact]
    public void InterfaceNotInInterfacesFolder_ShouldFail()
    {
        // Arrange
        LogArrange("Creating interface NOT in Interfaces/ folder");
        var rule = new CS001_InterfacesInInterfacesNamespaceRule();
        var source = """
            namespace MyProject.Passwords;
            public interface IPasswordHasher { }
            """;
        var compilations = CreateCompilations(source, "src/Passwords/IPasswordHasher.cs");

        // Act
        LogAct("Analyzing interface not in Interfaces/ folder");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying interface not in Interfaces/ folder fails");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "IPasswordHasher");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
        typeResult.Violation!.Rule.ShouldBe("CS001_InterfacesInInterfacesNamespace");
        typeResult.Violation.Message.ShouldContain("IPasswordHasher");
    }

    [Fact]
    public void InterfaceInRootFolder_ShouldFail()
    {
        // Arrange
        LogArrange("Creating interface in root folder");
        var rule = new CS001_InterfacesInInterfacesNamespaceRule();
        var source = """
            namespace MyProject;
            public interface IMyService { }
            """;
        var compilations = CreateCompilations(source, "src/IMyService.cs");

        // Act
        LogAct("Analyzing interface in root folder");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying interface in root folder fails");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "IMyService");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
    }

    [Fact]
    public void InterfaceInWrongSubfolder_ShouldFail()
    {
        // Arrange
        LogArrange("Creating interface in Contracts/ subfolder (not Interfaces/)");
        var rule = new CS001_InterfacesInInterfacesNamespaceRule();
        var source = """
            namespace MyProject.Passwords.Contracts;
            public interface IHasher { }
            """;
        var compilations = CreateCompilations(source, "src/Passwords/Contracts/IHasher.cs");

        // Act
        LogAct("Analyzing interface in wrong subfolder");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying interface in wrong subfolder fails");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "IHasher");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
        typeResult.Violation.ShouldNotBeNull();
    }

    #endregion

    #region Violation Metadata

    [Fact]
    public void Violation_ShouldHaveCorrectMetadata()
    {
        // Arrange
        LogArrange("Creating interface to verify violation metadata");
        var rule = new CS001_InterfacesInInterfacesNamespaceRule();
        var source = """
            namespace MyProject.Services;
            public interface IOrderService { }
            """;
        var compilations = CreateCompilations(source, "src/Services/IOrderService.cs");

        // Act
        LogAct("Analyzing violation metadata");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying violation metadata fields");
        var typeResult = results[0].TypeResults.First(t => t.TypeName == "IOrderService");
        var violation = typeResult.Violation!;

        violation.Rule.ShouldBe("CS001_InterfacesInInterfacesNamespace");
        violation.Severity.ShouldBe(Severity.Error);
        violation.Adr.ShouldBe("docs/adrs/code-style/CS-001-interfaces-em-namespace-interfaces.md");
        violation.Project.ShouldBe("TestProject");
        violation.Message.ShouldContain("IOrderService");
        violation.Message.ShouldContain("Interfaces/");
        violation.LlmHint.ShouldContain("IOrderService");
        violation.LlmHint.ShouldContain("Interfaces");
    }

    #endregion

    #region Rule Properties

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new CS001_InterfacesInInterfacesNamespaceRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("CS001_InterfacesInInterfacesNamespace");
        rule.Description.ShouldContain("Interfaces/");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/code-style/CS-001-interfaces-em-namespace-interfaces.md");
    }

    #endregion

    #region Mixed Types

    [Fact]
    public void MixedTypes_OnlyInterfaceOutsideShouldFail()
    {
        // Arrange
        LogArrange("Creating mix of types in same file");
        var rule = new CS001_InterfacesInInterfacesNamespaceRule();
        var source = """
            namespace MyProject.Services;
            public interface IBadInterface { }
            public sealed class GoodClass { }
            public enum GoodEnum { A, B }
            """;
        var compilations = CreateCompilations(source, "src/Services/Mixed.cs");

        // Act
        LogAct("Analyzing mixed types");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying only interface fails");
        var interfaceResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "IBadInterface");
        interfaceResult.ShouldNotBeNull();
        interfaceResult.Status.ShouldBe(TypeAnalysisStatus.Failed);

        var classResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "GoodClass");
        classResult.ShouldNotBeNull();
        classResult.Status.ShouldBe(TypeAnalysisStatus.Passed);

        var enumResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "GoodEnum");
        enumResult.ShouldNotBeNull();
        enumResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    [Fact]
    public void MultipleInterfaces_AllInInterfacesFolder_ShouldPass()
    {
        // Arrange
        LogArrange("Creating multiple interfaces in Interfaces/ folder");
        var rule = new CS001_InterfacesInInterfacesNamespaceRule();
        var source = """
            namespace MyProject.Services.Interfaces;
            public interface IServiceA { }
            public interface IServiceB { }
            """;
        var compilations = CreateCompilations(source, "src/Services/Interfaces/Services.cs");

        // Act
        LogAct("Analyzing multiple interfaces in Interfaces/ folder");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying all interfaces pass");
        var resultA = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "IServiceA");
        resultA.ShouldNotBeNull();
        resultA.Status.ShouldBe(TypeAnalysisStatus.Passed);

        var resultB = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "IServiceB");
        resultB.ShouldNotBeNull();
        resultB.Status.ShouldBe(TypeAnalysisStatus.Passed);
    }

    #endregion

    #region Edge Cases — File Path Variations

    [Fact]
    public void InterfaceWithBackslashPath_InInterfacesFolder_ShouldPass()
    {
        // Arrange
        LogArrange("Creating interface with backslash path");
        var rule = new CS001_InterfacesInInterfacesNamespaceRule();
        var source = """
            namespace MyProject.Passwords.Interfaces;
            public interface IHasher { }
            """;
        // Simulate Windows-style path (backslashes)
        var compilations = CreateCompilations(source,
            "src\\Passwords\\Interfaces\\IHasher.cs");

        // Act
        LogAct("Analyzing interface with backslash path");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying backslash path is normalized and passes");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "IHasher");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Passed);
        typeResult.Violation.ShouldBeNull();
    }

    [Fact]
    public void InterfaceWithInterfacesInFileName_NotInFolder_ShouldFail()
    {
        // Arrange
        LogArrange("Creating interface with 'Interfaces' in filename but not in folder");
        var rule = new CS001_InterfacesInInterfacesNamespaceRule();
        var source = """
            namespace MyProject.Services;
            public interface IInterfacesManager { }
            """;
        // "Interfaces" is in the type name, not in the folder path
        var compilations = CreateCompilations(source,
            "src/Services/IInterfacesManager.cs");

        // Act
        LogAct("Analyzing interface with misleading name");
        var results = rule.Analyze(compilations, "/repo");

        // Assert
        LogAssert("Verifying interface not in Interfaces/ folder fails even with misleading name");
        var typeResult = results[0].TypeResults.FirstOrDefault(t => t.TypeName == "IInterfacesManager");
        typeResult.ShouldNotBeNull();
        typeResult.Status.ShouldBe(TypeAnalysisStatus.Failed);
    }

    #endregion

    #region Helpers

    private static Dictionary<string, Compilation> CreateCompilations(string source, string filePath)
    {
        return new Dictionary<string, Compilation>
        {
            ["TestProject"] = CreateSingleCompilation(source, "TestProject", filePath)
        };
    }

    private static Compilation CreateSingleCompilation(string source, string assemblyName, string filePath)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, path: filePath);
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
        };

        return CSharpCompilation.Create(
            assemblyName,
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    #endregion
}
