using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.PostgreSqlRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Testing.Architecture.Rules.PostgreSqlRules;

public class PG001_MapBinaryImporterWritesAllColumnsRuleTests : TestBase
{
    public PG001_MapBinaryImporterWritesAllColumnsRuleTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    #region Rule Properties

    [Fact]
    public void RuleProperties_ShouldBeCorrect()
    {
        // Arrange
        LogArrange("Creating rule to verify properties");

        // Act
        LogAct("Reading rule properties");
        var rule = new PG001_MapBinaryImporterWritesAllColumnsRule();

        // Assert
        LogAssert("Verifying rule properties");
        rule.Name.ShouldBe("PG001_MapBinaryImporterWritesAllColumns");
        rule.Description.ShouldContain("MapBinaryImporter");
        rule.DefaultSeverity.ShouldBe(Severity.Error);
        rule.AdrPath.ShouldBe("docs/adrs/postgresql/PG-001-binary-importer-todas-colunas.md");
        rule.Category.ShouldBe("PostgreSQL");
    }

    #endregion

    #region Non Infra.Data.Tech Projects Should Be Ignored

    [Theory]
    [InlineData("ShopDemo.Auth.Domain")]
    [InlineData("Bedrock.BuildingBlocks.Core")]
    public void NonInfraDataTechProject_ShouldBeIgnored(string projectName)
    {
        // Arrange
        LogArrange($"Testing non-Infra.Data.Tech project '{projectName}'");
        var rule = new PG001_MapBinaryImporterWritesAllColumnsRule();
        var source = CreateValidMapperSource(4);
        var compilations = CreateCompilationWithSource(projectName, source);

        // Act
        LogAct("Analyzing project");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying project is ignored");
        results.ShouldBeEmpty();
    }

    #endregion

    #region Correct Write Count Should Pass

    [Fact]
    public void Mapper_WithCorrectWriteCount_ShouldPass()
    {
        // Arrange
        LogArrange("Creating mapper with 14 Write calls (10 base + 4 MapColumn)");
        var rule = new PG001_MapBinaryImporterWritesAllColumnsRule();
        var source = CreateValidMapperSource(4);
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing mapper with correct write count");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying no violations");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .ToList();
        violations.ShouldBeEmpty();

        var passed = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Status == TypeAnalysisStatus.Passed)
            .ToList();
        passed.Count.ShouldBe(1);
    }

    #endregion

    #region Too Few Writes Should Violate

    [Fact]
    public void Mapper_WithTooFewWrites_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating mapper with 12 Write calls but 4 MapColumn (expected 14)");
        var rule = new PG001_MapBinaryImporterWritesAllColumnsRule();
        var source = CreateMapperWithMismatchedWrites(4, 12);
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing mapper with too few writes");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for mismatched write count");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("12");
        violations[0].Message.ShouldContain("14");
    }

    #endregion

    #region Too Many Writes Should Violate

    [Fact]
    public void Mapper_WithTooManyWrites_ShouldGenerateViolation()
    {
        // Arrange
        LogArrange("Creating mapper with 16 Write calls but 4 MapColumn (expected 14)");
        var rule = new PG001_MapBinaryImporterWritesAllColumnsRule();
        var source = CreateMapperWithMismatchedWrites(4, 16);
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing mapper with too many writes");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying violation for mismatched write count");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();
        violations.Count.ShouldBe(1);
        violations[0].Message.ShouldContain("16");
        violations[0].Message.ShouldContain("14");
    }

    #endregion

    #region AutoMapColumns Should Be Skipped

    [Fact]
    public void Mapper_WithAutoMapColumns_ShouldPass()
    {
        // Arrange
        LogArrange("Creating mapper with AutoMapColumns (cannot count statically)");
        var rule = new PG001_MapBinaryImporterWritesAllColumnsRule();
        var source = """
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Mappers
            {
                public class Importer { public void Write(object val, int type) { } }
                public class MapperOptions
                {
                    public MapperOptions AutoMapColumns() => this;
                    public MapperOptions MapTable(string s, string n) => this;
                }
                public abstract class DataModelMapperBase<T>
                {
                    protected abstract void ConfigureInternal(MapperOptions options);
                    public abstract void MapBinaryImporter(Importer importer, T model);
                }
                public sealed class UserDataModelMapper : DataModelMapperBase<object>
                {
                    protected override void ConfigureInternal(MapperOptions options)
                    {
                        options.MapTable("public", "users").AutoMapColumns();
                    }
                    public override void MapBinaryImporter(Importer importer, object model)
                    {
                        importer.Write(model, 1);
                        importer.Write(model, 2);
                    }
                }
            }
            """;
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing mapper with AutoMapColumns");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying AutoMapColumns mapper is skipped");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .ToList();
        violations.ShouldBeEmpty();
    }

    #endregion

    #region Zero MapColumn With Only Base Writes Should Pass

    [Fact]
    public void Mapper_WithZeroMapColumn_And10Writes_ShouldPass()
    {
        // Arrange
        LogArrange("Creating mapper with 0 MapColumn and 10 Write calls (base only)");
        var rule = new PG001_MapBinaryImporterWritesAllColumnsRule();
        var source = CreateValidMapperSource(0);
        var compilations = CreateCompilationWithSource("ShopDemo.Auth.Infra.Data.PostgreSql", source);

        // Act
        LogAct("Analyzing mapper with base-only writes");
        var results = rule.Analyze(compilations, Path.GetTempPath());

        // Assert
        LogAssert("Verifying no violations for base-only writes");
        var violations = results
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .ToList();
        violations.ShouldBeEmpty();
    }

    #endregion

    #region Helpers

    private static string CreateValidMapperSource(int mapColumnCount)
    {
        var mapColumns = string.Join("\n                        ",
            Enumerable.Range(0, mapColumnCount).Select(i => $".MapColumn(\"Col{i}\")"));

        var totalWrites = 10 + mapColumnCount;
        var writes = string.Join("\n                        ",
            Enumerable.Range(0, totalWrites).Select(i => $"importer.Write(model, {i});"));

        return $$"""
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Mappers
            {
                public class Importer { public void Write(object val, int type) { } }
                public class MapperOptions
                {
                    public MapperOptions MapTable(string s, string n) => this;
                    public MapperOptions MapColumn(string name) => this;
                }
                public abstract class DataModelMapperBase<T>
                {
                    protected abstract void ConfigureInternal(MapperOptions options);
                    public abstract void MapBinaryImporter(Importer importer, T model);
                }
                public sealed class UserDataModelMapper : DataModelMapperBase<object>
                {
                    protected override void ConfigureInternal(MapperOptions options)
                    {
                        options
                            .MapTable("public", "users")
                            {{mapColumns}};
                    }
                    public override void MapBinaryImporter(Importer importer, object model)
                    {
                        {{writes}}
                    }
                }
            }
            """;
    }

    private static string CreateMapperWithMismatchedWrites(int mapColumnCount, int writeCount)
    {
        var mapColumns = string.Join("\n                        ",
            Enumerable.Range(0, mapColumnCount).Select(i => $".MapColumn(\"Col{i}\")"));

        var writes = string.Join("\n                        ",
            Enumerable.Range(0, writeCount).Select(i => $"importer.Write(model, {i});"));

        return $$"""
            namespace ShopDemo.Auth.Infra.Data.PostgreSql.Mappers
            {
                public class Importer { public void Write(object val, int type) { } }
                public class MapperOptions
                {
                    public MapperOptions MapTable(string s, string n) => this;
                    public MapperOptions MapColumn(string name) => this;
                }
                public abstract class DataModelMapperBase<T>
                {
                    protected abstract void ConfigureInternal(MapperOptions options);
                    public abstract void MapBinaryImporter(Importer importer, T model);
                }
                public sealed class UserDataModelMapper : DataModelMapperBase<object>
                {
                    protected override void ConfigureInternal(MapperOptions options)
                    {
                        options
                            .MapTable("public", "users")
                            {{mapColumns}};
                    }
                    public override void MapBinaryImporter(Importer importer, object model)
                    {
                        {{writes}}
                    }
                }
            }
            """;
    }

    private static Dictionary<string, Compilation> CreateCompilationWithSource(string projectName, string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, path: "TestFile.cs");
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
        };

        return new Dictionary<string, Compilation>
        {
            [projectName] = CSharpCompilation.Create(
                projectName,
                [syntaxTree],
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
        };
    }

    #endregion
}
