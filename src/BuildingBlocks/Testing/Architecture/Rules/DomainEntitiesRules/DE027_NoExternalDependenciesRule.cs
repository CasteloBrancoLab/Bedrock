using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-027: Entidades de domínio NÃO devem ter dependências externas (repositórios,
/// serviços, configurações, etc.).
/// <para>
/// O que é verificado:
/// <list type="bullet">
///   <item>Campos (fields) de instância NÃO devem ser de tipo interface</item>
///   <item>Entidades devem ser autocontidas, recebendo dados via parâmetros</item>
/// </list>
/// </para>
/// <para>
/// Exceções (não verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estáticas, records, enums, interfaces, structs</item>
///   <item>Campos estáticos (metadados, constantes)</item>
///   <item>Campos gerados pelo compilador (backing fields, etc.)</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE027_NoExternalDependenciesRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE027_NoExternalDependencies";

    public override string Description =>
        "Entidades de domínio não devem ter dependências externas (interfaces injetadas) (DE-027)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-027-entidades-nao-tem-dependencias-externas.md";

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        foreach (var member in type.GetMembers())
        {
            if (member is not IFieldSymbol field)
                continue;

            // Ignorar campos estáticos (metadados, constantes)
            if (field.IsStatic)
                continue;

            // Ignorar campos gerados pelo compilador (backing fields, etc.)
            if (field.IsImplicitlyDeclared)
                continue;

            // Verificar se o tipo do campo é uma interface
            if (field.Type.TypeKind == TypeKind.Interface)
            {
                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = GetFieldLineNumber(field, context.LineNumber),
                    Message = $"A classe '{type.Name}' possui campo '{field.Name}' do tipo " +
                              $"interface '{field.Type.Name}'. Entidades de domínio não devem " +
                              $"ter dependências externas (repositórios, serviços, configurações). " +
                              $"Dados devem ser passados via parâmetros (Input Objects)",
                    LlmHint = $"Remover o campo '{field.Name}' do tipo '{field.Type.Name}' da " +
                              $"classe '{type.Name}'. Se a lógica depende de serviço externo, " +
                              $"mover para Application Service que chama a entidade com dados " +
                              $"já resolvidos. Consultar ADR DE-027 para exemplos"
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Obtém o número da linha onde o campo é declarado.
    /// </summary>
    private static int GetFieldLineNumber(IFieldSymbol field, int fallbackLineNumber)
    {
        var location = field.Locations.FirstOrDefault(l => l.IsInSource);
        if (location is null)
            return fallbackLineNumber;

        return location.GetLineSpan().StartLinePosition.Line + 1;
    }
}
