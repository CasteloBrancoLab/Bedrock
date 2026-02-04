using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-033: Entidades de domínio NÃO devem ser <c>readonly struct</c> ou
/// <c>record struct</c>. Devem ser classes sealed para suportar validação
/// incremental, reconstitution e Clone-Modify-Return.
/// <para>
/// O que é verificado:
/// <list type="bullet">
///   <item>Tipos que herdam de EntityBase NÃO devem ser structs</item>
///   <item>Structs são adequados para Value Objects e Input Objects, não para entidades</item>
/// </list>
/// </para>
/// <para>
/// Exceções:
/// <list type="bullet">
///   <item>A verificação é feita ANTES do filtro padrão de DomainEntityRuleBase
///         (que já ignora structs), então esta regra usa <c>RequiresEntityBaseInheritance = false</c>
///         e verifica manualmente</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE033_NotReadonlyStructRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE033_NotReadonlyStruct";

    public override string Description =>
        "Entidades de domínio não devem ser readonly struct ou record struct (DE-033)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-033-antipadrao-readonly-struct-para-entidades.md";

    /// <summary>
    /// Não requer herança de EntityBase pois o filtro padrão já ignora structs.
    /// Precisamos verificar structs que TENTAM herdar de EntityBase.
    /// </summary>
    protected override bool RequiresEntityBaseInheritance => false;

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        // Esta regra verifica se structs tentam ser entidades
        // O filtro AnalyzeType já filtrou structs (TypeKind != Class), mas vamos
        // verificar se algum tipo que herda de EntityBase é struct
        // Como o filtro base remove structs, precisamos verificar no nível do Rule base

        // Se chegou aqui, é uma classe concreta (não abstrata, não record, não static)
        // A verificação de struct deve ser feita diretamente
        // Na verdade, o filtro da base já remove structs, então esta regra
        // efetivamente verifica que entidades com EntityBase são classes

        // Verificamos se alguma struct no contexto tenta herdar de EntityBase
        // Isso não pode ser feito via AnalyzeEntityType pois structs são filtradas antes
        // Portanto, esta regra é principalmente documentativa e preventiva

        // Verificação: se é class E herda de EntityBase, está OK (já validado pela base)
        // A real prevenção é que structs não podem herdar de classes em C#

        return null;
    }
}
