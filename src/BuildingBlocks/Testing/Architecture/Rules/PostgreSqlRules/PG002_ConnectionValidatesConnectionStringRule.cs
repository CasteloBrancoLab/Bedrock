using Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.PostgreSqlRules;

/// <summary>
/// PG-002: O metodo ConfigureInternal de cada Connection (que herda PostgreSqlConnectionBase)
/// deve validar a connection string usando ThrowIfNullOrWhiteSpace ou ThrowIfNullOrEmpty
/// antes de configura-la. Conexoes sem validacao podem causar erros silenciosos
/// ou dificeis de diagnosticar em runtime.
/// </summary>
public sealed class PG002_ConnectionValidatesConnectionStringRule : InfrastructureTypeRuleBase
{
    private const string ConnectionBaseName = "PostgreSqlConnectionBase";
    private const string ConfigureInternalMethodName = "ConfigureInternal";

    private static readonly string[] ValidationMethods =
    [
        "ThrowIfNullOrWhiteSpace",
        "ThrowIfNullOrEmpty"
    ];

    public override string Category => "PostgreSQL";

    public override string Name => "PG002_ConnectionValidatesConnectionString";

    public override string Description =>
        "ConfigureInternal da Connection deve validar connection string com ThrowIfNullOrWhiteSpace/ThrowIfNullOrEmpty (PG-002).";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath => "docs/adrs/postgresql/PG-002-connection-validar-connectionstring.md";

    protected override Violation? AnalyzeType(TypeContext context) => null;

    public override IReadOnlyList<RuleAnalysisResult> Analyze(
        IReadOnlyDictionary<string, Compilation> compilations,
        string rootDir)
    {
        var results = new List<RuleAnalysisResult>();

        foreach (var (projectName, compilation) in compilations)
        {
            if (!IsInfraDataTechProject(projectName))
                continue;

            var typeResults = new List<TypeAnalysisResult>();
            var assemblySymbol = compilation.Assembly;
            var allTypes = GetAllNamedTypes(compilation.GlobalNamespace);

            var ownTypes = allTypes
                .Where(t => SymbolEqualityComparer.Default.Equals(t.ContainingAssembly, assemblySymbol))
                .ToList();

            var connections = ownTypes
                .Where(t => t.TypeKind == TypeKind.Class && !t.IsAbstract && !t.IsStatic)
                .Where(t => InheritsFromConnectionBase(t))
                .ToList();

            foreach (var connection in connections)
            {
                var location = connection.Locations.FirstOrDefault(l => l.IsInSource);
                var filePath = location is not null ? GetRelativePath(location.GetLineSpan().Path, rootDir) : "";
                var lineNumber = location is not null ? location.GetLineSpan().StartLinePosition.Line + 1 : 1;

                var validates = ConfigureInternalValidatesConnectionString(connection);

                if (validates)
                {
                    typeResults.Add(new TypeAnalysisResult
                    {
                        TypeName = connection.Name,
                        TypeFullName = connection.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        File = filePath,
                        Line = lineNumber,
                        Status = TypeAnalysisStatus.Passed,
                        Violation = null
                    });
                }
                else
                {
                    typeResults.Add(new TypeAnalysisResult
                    {
                        TypeName = connection.Name,
                        TypeFullName = connection.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        File = filePath,
                        Line = lineNumber,
                        Status = TypeAnalysisStatus.Failed,
                        Violation = new Violation
                        {
                            Rule = Name,
                            Severity = DefaultSeverity,
                            Adr = AdrPath,
                            Project = projectName,
                            File = filePath,
                            Line = lineNumber,
                            Message = $"Connection '{connection.Name}' em '{projectName}': " +
                                      $"ConfigureInternal nao valida connection string com ThrowIfNullOrWhiteSpace/ThrowIfNullOrEmpty.",
                            LlmHint = $"Na connection '{connection.Name}', adicione " +
                                      $"ArgumentException.ThrowIfNullOrWhiteSpace(connectionString) " +
                                      $"antes de chamar options.WithConnectionString(). " +
                                      $"Consulte a ADR PG-002."
                        }
                    });
                }
            }

            results.Add(new RuleAnalysisResult
            {
                RuleCategory = Category,
                RuleName = Name,
                RuleDescription = Description,
                DefaultSeverity = DefaultSeverity,
                AdrPath = AdrPath,
                ProjectName = projectName,
                TypeResults = typeResults
            });
        }

        return results;
    }

    private static bool InheritsFromConnectionBase(INamedTypeSymbol type)
    {
        var baseType = type.BaseType;
        while (baseType is not null)
        {
            if (baseType.Name == ConnectionBaseName)
                return true;
            baseType = baseType.BaseType;
        }
        return false;
    }

    private static bool ConfigureInternalValidatesConnectionString(INamedTypeSymbol connection)
    {
        var configureMethod = connection.GetMembers()
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m => m.Name == ConfigureInternalMethodName && m.IsOverride);

        if (configureMethod is null)
            return false;

        var syntaxRef = configureMethod.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxRef is null)
            return false;

        var methodSyntax = syntaxRef.GetSyntax() as MethodDeclarationSyntax;
        if (methodSyntax is null)
            return false;

        var bodyNodes = methodSyntax.Body is not null
            ? methodSyntax.Body.DescendantNodes()
            : methodSyntax.ExpressionBody?.DescendantNodes() ?? [];

        var invocations = bodyNodes.OfType<InvocationExpressionSyntax>();

        foreach (var invocation in invocations)
        {
            var methodName = ExtractMethodName(invocation);
            if (methodName is not null && ValidationMethods.Contains(methodName))
                return true;
        }

        return false;
    }

    private static string? ExtractMethodName(InvocationExpressionSyntax invocation)
    {
        return invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,
            IdentifierNameSyntax identifier => identifier.Identifier.Text,
            _ => null
        };
    }
}
