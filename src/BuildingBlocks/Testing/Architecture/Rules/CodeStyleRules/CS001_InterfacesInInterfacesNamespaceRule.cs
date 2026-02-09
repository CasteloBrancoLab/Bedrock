using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.CodeStyleRules;

/// <summary>
/// Regra CS-001: Toda interface (I*.cs) deve residir em uma subpasta Interfaces/
/// refletindo no namespace. Exemplo: Passwords/Interfaces/IPasswordHasher.cs
/// com namespace ...Passwords.Interfaces.
/// </summary>
public sealed class CS001_InterfacesInInterfacesNamespaceRule : Rule
{
    public override string Name => "CS001_InterfacesInInterfacesNamespace";
    public override string Description =>
        "Interfaces devem residir em subpasta Interfaces/ refletindo no namespace (CS-001)";
    public override Severity DefaultSeverity => Severity.Error;
    public override string AdrPath =>
        "docs/adrs/code-style/CS-001-interfaces-em-namespace-interfaces.md";

    protected override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;

        // Aplica-se apenas a interfaces
        if (type.TypeKind != TypeKind.Interface)
            return null;

        // Verificar se o arquivo está em uma pasta Interfaces/
        var filePath = context.RelativeFilePath.Replace('\\', '/');
        var directory = filePath.Contains('/')
            ? filePath[..filePath.LastIndexOf('/')]
            : string.Empty;

        bool isInInterfacesFolder = directory.EndsWith("/Interfaces", StringComparison.Ordinal) ||
                                    directory.Contains("/Interfaces/", StringComparison.Ordinal);

        if (isInInterfacesFolder)
            return null;

        // Verificar se o namespace contém .Interfaces
        var namespaceName = type.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        bool hasInterfacesNamespace = namespaceName.EndsWith(".Interfaces", StringComparison.Ordinal) ||
                                     namespaceName.Contains(".Interfaces.", StringComparison.Ordinal);

        if (hasInterfacesNamespace)
            return null;

        return new Violation
        {
            Rule = Name,
            Severity = DefaultSeverity,
            Adr = AdrPath,
            Project = context.ProjectName,
            File = context.RelativeFilePath,
            Line = context.LineNumber,
            Message = $"Interface '{type.Name}' não está em subpasta Interfaces/ " +
                      $"(arquivo: {context.RelativeFilePath})",
            LlmHint = $"Mover '{type.Name}' para uma subpasta Interfaces/ " +
                      $"e atualizar o namespace para incluir .Interfaces"
        };
    }
}
