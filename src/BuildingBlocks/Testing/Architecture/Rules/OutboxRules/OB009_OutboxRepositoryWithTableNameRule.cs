using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.OutboxRules;

/// <summary>
/// OB-009: ConfigureInternal de repositorios outbox deve chamar WithTableName
/// para definir o nome da tabela com prefixo do BC (ex: "auth_outbox").
/// </summary>
public sealed class OB009_OutboxRepositoryWithTableNameRule : OutboxRuleBase
{
    public override string Name => "OB009_OutboxRepositoryWithTableName";

    public override string Description =>
        "ConfigureInternal de repositorios outbox deve chamar WithTableName (OB-009).";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath => "docs/adrs/outbox/OB-009-naming-convention-tabela-outbox-por-bc.md";

    protected override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;

        if (type.IsAbstract || type.TypeKind != TypeKind.Class)
            return null;

        if (IsBuildingBlockProject(context.ProjectName))
            return null;

        if (!InheritsFromBaseClass(type, OutboxRepositoryBaseName))
            return null;

        var configureMethod = type.GetMembers("ConfigureInternal")
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m => m.IsOverride);

        if (configureMethod is null)
            return null;

        var syntaxRef = configureMethod.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxRef is null)
            return null;

        var methodSyntax = syntaxRef.GetSyntax();
        var hasWithTableName = methodSyntax.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Any(inv =>
            {
                var expr = inv.Expression;
                return expr switch
                {
                    MemberAccessExpressionSyntax memberAccess
                        => memberAccess.Name.Identifier.Text == "WithTableName",
                    IdentifierNameSyntax identifier
                        => identifier.Identifier.Text == "WithTableName",
                    _ => false
                };
            });

        if (hasWithTableName)
            return null;

        return new Violation
        {
            Rule = Name,
            Severity = DefaultSeverity,
            Adr = AdrPath,
            Project = context.ProjectName,
            File = context.RelativeFilePath,
            Line = context.LineNumber,
            Message = $"Classe '{type.Name}' nao chama WithTableName em ConfigureInternal. " +
                      $"Cada BC deve definir o nome da tabela outbox com prefixo (ex: \"auth_outbox\").",
            LlmHint = $"Adicionar 'options.WithTableName(\"{{bc}}_outbox\")' no corpo de " +
                      $"ConfigureInternal de '{type.Name}'. Consulte a ADR OB-009."
        };
    }
}
