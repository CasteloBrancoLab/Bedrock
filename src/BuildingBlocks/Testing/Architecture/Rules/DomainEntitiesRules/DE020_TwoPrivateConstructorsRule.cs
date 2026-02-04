using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-020: Entidades devem ter exatamente dois construtores privados:
/// um vazio (para validação incremental via RegisterNew) e um completo
/// (para reconstitution via CreateFromExistingInfo e Clone).
/// <para>
/// O que é verificado:
/// <list type="bullet">
///   <item>Entidades devem ter exatamente 2 construtores de instância</item>
///   <item>Ambos devem ser privados</item>
///   <item>Um deve ser sem parâmetros (vazio)</item>
///   <item>Um deve ter parâmetros (completo)</item>
/// </list>
/// </para>
/// <para>
/// Exceções (não verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estáticas, records, enums, interfaces, structs</item>
///   <item>Construtores estáticos (não são construtores de instância)</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE020_TwoPrivateConstructorsRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE020_TwoPrivateConstructors";

    public override string Description =>
        "Entidades devem ter exatamente dois construtores privados: vazio e completo (DE-020)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-020-dois-construtores-privados.md";

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        // Coletar construtores de instância (não estáticos)
        var instanceConstructors = CollectInstanceConstructors(type);

        // Verificar quantidade
        if (instanceConstructors.Count != 2)
        {
            return new Violation
            {
                Rule = Name,
                Severity = DefaultSeverity,
                Adr = AdrPath,
                Project = context.ProjectName,
                File = context.RelativeFilePath,
                Line = context.LineNumber,
                Message = $"A classe '{type.Name}' tem {instanceConstructors.Count} construtor(es) de instância, " +
                          $"mas deveria ter exatamente 2: um vazio (para RegisterNew) e um completo " +
                          $"(para CreateFromExistingInfo/Clone)",
                LlmHint = $"Garantir que a classe '{type.Name}' tenha exatamente 2 construtores privados: " +
                          $"'private {type.Name}()' (vazio) e " +
                          $"'private {type.Name}(EntityInfo entityInfo, ...)' (completo). " +
                          $"Consultar ADR DE-020 para exemplos de uso correto"
            };
        }

        // Verificar que ambos são privados
        foreach (var ctor in instanceConstructors)
        {
            if (ctor.DeclaredAccessibility != Accessibility.Private)
            {
                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = GetConstructorLineNumber(ctor, context.LineNumber),
                    Message = $"O construtor da classe '{type.Name}' com {ctor.Parameters.Length} parâmetro(s) " +
                              $"tem acessibilidade '{ctor.DeclaredAccessibility}', mas deveria ser 'Private'. " +
                              $"Todos os construtores de entidades devem ser privados para garantir encapsulamento",
                    LlmHint = $"Alterar o construtor da classe '{type.Name}' com {ctor.Parameters.Length} " +
                              $"parâmetro(s) para 'private'. Construtores públicos permitem criação de " +
                              $"estado inválido, violando o encapsulamento. " +
                              $"Consultar ADR DE-020 para fundamentação"
                };
            }
        }

        // Verificar que há um sem parâmetros e um com parâmetros
        var hasParameterless = false;
        var hasWithParameters = false;

        foreach (var ctor in instanceConstructors)
        {
            if (ctor.Parameters.Length == 0)
                hasParameterless = true;
            else
                hasWithParameters = true;
        }

        if (!hasParameterless)
        {
            return new Violation
            {
                Rule = Name,
                Severity = DefaultSeverity,
                Adr = AdrPath,
                Project = context.ProjectName,
                File = context.RelativeFilePath,
                Line = context.LineNumber,
                Message = $"A classe '{type.Name}' não possui construtor vazio (sem parâmetros). " +
                          $"O construtor vazio é necessário para validação incremental via RegisterNew",
                LlmHint = $"Adicionar construtor 'private {type.Name}() {{ }}' na classe '{type.Name}'. " +
                          $"O construtor vazio permite criar instância antes da validação incremental. " +
                          $"Consultar ADR DE-020 para exemplos de uso correto"
            };
        }

        if (!hasWithParameters)
        {
            return new Violation
            {
                Rule = Name,
                Severity = DefaultSeverity,
                Adr = AdrPath,
                Project = context.ProjectName,
                File = context.RelativeFilePath,
                Line = context.LineNumber,
                Message = $"A classe '{type.Name}' não possui construtor completo (com parâmetros). " +
                          $"O construtor completo é necessário para reconstitution via CreateFromExistingInfo e Clone",
                LlmHint = $"Adicionar construtor 'private {type.Name}(EntityInfo entityInfo, ...)' " +
                          $"na classe '{type.Name}' que recebe todos os dados e atribui diretamente " +
                          $"SEM validação. Consultar ADR DE-020 para exemplos de uso correto"
            };
        }

        return null;
    }

    /// <summary>
    /// Coleta todos os construtores de instância (excluindo estáticos e implícitos).
    /// </summary>
    private static List<IMethodSymbol> CollectInstanceConstructors(INamedTypeSymbol type)
    {
        var constructors = new List<IMethodSymbol>();

        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            // Apenas construtores de instância
            if (method.MethodKind != MethodKind.Constructor)
                continue;

            // Ignorar construtores implícitos gerados pelo compilador
            if (method.IsImplicitlyDeclared)
                continue;

            constructors.Add(method);
        }

        return constructors;
    }

    /// <summary>
    /// Obtém o número da linha onde o construtor é declarado, ou fallback para a linha da classe.
    /// </summary>
    private static int GetConstructorLineNumber(IMethodSymbol constructor, int fallbackLineNumber)
    {
        var location = constructor.Locations.FirstOrDefault(l => l.IsInSource);
        if (location is null)
            return fallbackLineNumber;

        return location.GetLineSpan().StartLinePosition.Line + 1;
    }
}
